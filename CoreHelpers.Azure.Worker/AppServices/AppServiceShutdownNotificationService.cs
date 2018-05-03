using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreHelpers.Azure.Worker.Hosting;

namespace CoreHelpers.Azure.Worker.AppServices
{
	public class AppServiceShutdownNotificationService : IShutdownNotificationService
	{		
        private List<Func<Task>> shutdownNotificationHandlers { get; set; } = new List<Func<Task>>();
        private ManualResetEvent shutdownCompleted { get; set; } = new ManualResetEvent(false);

		public AppServiceShutdownNotificationService() 
		{ 
		 	Task.Run(async () =>
			{
			
				// Get the shutdown file path from the environment
				var shutdownFile = AppServiceShutdownNotificationService.GetShutdownFileName();

				// initialize the watcher
				var fileSystemWatcher = new FileSystemWatcherEx(shutdownFile);
								
				// wait until the event is signaled
                fileSystemWatcher.FileChangedEvent.WaitOne(Timeout.Infinite);

                // use the revers order 
                shutdownNotificationHandlers.Reverse();


                // signal all registered notification handelrs
                foreach(var action in shutdownNotificationHandlers)
                {
                    await action.Invoke();
                };

                // singnal shutdown
                shutdownCompleted.Set();
			});
		}
		
		private static string GetShutdownFileName()
		{
			var triggerFileName = Environment.GetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE");
			if (triggerFileName == null) { triggerFileName = "/tmp/stop.notify"; }
			return triggerFileName;
		}
				
        public void OnShutdownNotification(Func<Task> action)
        {
            shutdownNotificationHandlers.Add(action);
        }

        public void WaitForAllNotificationHandlers()
        {
            shutdownCompleted.WaitOne(Timeout.Infinite);
        }
	}
	
	internal class FileSystemWatcherEx : FileSystemWatcher 
	{
		public ManualResetEvent FileChangedEvent { get; set; }
		
		private string _FileToWatch { get; set; }

		public FileSystemWatcherEx(string fileToWatch)		
		: base(System.IO.Path.GetDirectoryName(fileToWatch))
		{
			_FileToWatch = fileToWatch;
			FileChangedEvent = new ManualResetEvent(false);
			
			NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
			IncludeSubdirectories = false;
			EnableRaisingEvents = true;


			this.Created += OnChanged;
			this.Changed += OnChanged;
		}

		
		private void OnChanged(object sender, FileSystemEventArgs e)
		{						
			if (e.FullPath.IndexOf(System.IO.Path.GetFileName(_FileToWatch), StringComparison.OrdinalIgnoreCase) >= 0)
			{
				FileChangedEvent.Set();		
			}			
		}				
	}	
}
