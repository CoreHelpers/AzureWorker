using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public interface IShutdownNotificationService
	{
		// false - abort the loop because shutdown notified
		// true - just parent task are used 
		bool WaitForShutdown(Task parentTask);
	}
}
