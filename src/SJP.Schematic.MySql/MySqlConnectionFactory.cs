﻿using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using Polly;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql
{
    /// <summary>
    /// A connection factory that provides MySQL connections.
    /// </summary>
    /// <seealso cref="IDbConnectionFactory" />
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlConnectionFactory"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionString"/> is <c>null</c>, empty or whitespace.</exception>
        public MySqlConnectionFactory(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        protected string ConnectionString { get; }

        /// <summary>
        /// Creates a database connection instance, but does not open the connection.
        /// </summary>
        /// <returns>An object representing a database connection</returns>
        public IDbConnection CreateConnection() => new MySqlConnection(ConnectionString);

        /// <summary>
        /// Creates and opens a database connection.
        /// </summary>
        /// <returns>An object representing a database connection.</returns>
        public IDbConnection OpenConnection()
        {
            var connection = new MySqlConnection(ConnectionString);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        /// <summary>
        /// Creates and opens a database connection asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task containing an object representing a database connection when completed.</returns>
        public async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new MySqlConnection(ConnectionString);

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            return connection;
        }

        /// <summary>
        /// Determines whether connections retrieved from this factory should be disposed.
        /// </summary>
        /// <value>Always <c>true</c>.</value>
        public bool DisposeConnection { get; } = true;

        /// <summary>
        /// Gets a database command retry policy builder.
        /// </summary>
        /// <value>A retry policy builder.</value>
        public PolicyBuilder RetryPolicy => Policy
            .Handle<MySqlException>(IsTransientError)
            .Or<TimeoutException>();

        private static bool IsTransientError(MySqlException mysqlEx)
        {
            switch (mysqlEx.Number)
            {
                case 1042: // ER_BAD_HOST_ERROR
                case 2002: // CR_CONNECTION_ERROR
                case 2003: // CR_CONN_HOST_ERROR
                case 2006: // CR_SERVER_GONE_ERROR
                case 2009: // CR_WRONG_HOST_INFO
                case 2013: // CR_SERVER_LOST
                    return true;
                default:
                    return false;
            }
        }
    }
}
