using System;
namespace CoreHelpers.Azure.Worker.Hosting
{
	public interface IWorkerHostPollingControllerService
	{
		void SkipNextPolling();
	}
}
