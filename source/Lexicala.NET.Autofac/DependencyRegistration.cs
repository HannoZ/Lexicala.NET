﻿using Autofac;
using Lexicala.NET.Configuration;
using Lexicala.NET.Parsing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Lexicala.NET.Autofac
{
    public static class DependencyRegistration
    {
        public static IContainer RegisterLexixala(LexicalaConfig config)
        {
            var builder = new ContainerBuilder();

            var httpClient = config.CreateHttpClient();
            builder.RegisterInstance(httpClient)
                .SingleInstance();

            var lexicalaClient = new LexicalaClient(httpClient);
            builder.RegisterInstance(lexicalaClient)
                .As<ILexicalaClient>();

            // register options since MemoryCache depends on it
            builder.RegisterGeneric(typeof(OptionsManager<>)).As(typeof(IOptions<>)).SingleInstance();
            builder.RegisterGeneric(typeof(OptionsManager<>)).As(typeof(IOptionsSnapshot<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(OptionsMonitor<>)).As(typeof(IOptionsMonitor<>)).SingleInstance();
            builder.RegisterGeneric(typeof(OptionsFactory<>)).As(typeof(IOptionsFactory<>));
            builder.RegisterGeneric(typeof(OptionsCache<>)).As(typeof(IOptionsMonitorCache<>)).SingleInstance();

            builder.RegisterType<MemoryCache>()
                .As<IMemoryCache>()
                .SingleInstance();

            builder.RegisterType<LexicalaSearchParser>()
                .As<ILexicalaSearchParser>()
                .SingleInstance();

            return builder.Build();
        }
    }
}