using System;
using CoreHelpers.Azure.Worker.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.AppServices
{
	public static class AppServiceShutdownHandlerExtension
	{
		public static IWorkerHostBuilder UseAzureAppServiceShutdownHandler(this IWorkerHostBuilder hostBuilder) 
    	{
			// get the services
			var services = (hostBuilder as WorkerHostBuilder).Services;

			// add the worker
            (hostBuilder as WorkerHostBuilder).Services.AddSingleton<IShutdownNotificationService>(new AppServiceShutdownNotificationService());
			
			// done
			return hostBuilder;
    	}	
	}
}
