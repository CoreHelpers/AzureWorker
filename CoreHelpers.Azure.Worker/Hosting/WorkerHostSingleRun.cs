using System;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public class WorkerHostSingleRun : WorkerHost
	{
		public WorkerHostSingleRun(IServiceCollection serviceCollection) 
		: base(serviceCollection)
		{}
	}
}
