using System;
using System.Threading.Tasks;
using CoreHelpers.Azure.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public class WorkerHostPolling : WorkerHost
    {
    	private int Polling { get; set; }
    	
    	public WorkerHostPolling(IServiceCollection serviceCollection, int polling) 
    	: base(serviceCollection)
    	{
			Polling = polling;			
    	}
    	
		public override async Task RunAsync() 
		{
            // get the polling service 
            var pollingService = ServiceCollection.BuildServiceProvider().GetService<IPollingService>();            

			// get the shutdown handler 
			var shutdownService = ServiceCollection.BuildServiceProvider().GetService<IShutdownNotificationService>();

            // register a handler who interupts the next polling 
            var bReceivedShutdownNotification = false;
            shutdownService.OnShutdownNotification(async () =>
            {
                bReceivedShutdownNotification = true;
                pollingService.AbortDuringNextPolling();
                await Task.CompletedTask;
            });
			
			// run our infinite loop with polling
			do
			{                
				// do the basic work 
				await base.RunAsync();
										
			} while (pollingService.Wait(Polling));

            // ensure all shutdown activities are executed gracefully 
            if (bReceivedShutdownNotification)
                shutdownService.WaitForAllNotificationHandlers();
		}        	
    }
}