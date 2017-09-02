using System;
using CoreHelpers.Azure.Worker.Hosting;
using Xunit;

namespace CoreHelpers.Azure.Worker.Tests.Hosting
{
	public class WorkerHostingEnvironmentTests
	{
		[Fact]
		public void HostingEnvironmentContainsProcessRootPath() 
		{
			var env = new WorkerHostingEnvironment();
			Assert.EndsWith("microsoft.testplatform.testhost/15.3.0/lib/netstandard1.5", env.ProcessRootPath);
		}
		
		[Fact]
		public void HostingEnvironmentHasTheRightEnvironment() 
		{
			// our env model 
			var env = new WorkerHostingEnvironment();
			Assert.Equal("Production", env.EnvironmentName);
			
			// manipulate the data
			Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
			Assert.Equal("Development", env.EnvironmentName);								
			
			// manipulate the data
			Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Staging");
			Assert.Equal("Staging", env.EnvironmentName);								
		}
	}
}
