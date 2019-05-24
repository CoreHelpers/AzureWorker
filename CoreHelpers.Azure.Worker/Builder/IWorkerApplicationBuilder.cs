using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Builder
{
    public interface IWorkerApplicationBuilder
    {		
    	IServiceProvider ApplicationServices { get;  }
    	
    	ICollection<Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task>> RegisteredMiddleWares { get; }
    	
    	ICollection<Func<WorkerApplicationOperation, Task>> RegisteredFinalizedMiddleWares { get; set; }
    	
        ICollection<Func<WorkerApplicationOperation, Exception, Task>> RegisteredErrorMiddleWares { get; } 

		ICollection<Func<WorkerApplicationOperation, Task>> RegisteredTimeoutMiddleWares { get; } 
    	
		IWorkerApplicationBuilder Use(Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task> middleware);

		IWorkerApplicationBuilder UseOnFinalize(Func<WorkerApplicationOperation, Task> middleware);
		
        IWorkerApplicationBuilder UseOnError(Func<WorkerApplicationOperation, Exception, Task> middleware);

		IWorkerApplicationBuilder UseOnTimeout(Func<WorkerApplicationOperation, Task> middleware);
    }
}
