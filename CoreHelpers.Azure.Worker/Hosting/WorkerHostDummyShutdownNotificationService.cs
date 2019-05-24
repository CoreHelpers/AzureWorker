using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public class WorkerHostDummyShutdownNotificationService : IShutdownNotificationService
	{
		public void OnShutdownNotification(Func<Task> action)
        {}

        public void WaitForAllNotificationHandlers()
        {}
	}
}
