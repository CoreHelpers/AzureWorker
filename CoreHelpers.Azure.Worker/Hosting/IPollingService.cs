using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public interface IPollingService
	{
		void SkipNextPolling();

        void AbortDuringNextPolling();

		bool Wait(int polling);
	}
}
