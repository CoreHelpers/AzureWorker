using System;
using CoreHelpers.Azure.Worker.AppServices;
using CoreHelpers.Azure.Worker.Hosting;
using CoreHelpers.Azure.Worker.Logging;

namespace CoreHelpersAzure.Worker.SampleLoop
{
	class Program
	{
		public static void Main(string[] args)
		{
			var host = new WorkerHostBuilder()				
				.UseConsoleLogging()				
				.UseStartup<Startup>()											
				.UsePolling(2000)
                .UseAzureAppServiceShutdownHandler()
				.Build();			
			
			host.Run(TimeSpan.FromSeconds(10));	
			// host.Run(TimeSpan.Zero);  
		}
	}
}
