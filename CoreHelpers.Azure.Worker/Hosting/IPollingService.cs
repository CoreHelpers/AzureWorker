using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public interface IPollingService
	{
		void SkipNextPolling();
		
		Task Wait(int polling);
	}
}
