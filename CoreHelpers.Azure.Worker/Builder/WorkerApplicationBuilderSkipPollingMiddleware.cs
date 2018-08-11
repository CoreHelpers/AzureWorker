using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CoreHelpers.Azure.Worker.Hosting;

namespace CoreHelpers.Azure.Worker.Builder
{
	public static class WorkerApplicationBuilderSkipPollingMiddleware
	{
		public static IWorkerApplicationBuilder UseSkipPollingMiddleware(this IWorkerApplicationBuilder app, ILoggerFactory loggerFactory) 		
		{			
			// establish logger
			var logger = loggerFactory.CreateLogger("SkipPollingMiddleware");

			// get the polling service
			var pollingService = app.ApplicationServices.GetService<IPollingService>();
			
			// register the middleware
			app.Use(async (WorkerApplicationOperation operation, IWorkerApplicationMiddlewareExecutionController context) =>
			{
				// log
				logger.LogInformation("Skipping next polling...");

				// done
				pollingService.SkipNextPolling();

                // done
                await context.Invoke();
			});							
			
            app.UseOnError(async (WorkerApplicationOperation operation, Exception error) => {
				// log
				logger.LogInformation("Skipping next polling...");

				// done
				pollingService.SkipNextPolling();

				// done
				await Task.CompletedTask;		
			});

			return app;
		}
	}
}
