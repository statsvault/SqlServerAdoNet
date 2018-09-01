using System;
using System.Data;
using System.Data.Common;

namespace StatKings.SqlServerAdoNet
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Static factory for creating a unit of work instance.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>IUnitOfWork</returns>
        public static IUnitOfWork Create(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("A connection string is requried.", nameof(connectionString));
            }

            // Get the provider from the provider factory and create the connection.
            var provider = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var connection = provider.CreateConnection();

            try
            {
                connection.ConnectionString = connectionString;
                connection.Open();
            }
            catch (Exception ex)
            {
                var message = "Failed to create connection for '{0}'.  See inner exception for more details.";
                throw new DataException(string.Format(message, connectionString), ex);
            }

            return new UnitOfWork(connection);
        }

        private IDbConnection _connection;
        private IDbTransaction _transaction;

        /// <summary>
        /// Ctor.  Private and must be created by using the static Create factory method.
        /// </summary>
        private UnitOfWork(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Create a command object for the connection.  Enlist the transaction, if one exists.
        /// </summary>
        /// <returns>IDbCommand</returns>
        public IDbCommand CreateCommand()
        {
            var cmd = _connection.CreateCommand();
            cmd.Transaction = _transaction;
            return cmd;
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        public void BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction has already been started.");
            }

            try
            {
                _transaction = _connection.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("A transaction could not be started.  See inner exception for more details.", ex);
            }
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public void Commit()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("The transaction has already been commited, rolled back, or was never started.");
            }

            try
            {
                _transaction.Commit();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The transaction has already been committed or the connection has been broken.  See inner exception for more details.", ex);
            }

            _transaction = null;
        }

        /// <summary>
        /// Rolls back the database transaction.
        /// </summary>
        public void Rollback()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("The transaction has already been rolled back, commited, or was never started.");
            }

            try
            {
                _transaction.Rollback();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The transaction has already been rolled back or the connection has been broken.  See inner exception for more details.", ex);
            }

            _transaction = null;
        }

        /// <summary>
        /// Cleanup the database connection.
        /// </summary>
        public void Dispose()
        {
            if (_transaction != null)
            {
                // This should happen automatically when closing the connection, but just in case...
                _transaction.Rollback();
                _transaction = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
        }
    }
}

