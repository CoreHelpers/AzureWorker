using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using CoreHelpers.Azure.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreHelpers.Azure.Worker.Hosting.Internal
{
	public class ConventionBasedStartup : IStartup
	{
		private readonly StartupMethods _methods;

        public ConventionBasedStartup(StartupMethods methods)
        {
            _methods = methods;
        }
        
        public void Configure(IWorkerApplicationBuilder app)
        {
            try
            {
                _methods.ConfigureDelegate(app);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }
	
		public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                return _methods.ConfigureServicesDelegate(services);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }

                throw;
            }
        }

		
	}
}
