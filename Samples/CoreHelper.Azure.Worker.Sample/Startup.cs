using CoreHelpers.Azure.Worker.Builder;
using CoreHelpers.Azure.Worker.Hosting;
using CoreHelpers.Azure.Worker.AppServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CoreHelpers.Azure.Worker.Clients;
using System.Collections.Generic;
using System.Threading;
using System;

namespace CoreHelpers.Azure.Worker.Sample
{
	public class Startup 
	{
		public Startup(IWorkerHostingEnvironment env)
		{ 
			// build the rest
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ProcessRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddJsonFile($"appsettings.demo.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }		
		
		public void ConfigureServices(IServiceCollection services)
        {
        	// Allow to use the configuration in other services
     		services.AddSingleton<IConfiguration>(Configuration);			
        }
        
		public void Configure(IWorkerApplicationBuilder app, IWorkerHostingEnvironment env, ILoggerFactory loggerFactory, IPollingService pollingService) 
		{
			// get the right section
			var configSection = Configuration.GetSection("DemoAccount");
            var queueConfig1 = new AzureQueueClientPriorityConfiguration(configSection.GetValue<string>("Queue"), 1, configSection.GetValue<string>("Account"), configSection.GetValue<string>("Key"));
            var queueConfig2 = new AzureQueueClientPriorityConfiguration(configSection.GetValue<string>("Queue1"), 2, configSection.GetValue<string>("Account"), configSection.GetValue<string>("Key"));

            // Use our middle ware which checks the queue for a new task
            // app.UseStorageQueueProcessor(loggerFactory, new List<AzureQueueClientPriorityConfiguration>() { queueConfig1, queueConfig2 }, async (operation, message, next) => {
			app.UseStorageQueueProcessorNotBlocking(loggerFactory, new List<AzureQueueClientPriorityConfiguration>() { queueConfig1, queueConfig2 }, async (operation, message, next) => {

                // get a logger
                var logger = loggerFactory.CreateLogger("Processor");

				// log the message 
				logger.LogInformation("Received Message: {0}", message);

                // delay
                Thread.Sleep(5000);

				// jump to the next middleware
				await next.Invoke();				
    		});

			app.Use(async (arg1, next) =>
			{
				Console.WriteLine("Stopping at next polling");
				pollingService.AbortDuringNextPolling();

                // abort the polling 
                await next.Invoke();
			});
			
			// When executing this middleware we are skipping the polling because we don't wnat to wait. This works
			// well together witht he job queue message processor which skips all middle ware execution when 
			// no job is arrived
			app.UseSkipPollingMiddleware(loggerFactory); 											
		}			
	}
}
