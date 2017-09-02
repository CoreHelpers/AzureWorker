using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using CoreHelpers.Azure.Worker.Builder;

namespace CoreHelpers.Azure.Worker.Hosting
{
    public class WorkerHostBuilder : IWorkerHostBuilder
    {    	
    	public IServiceCollection Services { get; set; } = new ServiceCollection();            
    	
    	public WorkerHostBuilder() 
    	{    		    	
    		Services.AddSingleton<IWorkerHostingEnvironment>(new WorkerHostingEnvironment());
    		Services.AddSingleton<IWorkerApplicationBuilder>(new WorkerApplicationBuilder(Services));
    		Services.AddSingleton<ILoggerFactory>(new LoggerFactory());
    		Services.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();    		
    	}
    	
        public IWorkerHost Build() 
        {
			// Lookup the startup type
         	var serviceProvider = Services.BuildServiceProvider();
    		var startupService = serviceProvider.GetService<IStartup>();
			var shutdownSerivce = serviceProvider.GetService<IShutdownNotificationService>();
			var pollingService = serviceProvider.GetService<IPollingService>();

			// register deafult shutdown service         
			if (shutdownSerivce == null)
				Services.AddSingleton<IShutdownNotificationService>(new WorkerHostDummyShutdownNotificationService());

			if (pollingService == null)
				Services.AddSingleton<IPollingService>(new PollingService());
				
        	// should we not found one throw execption 			
			if (startupService == null) { throw new Exception("Please defined the startup type via UseStartup method");  }

			// Lookup the WorkerHost
			var workerHost = serviceProvider.GetService<IWorkerHost>();
			if (workerHost == null) {
				Services.AddSingleton<IWorkerHost>(new WorkerHostSingleRun(Services));				
				workerHost = serviceProvider.GetService<IWorkerHost>();
		  	}						

			// build the host
			return workerHost;
        }
    }
}
