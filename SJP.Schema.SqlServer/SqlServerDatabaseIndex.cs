﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerDatabaseViewIndex : SqlServerDatabaseIndex<IRelationalDatabaseView>, IDatabaseViewIndex
    {
        public SqlServerDatabaseViewIndex(IRelationalDatabaseView view, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseViewColumn> includedColumns)
            : base(view, name, isUnique, columns, includedColumns)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }

    public class SqlServerDatabaseTableIndex : SqlServerDatabaseIndex<IRelationalDatabaseTable>, IDatabaseTableIndex
    {
        public SqlServerDatabaseTableIndex(IRelationalDatabaseTable table, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseTableColumn> includedColumns)
            : base(table, name, isUnique, columns, includedColumns)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public abstract class SqlServerDatabaseIndex<T> : IDatabaseIndex<T> where T : class, IDatabaseQueryable
    {
        protected SqlServerDatabaseIndex(T parent, Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));
            if (includedColumns != null && includedColumns.AnyNull())
                throw new ArgumentNullException(nameof(includedColumns));

            if (includedColumns == null)
                includedColumns = Enumerable.Empty<IDatabaseColumn>();

            Parent = parent;
            Name = name;
            IsUnique = isUnique;
            Columns = columns;
            IncludedColumns = includedColumns;
        }

        public T Parent { get; }

        public Identifier Name { get; }

        public bool IsUnique { get; }

        public IEnumerable<IDatabaseIndexColumn> Columns { get; }

        public IEnumerable<IDatabaseColumn> IncludedColumns { get; }
    }

    public class SqlServerDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public SqlServerDatabaseIndexColumn(IDatabaseColumn column, IndexColumnOrder order)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var columns = new[] { column };
            DependentColumns = columns.ToImmutableList();
            Order = order;
        }

        public IList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }

        public string GetExpression(IDatabaseDialect dialect)
        {
            return DependentColumns
                .Select(c => dialect.QuoteName(c.Name))
                .Single();
        }
    }
}
