﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class RelationalDatabaseView : IRelationalDatabaseView
    {
        public RelationalDatabaseView(
            IRelationalDatabase database,
            Identifier viewName,
            string definition,
            IReadOnlyList<IDatabaseColumn> columns,
            IReadOnlyCollection<IDatabaseIndex> indexes,
            IEqualityComparer<Identifier> comparer = null)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Name = viewName ?? throw new ArgumentNullException(nameof(viewName));
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
            IsIndexed = Indexes.Count > 0;
            Definition = definition;

            Comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, database.ServerName, database.DatabaseName, database.DefaultSchema);

            Column = CreateColumnLookup(Columns, Comparer);
            Index = CreateIndexLookup(Indexes, Comparer);
        }

        public Identifier Name { get; }

        protected IRelationalDatabase Database { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        public string Definition { get; }

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Definition);

        public bool IsIndexed { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseIndex> Index { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Index);

        public IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Indexes);

        public IReadOnlyDictionary<Identifier, IDatabaseColumn> Column { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Column);

        public IReadOnlyList<IDatabaseColumn> Columns { get; }

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Columns);

        private static IReadOnlyDictionary<Identifier, IDatabaseColumn> CreateColumnLookup(IReadOnlyList<IDatabaseColumn> columns, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count, comparer);

            var namedColumns = columns.Where(c => c.Name != null);
            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result;
        }

        private static IReadOnlyDictionary<Identifier, IDatabaseIndex> CreateIndexLookup(IReadOnlyCollection<IDatabaseIndex> indexes, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count, comparer);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result;
        }
    }
}
