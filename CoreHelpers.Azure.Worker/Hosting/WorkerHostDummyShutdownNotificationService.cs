using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public class WorkerHostDummyShutdownNotificationService : IShutdownNotificationService
	{
		public bool WaitForShutdown(Task parentTask)
		{
			parentTask.Wait();
			return true;
		}
	}
}
