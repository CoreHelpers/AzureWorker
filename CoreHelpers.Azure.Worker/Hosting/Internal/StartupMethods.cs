﻿using System;
using System.Diagnostics;
using CoreHelpers.Azure.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreHelpers.Azure.Worker.Hosting.Internal
{
	public class StartupMethods
	{
		 public StartupMethods(object instance, Action<IWorkerApplicationBuilder> configure, Func<IServiceCollection, IServiceProvider> configureServices)
        {
            Debug.Assert(configure != null);
            Debug.Assert(configureServices != null);

            StartupInstance = instance;
            ConfigureDelegate = configure;
            ConfigureServicesDelegate = configureServices;
        }

        public object StartupInstance { get; }
        public Func<IServiceCollection, IServiceProvider> ConfigureServicesDelegate { get; }
        public Action<IWorkerApplicationBuilder> ConfigureDelegate { get; }
	}
}
