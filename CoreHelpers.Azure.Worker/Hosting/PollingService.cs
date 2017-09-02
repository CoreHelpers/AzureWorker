using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public class PollingService : IPollingService
	{		
		private bool _skipNextPolling { get; set; }
		
		
		public PollingService() 
		{
		
		}
		public void SkipNextPolling()
		{
			_skipNextPolling = true;
		}
	
		public async Task Wait(int polling)
		{
			if (_skipNextPolling)
				await Task.CompletedTask;
			else
				await Task.Delay(polling);

			_skipNextPolling = false;
		}
	}
}
