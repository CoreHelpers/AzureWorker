using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public class WorkerHostingEnvironment : IWorkerHostingEnvironment
	{
        private bool? cachedCheckIsRunningInContainerEnvironment { get; set; }

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

        public bool IsRunningInContainerEnvironment() 
        {
            // take the cached value if exists
            if (cachedCheckIsRunningInContainerEnvironment.HasValue)
                return cachedCheckIsRunningInContainerEnvironment.Value;
            
            // check if we running on linux othwisw we can't running in docker
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return false;

            // as we are running on linux get the content of 
            // /proc/1/cgroup
            var cGroupContent = File.ReadAllText("/proc/1/cgroup");

            // set the cache value
            cachedCheckIsRunningInContainerEnvironment = cGroupContent.Contains("/docker/") || cGroupContent.Contains("/kubepods/");

            // return the value 
            return cachedCheckIsRunningInContainerEnvironment.Value;
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
