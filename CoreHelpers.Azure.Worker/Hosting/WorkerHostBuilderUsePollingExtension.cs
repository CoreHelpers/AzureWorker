using System;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.Hosting
{
    public static class WorkerHostBuilderUsePollingExtension
    {
    	public static IWorkerHostBuilder UsePolling(this IWorkerHostBuilder hostBuilder, int pollingInterval) 
    	{
			// get the services
			var services = (hostBuilder as WorkerHostBuilder).Services;

			// add the worker
			(hostBuilder as WorkerHostBuilder).Services.AddSingleton(
				typeof(IWorkerHost), 
				new WorkerHostPolling(services, pollingInterval));
			
			// done
			return hostBuilder;
    	}	
    }
}
