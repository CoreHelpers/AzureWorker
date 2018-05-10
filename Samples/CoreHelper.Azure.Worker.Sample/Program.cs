using System;
using CoreHelpers.Azure.Worker.AppServices;
using CoreHelpers.Azure.Worker.Hosting;
using CoreHelpers.Azure.Worker.Logging;

namespace CoreHelpers.Azure.Worker.Sample
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var host = new WorkerHostBuilder()				
				.UseConsoleLogging()
				.UseFileLoggingInAppServices()
				.UseStartup<Startup>()							
				.UseAzureAppServiceShutdownHandler()
				.UsePolling(500)				
				.Build();			
			
			host.Run(TimeSpan.Zero);	
		}
	}
}
