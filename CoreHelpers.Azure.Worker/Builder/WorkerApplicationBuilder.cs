using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.Builder
{
	public class WorkerApplicationBuilder : IWorkerApplicationBuilder
	{
		public ICollection<Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task>> RegisteredMiddleWares { get; set; }
		public ICollection<Func<Exception, Task>> RegisteredErrorMiddleWares { get; set; }
		public ICollection<Func<WorkerApplicationOperation, Task>> RegisteredFinalizedMiddleWares { get; set; }
		public ICollection<Func<WorkerApplicationOperation, Task>> RegisteredTimeoutMiddleWares { get; set; }

		private IServiceCollection Services { get; set; }
		public IServiceProvider ApplicationServices { get { return Services.BuildServiceProvider(); } }
                
		public WorkerApplicationBuilder(IServiceCollection services) 
		{
			RegisteredMiddleWares = new List<Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task>>();
			RegisteredErrorMiddleWares = new List<Func<Exception, Task>>();	
			RegisteredFinalizedMiddleWares = new List<Func<WorkerApplicationOperation, Task>>();
			RegisteredTimeoutMiddleWares = new List<Func<WorkerApplicationOperation, Task>>();
			Services = services;
		}
		
		public IWorkerApplicationBuilder Use(Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task> middleware)
		{
			RegisteredMiddleWares.Add(middleware);			
			return this;
		}

		public IWorkerApplicationBuilder UseOnError(Func<Exception, Task> middleware)
		{
			RegisteredErrorMiddleWares.Add(middleware);
			return this;
		}

		public IWorkerApplicationBuilder UseOnFinalize(Func<WorkerApplicationOperation, Task> middleware)
		{
			RegisteredFinalizedMiddleWares.Add(middleware);
			return this;
		}

		public IWorkerApplicationBuilder UseOnTimeout(Func<WorkerApplicationOperation, Task> middleware)
		{
			RegisteredTimeoutMiddleWares.Add(middleware);
            return this;
		}
	}
}
