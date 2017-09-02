using System;
using System.IO;
using System.Reflection;
using CoreHelpers.Azure.Worker.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace CoreHelpers.Azure.Worker.Logging
{
	public static class UseFileLogginExtensions
	{
		public static WorkerHostBuilder UseFileLogging(this WorkerHostBuilder host, string logPath, string definedWorkerName) 
		{
			// build the logfile name 
			var workerName = String.Empty;

			if (String.IsNullOrEmpty(definedWorkerName))
				workerName = String.Format("Worker-{0}-{1}", Environment.MachineName, Assembly.GetEntryAssembly().GetName().Name);
			else
				workerName = String.Format("Worker-{0}-{1}", Environment.MachineName, definedWorkerName);	

			// ensure the log path exists			
			Directory.CreateDirectory(logPath);
			
			// add the logger
			var loggerFactory = host.Services.BuildServiceProvider().GetService<ILoggerFactory>();			
			loggerFactory.AddFile(Path.Combine(logPath, workerName + "-{Date}.log"));
			
			// done 
			return host;
		}
	}
}
