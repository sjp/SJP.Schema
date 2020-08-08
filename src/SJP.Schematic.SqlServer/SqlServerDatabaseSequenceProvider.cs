﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    /// <summary>
    /// A comment provider for SQL Server database sequences.
    /// </summary>
    /// <seealso cref="IDatabaseSequenceProvider" />
    public class SqlServerDatabaseSequenceProvider : IDatabaseSequenceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDatabaseSequenceProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <c>null</c>.</exception>
        public SqlServerDatabaseSequenceProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// A database connection factory.
        /// </summary>
        /// <value>A database connection factory.</value>
        protected IDbConnectionFactory Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Gets all database sequences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database sequences.</returns>
        public async IAsyncEnumerable<IDatabaseSequence> GetAllSequences([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<SequenceData>(SequencesQuery, cancellationToken).ConfigureAwait(false);
            var sequences = queryResult
                .Select(row =>
                {
                    var sequenceName = QualifySequenceName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.ObjectName));
                    return BuildSequenceFromDto(sequenceName, row);
                });

            foreach (var sequence in sequences)
                yield return sequence;
        }

        /// <summary>
        /// Gets a query that retrieves information on all sequences in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequencesQuery => SequencesQuerySql;

        private static readonly string SequencesQuerySql = @$"
select
    schema_name(schema_id) as [{ nameof(SequenceData.SchemaName) }],
    name as [{ nameof(SequenceData.ObjectName) }],
    start_value as [{ nameof(SequenceData.StartValue) }],
    increment as [{ nameof(SequenceData.Increment) }],
    minimum_value as [{ nameof(SequenceData.MinValue) }],
    maximum_value as [{ nameof(SequenceData.MaxValue) }],
    is_cycling as [{ nameof(SequenceData.Cycle) }],
    is_cached as [{ nameof(SequenceData.IsCached) }],
    cache_size as [{ nameof(SequenceData.CacheSize) }]
from sys.sequences
where is_ms_shipped = 0
order by schema_name(schema_id), name";

        /// <summary>
        /// Gets a database sequence.
        /// </summary>
        /// <param name="sequenceName">A database sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequence(candidateSequenceName, cancellationToken);
        }

        /// <summary>
        /// Gets the resolved name of the sequence. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="sequenceName">A sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A sequence name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNone<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = candidateSequenceName.Schema, SequenceName = candidateSequenceName.LocalName },
                cancellationToken
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSequenceName.Server, candidateSequenceName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// Gets a query that resolves the name of a sequence.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private static readonly string SequenceNameQuerySql = @$"
select top 1 schema_name(schema_id) as [{ nameof(QualifiedName.SchemaName) }], name as [{ nameof(QualifiedName.ObjectName) }]
from sys.sequences
where schema_id = schema_id(@SchemaName) and name = @SequenceName and is_ms_shipped = 0";

        private static IDatabaseSequence BuildSequenceFromDto(Identifier sequenceName, SequenceData seqData)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));
            if (seqData == null)
                throw new ArgumentNullException(nameof(seqData));

            return new DatabaseSequence(
                sequenceName,
                seqData.StartValue,
                seqData.Increment,
                Option<decimal>.Some(seqData.MinValue),
                Option<decimal>.Some(seqData.MaxValue),
                seqData.Cycle,
                seqData.IsCached ? seqData.CacheSize ?? -1 : 0 // -1 as unknown/database determined
            );
        }

        /// <summary>
        /// Gets a query that retrieves all relevant information on a sequence.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequenceQuery => SequenceQuerySql;

        private static readonly string SequenceQuerySql = @$"
select
    start_value as [{ nameof(SequenceData.StartValue) }],
    increment as [{ nameof(SequenceData.Increment) }],
    minimum_value as [{ nameof(SequenceData.MinValue) }],
    maximum_value as [{ nameof(SequenceData.MaxValue) }],
    is_cycling as [{ nameof(SequenceData.Cycle) }],
    is_cached as [{ nameof(SequenceData.IsCached) }],
    cache_size as [{ nameof(SequenceData.CacheSize) }]
from sys.sequences
where schema_name(schema_id) = @SchemaName and name = @SequenceName and is_ms_shipped = 0";

        /// <summary>
        /// Retrieves database sequence information.
        /// </summary>
        /// <param name="sequenceName">A database sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseSequence> LoadSequence(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return GetResolvedSequenceName(candidateSequenceName, cancellationToken)
                .Bind(name => LoadSequenceData(name, cancellationToken)
                    .Map(seq => BuildSequenceFromDto(name, seq)));
        }

        private OptionAsync<SequenceData> LoadSequenceData(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Connection.QueryFirstOrNone<SequenceData>(
                SequenceQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName },
                cancellationToken
            );
        }

        /// <summary>
        /// Qualifies the name of the sequence.
        /// </summary>
        /// <param name="sequenceName">A view name.</param>
        /// <returns>A sequence name is at least as qualified as the given sequence name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected Identifier QualifySequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var schema = sequenceName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, sequenceName.LocalName);
        }
    }
}
