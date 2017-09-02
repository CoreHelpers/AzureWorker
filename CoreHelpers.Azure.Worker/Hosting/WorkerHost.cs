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
		
        public virtual async Task RunAsync() 
        {        	
			if (ApplicationBuilder.RegisteredMiddleWares.Count == 0)
				return;

			try
			{
				// generate the middleware stack
				var stack = new Stack<Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task>>(ApplicationBuilder.RegisteredMiddleWares.Reverse());

				// generate the worker operation context
				var operation = new WorkerApplicationOperation();

				// this executes all the regular middlewares
				await ExecuteNextMiddleWare(stack, operation);											
			} 
			catch(Exception e) 
			{
				var logger = LoggerFactory.CreateLogger("WorkerHost");	
				logger.LogError(new EventId(0), e, "Unhandled exception during middleware execution");

				foreach(var errorMiddleware in ApplicationBuilder.RegisteredErrorMiddleWares) 				
					await errorMiddleware(e);				
			}
        }

		public void Run() 
		{		
			// execute the configuration task 
			ConfigureAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			
			// Trigger the run cycle
			RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		
	}
}
