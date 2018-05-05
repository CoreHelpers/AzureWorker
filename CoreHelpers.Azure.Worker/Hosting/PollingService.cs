using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
	public class PollingService : IPollingService
	{		
		private bool _skipNextPolling { get; set; }
        private bool _abortNextPolling { get; set; }
		
        private AutoResetEvent _pollingAbort { get; set; }

		public PollingService() 
		{
            _skipNextPolling = false;
            _abortNextPolling = false;
            _pollingAbort = new AutoResetEvent(false);
		}

		public void SkipNextPolling()
		{
			_skipNextPolling = true;
            _pollingAbort.Set();
		}

        public void AbortDuringNextPolling()
        {
            _abortNextPolling = true;
            _pollingAbort.Set();
        }
	
		public bool Wait(int polling)
		{
            if (_abortNextPolling)
            {
                _abortNextPolling = false;
                _skipNextPolling = false;
                return false;
            }
            else if (_skipNextPolling) 
            {
                _abortNextPolling = false;
                _skipNextPolling = false;
                return true;
            } 
            else
            {
                // wait for the polling
                _pollingAbort.WaitOne(polling);

                // doubel hcek for abort 
                if (_abortNextPolling)
                    return false;
                else
                    return true;                
            }			
		}
	}
}
