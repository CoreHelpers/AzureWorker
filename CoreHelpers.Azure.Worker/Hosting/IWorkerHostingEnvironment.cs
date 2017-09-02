using System;
namespace CoreHelpers.Azure.Worker.Hosting
{
    public interface IWorkerHostingEnvironment
    {
    	string EnvironmentName { get; }
    	
    	string ProcessRootPath { get; }

		bool IsDevelopment();
		
		bool IsStaging();
		
		bool IsProduction();
    }
}
