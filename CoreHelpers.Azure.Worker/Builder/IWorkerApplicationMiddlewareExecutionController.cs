using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Builder
{
	public interface IWorkerApplicationMiddlewareExecutionController
	{
		Task Invoke();
		Task Skip();

		Task Execute(WorkerApplicationOperation operation, Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task> middleware, Func<Task> next, Func<Task> skip); 
	}
}
