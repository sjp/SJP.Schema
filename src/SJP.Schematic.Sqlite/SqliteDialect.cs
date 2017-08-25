﻿using System;
using System.Data;
using Microsoft.Data.Sqlite;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDialect : DatabaseDialect<SqliteDialect>
    {
        public override IDbConnection CreateConnection(string connectionString, bool openConnection = true)
        {
            var connection = new SqliteConnection(connectionString);
            if (openConnection)
                connection.Open();
            return connection;
        }

        public override string GetTypeName(DataType dataType)
        {
            throw new NotImplementedException();
        }

        public override bool IsValidColumnName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override bool IsValidConstraintName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override bool IsValidObjectName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            // sqlite doesn't support anything more complex than localnames, not even multiple schemas...
            return QuoteIdentifier(name.LocalName);
        }
    }
}
