﻿using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Parsing;

namespace SJP.Schematic.SqlServer
{
    public sealed class SqlServerDependencyProvider : IDependencyProvider
    {
        public SqlServerDependencyProvider(IEqualityComparer<Identifier> comparer = null)
        {
            Comparer = comparer ?? IdentifierComparer.Ordinal;
        }

        private IEqualityComparer<Identifier> Comparer { get; }

        public IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (expression.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(expression));

            var tokenizer = new SqlServerTokenizer();

            var tokenizeResult = tokenizer.TryTokenize(expression);
            if (!tokenizeResult.HasValue)
                throw new ArgumentException($"Could not parse the given expression as a SQL expression. Given: { expression }", nameof(expression));

            var result = new HashSet<Identifier>(Comparer);

            var tokens = tokenizeResult.Value;

            var next = tokens.ConsumeToken();
            while (next.HasValue)
            {
                var sqlIdentifier = SqlServerTokenParsers.QualifiedName(next.Location);
                if (sqlIdentifier.HasValue)
                {
                    var dependentIdentifier = sqlIdentifier.Value;
                    if (!Comparer.Equals(dependentIdentifier.Value, objectName))
                        result.Add(dependentIdentifier.Value);

                    next = sqlIdentifier.Remainder.ConsumeToken();
                }
                else
                {
                    next = next.Remainder.ConsumeToken();
                }
            }

            return result;
        }
    }
}
