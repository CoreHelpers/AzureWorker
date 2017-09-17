using System;
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
        
		public void Configure(IWorkerApplicationBuilder app, IWorkerHostingEnvironment env, ILoggerFactory loggerFactory) 
		{
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
		}		
	}
}
