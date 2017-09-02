using System;
using CoreHelpers.Azure.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreHelpers.Azure.Worker.Hosting
{
    public interface IStartup
    {
        IServiceProvider ConfigureServices(IServiceCollection services);

        void Configure(IWorkerApplicationBuilder app);
    }
}
