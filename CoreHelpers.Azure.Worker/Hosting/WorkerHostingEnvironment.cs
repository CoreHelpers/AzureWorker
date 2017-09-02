using System;
using System.IO;
using System.Reflection;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public class WorkerHostingEnvironment : IWorkerHostingEnvironment
	{
		public string ProcessRootPath { 
			get 
			{
				return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);				
			} 
		}

		public string EnvironmentName { 
			get 
			{
				return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null ? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") : "Production";	
			}
		}

		public bool IsDevelopment()
		{
			return EnvironmentName.Equals("Development");
		}

		public bool IsProduction()
		{
			return EnvironmentName.Equals("Production");
		}

		public bool IsStaging()
		{
			return EnvironmentName.Equals("Staging");
		}
	}
}
