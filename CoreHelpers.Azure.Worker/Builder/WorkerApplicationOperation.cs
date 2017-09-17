using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.Builder
{
    public class WorkerApplicationOperation : IDisposable
    {
		public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
		
		public IServiceProvider Services { get { return ServiceScope.ServiceProvider; } }

		private IServiceScope ServiceScope { get; set; }
		
		public WorkerApplicationOperation(IServiceScope scope) 
		{
			ServiceScope = scope;	
		}

		public void Dispose()
		{
			if (ServiceScope != null)
			{
				ServiceScope.Dispose();
				ServiceScope = null;
			}
		}
	}
}
