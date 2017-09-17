using CoreHelpers.Azure.Worker.Builder;
using CoreHelpers.Azure.Worker.Hosting;
using CoreHelpers.Azure.Worker.AppServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CoreHelpers.Azure.Worker.Clients;

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
        
		public void Configure(IWorkerApplicationBuilder app, IWorkerHostingEnvironment env, ILoggerFactory loggerFactory) 
		{
			// get the right section
			var configSection = Configuration.GetSection("DemoAccount");
			var queueConfig = new AzureQueueClientConfiguration(configSection.GetValue<string>("Queue"), configSection.GetValue<string>("Account"), configSection.GetValue<string>("Key"));
					
			// Use our middle ware which checks the queue for a new task
    		app.UseStorageQueueProcessor(loggerFactory, queueConfig, async (operation, message, next) => {

				// get a logger
				var logger = loggerFactory.CreateLogger("Processor");

				// log the message 
				logger.LogInformation("Received Message: {0}", message);
				
				// jump to the next middleware
				await next.Invoke();				
    		});   
			
			// When executing this middleware we are skipping the polling because we don't wnat to wait. This works
			// well together witht he job queue message processor which skips all middle ware execution when 
			// no job is arrived
			app.UseSkipPollingMiddleware(loggerFactory); 											
		}			
	}
}
