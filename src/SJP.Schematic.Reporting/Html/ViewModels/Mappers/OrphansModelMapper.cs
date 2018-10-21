﻿using System;
using System.Data;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class OrphansModelMapper
    {
        public OrphansModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

        public Orphans.Table Map(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var columnCount = table.Column.UCount();
            var rowCount = Connection.GetRowCount(Dialect, table.Name);

            return new Orphans.Table(table.Name, columnCount, rowCount);
        }

        public Task<Orphans.Table> MapAsync(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return MapAsyncCore(table);
        }

        private async Task<Orphans.Table> MapAsyncCore(IRelationalDatabaseTable table)
        {
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var rowCount = await Connection.GetRowCountAsync(Dialect, table.Name).ConfigureAwait(false);

            return new Orphans.Table(table.Name, columnLookup.UCount(), rowCount);
        }
    }
}
