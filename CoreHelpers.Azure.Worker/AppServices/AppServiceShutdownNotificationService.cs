using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreHelpers.Azure.Worker.Hosting;

namespace CoreHelpers.Azure.Worker.AppServices
{
	public class AppServiceShutdownNotificationService : IShutdownNotificationService
	{
		private Task fsWatcherTask { get; set; }
		
		public AppServiceShutdownNotificationService() 
		{ 
		 	fsWatcherTask = Task.Run(() =>
			{
			
				// Get the shutdown file path from the environment
				var shutdownFile = AppServiceShutdownNotificationService.GetShutdownFileName();

				// initialize the watcher
				var fileSystemWatcher = new FileSystemWatcherEx(shutdownFile);
								
				// wait until the event is signaled
				fileSystemWatcher.FileChangedEvent.WaitOne();
			});
		}
		
		private static string GetShutdownFileName()
		{
			var triggerFileName = Environment.GetEnvironmentVariable("WEBJOBS_SHUTDOWN_FILE");
			if (triggerFileName == null) { triggerFileName = "/tmp/stop.notify"; }
			return triggerFileName;
		}
		
		public bool WaitForShutdown(Task parentTask)
		{									
			// observer tasks 
			var observerTask = new Task[] {
				fsWatcherTask,
				parentTask != null ? parentTask : Task.CompletedTask
			};

			// wait for at least one 
			var waitResult = Task.WaitAny(observerTask);

			// check if it was the fs watcher
			return (waitResult != 0);
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
