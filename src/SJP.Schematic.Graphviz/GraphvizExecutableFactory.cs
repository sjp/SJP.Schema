﻿using System;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Graphviz
{
    public sealed class GraphvizExecutableFactory
    {
        private readonly string? _configuredPath;

        public GraphvizExecutableFactory()
        {
        }

        public GraphvizExecutableFactory(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _configuredPath = configuration.GetValue<string>("Graphviz:Dot");
        }

        public IGraphvizExecutable GetExecutable()
        {
            var envPath = Environment.GetEnvironmentVariable("SCHEMATIC_GRAPHVIZ_DOT");
            if (!envPath.IsNullOrWhiteSpace())
                return new GraphvizSystemExecutable(envPath);

            if (!_configuredPath.IsNullOrEmpty())
                return new GraphvizSystemExecutable(_configuredPath);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return new GraphvizTemporaryExecutable();

            // just try running 'dot', assume there's a system executable
            return new GraphvizSystemExecutable("dot");
        }
    }
}
