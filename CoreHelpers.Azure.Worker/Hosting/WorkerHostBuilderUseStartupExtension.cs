using System;
using System.Reflection;
using CoreHelpers.Azure.Worker.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public static class WorkerHostBuilderUseStartupExtension
	{
		public static IWorkerHostBuilder UseStartup(this IWorkerHostBuilder hostBuilder, Type startupType)
		{							
		    var startupAssemblyName = startupType.GetTypeInfo().Assembly.GetName().Name;		    		    
		    		   
			if (typeof(IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
			{
				hostBuilder.Services.AddSingleton(typeof(IStartup), startupType);			
			}
			else
			{
				hostBuilder.Services.AddSingleton(typeof(IStartup), sp =>
				{				
					var hostingEnvironment = sp.GetRequiredService<IWorkerHostingEnvironment>();
					return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType, hostingEnvironment.EnvironmentName));
				});
			}
		    		    
			return hostBuilder;
		}

		public static IWorkerHostBuilder UseStartup<T>(this WorkerHostBuilder hostBuilder) 
		{
			return hostBuilder.UseStartup(typeof(T));
        }                
    }
}