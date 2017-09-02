﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreHelpers.Azure.Worker.Hosting;
using CoreHelpers.Azure.Worker.Logging;

namespace CoreHelpers.Azure.Worker.AppServices
{
	public static class AppServiceLoggingExtensions
	{
		private static string BuildLogPath() 
		{			
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) 			
				return Environment.ExpandEnvironmentVariables(string.Format("%HOME%/Library/Logs/{0}", Assembly.GetEntryAssembly().GetName().Name));
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return Environment.ExpandEnvironmentVariables(string.Format("%temp%\\{0}\\LogFiles", Assembly.GetEntryAssembly().GetName().Name));
			else 
				return Environment.ExpandEnvironmentVariables(string.Format("/tmp/{0}/LogFiles", Assembly.GetEntryAssembly().GetName().Name));									
		}
		
		public static WorkerHostBuilder UseFileLoggingInAppServices(this WorkerHostBuilder host) 
		{					
			// check if we are running in an Azure App Service
			if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPSETTING_WEBSITE_SITE_NAME")))
			{
				// get the homePath
				string homePath = Environment.GetEnvironmentVariable("HOME");

				// use the assembly name directory
				var webjobName = Environment.GetEnvironmentVariable("WEBJOBS_NAME");
				if (String.IsNullOrEmpty(webjobName))
				{
					string assemblyPath = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetEntryAssembly().CodeBase).Path));
					var pathPortions = assemblyPath.Split(Path.DirectorySeparatorChar);
					webjobName = pathPortions[pathPortions.Length - 1];
				}
				        
        		// start logging on Azure WebSite
				return host.UseFileLogging(Path.Combine(homePath, "LogFiles", "WebJobs"), webjobName);
			}
			else
			{
				// start logging on non Azure WebSite
				return host.UseFileLogging(BuildLogPath(), String.Empty);
			}
		}
	}
}
