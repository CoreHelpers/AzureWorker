using System;
using System.Threading.Tasks;
using CoreHelper.Azure.Worker.SampleLoop.Services;
using CoreHelper.Azure.Worker.SampleLoop.Services.Contracts;
using CoreHelpers.Azure.Worker.Builder;
using CoreHelpers.Azure.Worker.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreHelpersAzure.Worker.SampleLoop
{
	public class Startup
	{
		public Startup(IWorkerHostingEnvironment env)
		{ 
			// build the rest
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ProcessRootPath)				
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }		
		
		public void ConfigureServices(IServiceCollection services)
        {
        	// Allow to use the configuration in other services
     		services.AddSingleton<IConfiguration>(Configuration);			
     		
     		// add a scoped service
     		services.AddScoped<IScopedService, ScopedServiceImp>();
        }
        
        public void Configure(IWorkerApplicationBuilder app, IWorkerHostingEnvironment env, ILoggerFactory loggerFactory, IShutdownNotificationService shutdownService, IPollingService pollingService, ITimeoutService timeoutService) 
		{
            shutdownService.OnShutdownNotification(async () =>
            {

                // get a logger
                var logger = loggerFactory.CreateLogger("ShutdownHandler");

                // delay 
                logger.LogInformation("Delaying shutdown by 10 seconds");
                await Task.Delay(5000);

                // done
                logger.LogInformation("Finished delay");
            });

			app.Use((WorkerApplicationOperation operation, IWorkerApplicationMiddlewareExecutionController next) =>
			{
				// get a logger
				var logger = loggerFactory.CreateLogger("Processor");

				// lookup a scoped service 
				var scopedService = operation.Services.GetService<IScopedService>();
															
				// log the message 				
				logger.LogInformation("MW01 - InstanceId: {0}", scopedService.InstanceId);
			
				return next.Invoke();
			});		
			
			app.Use((WorkerApplicationOperation operation, IWorkerApplicationMiddlewareExecutionController next) =>
			{
				
				// get a logger
				var logger = loggerFactory.CreateLogger("Processor");

				// lookup a scoped service 
				var scopedService = operation.Services.GetService<IScopedService>();
															
				// log the message 				
				logger.LogInformation("MW02 - InstanceId: {0}", scopedService.InstanceId);
			
				return next.Invoke();
			});

			/*app.Use((WorkerApplicationOperation operation, IWorkerApplicationMiddlewareExecutionController next) =>
            {
                // get a logger
                var logger = loggerFactory.CreateLogger("AbortNextPolling");

                logger.LogInformation("Abort...");
                pollingService.AbortDuringNextPolling();

                return next.Invoke();
            });*/
			
			app.Use(async (WorkerApplicationOperation operation, IWorkerApplicationMiddlewareExecutionController next) =>
            {

                // get a logger
                var logger = loggerFactory.CreateLogger("Processor");                
                logger.LogInformation("Delaying Job");

				// delay
				logger.LogInformation($"Delay 5sec - {DateTime.Now}");
				await Task.Delay(5000);

				// reset the timeout
				await timeoutService.ResetExecutionTimeout();

				// delay
				logger.LogInformation($"Delay 5sec - {DateTime.Now}");
				await Task.Delay(5000);

				// reset the timeout
				await timeoutService.ResetExecutionTimeout();

				// delay
				logger.LogInformation($"Delay 5sec - {DateTime.Now}");
				await Task.Delay(5000);

				// reset the timeout
				await timeoutService.ResetExecutionTimeout();

				// delay
				logger.LogInformation($"Delay 5sec - {DateTime.Now}");
				await Task.Delay(5000);

				// delay
				logger.LogInformation($"Delay 5sec - {DateTime.Now}");
				await Task.Delay(5000);

				// delay
				logger.LogInformation($"Delay 5sec - {DateTime.Now}");
				await Task.Delay(5000);

				// next 
				await next.Invoke();
            });

			app.UseOnTimeout(async (WorkerApplicationOperation operation) =>
			{
				Console.WriteLine("Timeout Exceeded");
				await Task.CompletedTask;

                // abort
				Console.WriteLine("Aborting Worker");
				pollingService.AbortDuringNextPolling();
			});
		}		
	}
}
