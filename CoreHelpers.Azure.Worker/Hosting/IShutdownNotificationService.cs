using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public interface IShutdownNotificationService
	{
        void OnShutdownNotification(Func<Task> action);

        void WaitForAllNotificationHandlers();
	}
}
