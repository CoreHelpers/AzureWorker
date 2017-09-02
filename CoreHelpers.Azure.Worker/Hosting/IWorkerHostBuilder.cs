using System;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.Hosting
{
    public interface IWorkerHostBuilder
    {
		IServiceCollection Services { get; }
    
		IWorkerHost Build();			
    }
}
