﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public sealed class EmptyRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        public Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken)) => _emptyTables;

        private readonly static Task<IReadOnlyCollection<IRelationalDatabaseTable>> _emptyTables = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTable>>(Array.Empty<IRelationalDatabaseTable>());

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return OptionAsync<IRelationalDatabaseTable>.None;
        }
    }
}
