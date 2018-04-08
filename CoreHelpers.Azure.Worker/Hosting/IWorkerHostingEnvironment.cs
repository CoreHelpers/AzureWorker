using System;
namespace CoreHelpers.Azure.Worker.Hosting
{
    public interface IWorkerHostingEnvironment
    {
    	string EnvironmentName { get; }
    	
    	string ProcessRootPath { get; }

        bool IsRunningInContainerEnvironment();

		bool IsDevelopment();
		
		bool IsStaging();
		
		bool IsProduction();
    }
}
