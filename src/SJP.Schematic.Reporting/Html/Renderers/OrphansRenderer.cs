﻿using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class OrphansRenderer : ITemplateRenderer
    {
        public OrphansRenderer(IDbConnection connection, IRelationalDatabase database, IHtmlFormatter formatter, DirectoryInfo exportDirectory)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private DirectoryInfo ExportDirectory { get; }

        public void Render()
        {
            var orphanedTables = Database.Tables
                .Where(t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
                .ToList();

            var mapper = new OrphansModelMapper(Connection, Database.Dialect);
            var orphanedTableViewModels = orphanedTables
                .Select(mapper.Map)
                .OrderBy(vm => vm.Name)
                .ToList();

            var templateParameter = new Orphans(orphanedTableViewModels);
            var renderedOrphans = Formatter.RenderTemplate(templateParameter);

            var orphansContainer = new Container(renderedOrphans, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(orphansContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "orphans.html");

            File.WriteAllText(outputPath, renderedPage);
        }

        public async Task RenderAsync()
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);
            var orphanedTables = tables
                .Where(t => t.ParentKeys.Empty() && t.ChildKeys.Empty())
                .ToList();

            var mapper = new OrphansModelMapper(Connection, Database.Dialect);
            var mappingTasks = orphanedTables.Select(mapper.MapAsync).ToArray();
            var orphanedTableViewModels = await Task.WhenAll(mappingTasks).ConfigureAwait(false);

            var templateParameter = new Orphans(orphanedTableViewModels);
            var renderedOrphans = Formatter.RenderTemplate(templateParameter);

            var orphansContainer = new Container(renderedOrphans, Database.DatabaseName, string.Empty);
            var renderedPage = Formatter.RenderTemplate(orphansContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "orphans.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
