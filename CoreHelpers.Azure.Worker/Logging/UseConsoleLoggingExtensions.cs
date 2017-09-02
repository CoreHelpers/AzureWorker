using System;
using CoreHelpers.Azure.Worker.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreHelpers.Azure.Worker.Logging
{
	public static class UseConsoleLoggingExtensions
	{
		public static WorkerHostBuilder UseConsoleLogging(this WorkerHostBuilder host) 
		{
			var loggerFactory = host.Services.BuildServiceProvider().GetService<ILoggerFactory>();
			loggerFactory.AddConsole();
			return host;
		}
	}
}
