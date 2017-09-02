﻿using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Builder
{
	public class WorkerApplicationMiddlewareExecutionController : IWorkerApplicationMiddlewareExecutionController
	{
		private bool _skipped { get; set; }

		public WorkerApplicationMiddlewareExecutionController()
		{
			_skipped = false;
		}

		public async Task Invoke()
		{
			_skipped = false;

			await Task.CompletedTask;
		}

		public async Task Skip()
		{
			_skipped = true;

			await Task.CompletedTask;
		}

		public async Task Execute(WorkerApplicationOperation operation, Func<WorkerApplicationOperation, IWorkerApplicationMiddlewareExecutionController, Task> middleware, Func<Task> next, Func<Task> skip)
		{
			await middleware(operation, this);

			if (_skipped)
				await skip.Invoke();
			else
				await next.Invoke();
		}
	}		
}
