﻿using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.DataAccess.Poco;

namespace SJP.Schematic.Tool.Handlers
{
    internal class GeneratePocoCommandHandler : DatabaseCommand
    {
        public GeneratePocoCommandHandler(FileInfo filePath)
            : base(filePath)
        {
        }

        public async Task<int> HandleCommand(FileInfo projectPath, string baseNamespace, string convention, CancellationToken cancellationToken)
        {
            var fileSystem = new FileSystem();
            var nameTranslator = GetNameTranslator(convention);
            var connection = await GetSchematicConnectionAsync(cancellationToken).ConfigureAwait(false);
            var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);
            var commentProvider = await connection.Dialect.GetRelationalDatabaseCommentProviderAsync(connection, cancellationToken).ConfigureAwait(false);

            var generator = new PocoDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);

            await generator.Generate(projectPath.FullName, baseNamespace, cancellationToken).ConfigureAwait(false);
            return ErrorCode.Success;
        }
    }
}