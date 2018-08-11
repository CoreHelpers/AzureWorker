﻿using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using CoreHelpers.Azure.Worker.Builder;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CoreHelpers.Azure.Worker.Hosting
{
    public abstract class WorkerHost : IWorkerHost
    {		
		protected IServiceCollection ServiceCollection { get; set; }
		protected IWorkerApplicationBuilder ApplicationBuilder { get; set; }
		protected ILoggerFactory LoggerFactory { get; set; }
		
		public WorkerHost(IServiceCollection serviceCollection) 
    	{			
			ServiceCollection = serviceCollection;
			ApplicationBuilder = ServiceCollection.BuildServiceProvider().GetService<IWorkerApplicationBuilder>();
			LoggerFactory = ServiceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
			if (LoggerFactory == null)
				LoggerFactory = new LoggerFactory(); 
			
    	}
    	
    	public virtual Task ConfigureAsync()
		{	
			// build an instance of the startup type         	         	
    		var startupService = ServiceCollection.BuildServiceProvider().GetService<IStartup>();
					
			// execute the configure service 
			var serviceProvider = startupService.ConfigureServices(ServiceCollection);
						
			// execute the configure method						
			startupService.Configure(ApplicationBuilder);

			// inject our finalize middle ware
			ApplicationBuilder.Use(async (operationFinalize, nextFinalize) =>
			{

				// now we are finalizing the execution
				foreach (var finalizeMiddleware in ApplicationBuilder.RegisteredFinalizedMiddleWares)
					await finalizeMiddleware(operationFinalize);

				// next 
				await nextFinalize.Invoke();
			});

			// return a done task
			return Task.CompletedTask;								     
		}
		
		private async Task ExecuteNextMiddleWare(Stack<Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task>> stack, WorkerApplicationOperation operation) 
		{
			if (stack.Count == 0)
				return;
				
			var middleware = stack.Pop();			

			var executionController = new WorkerApplicationMiddlewareExecutionController();

			await executionController.Execute(operation, middleware, async () => {			
				await (ExecuteNextMiddleWare(stack, operation));
			}, async () => {
				await Task.CompletedTask;
			});			
		}
		
		public virtual async Task RunAsync(TimeSpan executionTimeout) 
        {        	
			if (ApplicationBuilder.RegisteredMiddleWares.Count == 0)
				return;

			try
			{
				// generate the middleware stack
				var stack = new Stack<Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task>>(ApplicationBuilder.RegisteredMiddleWares.Reverse());

				// build the timeout task                 
				var timeoutTask = default(Task);
				if (executionTimeout > TimeSpan.Zero)
					timeoutTask = Task.Delay(executionTimeout);									

				// generate the worker operation context
				using (var operation = new WorkerApplicationOperation(ApplicationBuilder.ApplicationServices.CreateScope()))
				{
                    try
                    {
                        // this executes all the regular middlewares
                        var executionTask = ExecuteNextMiddleWare(stack, operation);

                        // build the wait queue
                        var tasksToWait = new List<Task>();
                        tasksToWait.Add(executionTask);

                        if (timeoutTask != null)
                            tasksToWait.Add(timeoutTask);

                        // wait for all 
                        var waitResult = Task.WaitAny(tasksToWait.ToArray());
                        if (tasksToWait[waitResult] == timeoutTask)
                        {
                            foreach (var timeoutMiddleware in ApplicationBuilder.RegisteredTimeoutMiddleWares)
                                await timeoutMiddleware(operation);
                        }
                        else
                        {
                            var taskFinished = tasksToWait[waitResult];
                            if (taskFinished.Exception != null)
                                throw taskFinished.Exception.InnerException;
                        }
                    } catch(Exception e) {
                        
                        var logger = LoggerFactory.CreateLogger("WorkerHost");  
                        logger.LogError(new EventId(0), e, "Unhandled exception during middleware execution (with operation)");

                        foreach(var errorMiddleware in ApplicationBuilder.RegisteredErrorMiddleWares)               
                            await errorMiddleware(operation, e);             
                    }

				}													
			} 
			catch(Exception e) 
			{
				var logger = LoggerFactory.CreateLogger("WorkerHost");	
				logger.LogError(new EventId(0), e, "Unhandled exception during middleware execution");

				foreach(var errorMiddleware in ApplicationBuilder.RegisteredErrorMiddleWares) 				
                    await errorMiddleware(null, e);				
			}
        }

		public void Run(TimeSpan executionTimeout) 
		{		
			// execute the configuration task 
			ConfigureAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			
			// Trigger the run cycle
			RunAsync(executionTimeout).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public Task RunAsync()
		{
			throw new NotImplementedException();
		}
	}
}
