﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class RoutinesRenderer : ITemplateRenderer
    {
        public RoutinesRenderer(
            IDbConnection connection,
            IRelationalDatabase database,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IDatabaseRoutine> routines,
            DirectoryInfo exportDirectory)
        {
            if (routines == null || routines.AnyNull())
                throw new ArgumentNullException(nameof(routines));

            Routines = routines;

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapper = new MainModelMapper(Connection, Database);

            var routineViewModels = Routines.Select(mapper.Map).ToList();
            var routinesVm = new Routines(routineViewModels);

            var renderedMain = Formatter.RenderTemplate(routinesVm);

            var databaseName = !Database.IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? Database.IdentifierDefaults.Database + " Database"
                : "Database";
            var pageTitle = "Routines — " + databaseName;
            var mainContainer = new Container(renderedMain, pageTitle, string.Empty);
            var renderedPage = Formatter.RenderTemplate(mainContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "routines.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
