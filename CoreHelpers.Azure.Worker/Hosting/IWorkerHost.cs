﻿using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
    public interface IWorkerHost
    {
		Task ConfigureAsync();
		
		Task RunAsync(TimeSpan executionTimeout);		
		
		void Run(TimeSpan executionTimeout);
    }
}
