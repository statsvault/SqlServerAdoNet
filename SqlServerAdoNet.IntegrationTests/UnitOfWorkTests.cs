using System;
using System.Data;
using NUnit.Framework;
using FluentAssertions;

namespace StatKings.SqlServerAdoNet.IntegrationTests
{
    /// <summary>
    /// Integration tests for StatKings.SqlServerAdoNet.UnitOfWork.
    /// </summary>
    [TestFixture]
    public class UnitOfWorkTests
    {
        /// <summary>
        /// Create the test database.  This will be run once.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Helper.CreateDatabase();
        }

        /// <summary>
        /// Drop the test database.  This will be run once.
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Helper.DropDatabase();
        }

        /// <summary>
        /// Test that a DataException is thrown when trying to create a unit of work for a data source
        /// that does not exist.
        /// </summary>
        [Test]
        public void Create_InvalidConnectionString_DataExceptionThrow()
        {
            Action act = () => UnitOfWork.Create("Server=dbserver;Database=dbtable;User Id=dbuser;Password=pwd;");
            act.Should().Throw<DataException>();
        }

        /// <summary>
        /// Test a unit of work is returned for a valid connection string.
        /// </summary>
        [Test]
        public void Create_ValidConnectionString_UnitOfWorkReturned()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.Should().NotBeNull();
            }
        }

        /// <summary>
        /// Test that the UnitOfWork will start a transaction. 
        /// </summary>
        [Test]
        public void BeginTransaction_TransactionNotAlreadyStarted_NoExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                Action act = () => uow.BeginTransaction();
                act.Should().NotThrow();
            }
        }

        /// <summary>
        /// Test that the UnitOfWork will throw an InvalidOperationException exception when trying to start
        /// a transaction when one already exists.
        /// </summary>
        [Test]
        public void BeginTransaction_TransactionAlreadyStarted_InvalidOperationExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                // Start a transaction and then try to start another one.
                uow.BeginTransaction();
                Action act = () => uow.BeginTransaction();
                act.Should().Throw<InvalidOperationException>().WithMessage("A Transaction has already been started.");
            }
        }

        /// <summary>
        /// Test that the UnitOfWork will commit an empty transaction (i.e. no data manipulation occurred).
        /// </summary>
        [Test]
        public void Commit_EmptyTransaction_NoExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                // Start a transaction and commit it.
                uow.BeginTransaction();
                Action act = () => uow.Commit();
                act.Should().NotThrow();
            }
        }

        /// <summary>
        /// Test that the UnitOfWork will throw an InvalidOperationException exception when trying to commit
        /// a transaction that does not exist.
        /// </summary>
        [Test]
        public void Commit_NoTransaction_InvalidOperationExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                Action act = () => uow.Commit();
                act.Should().Throw<InvalidOperationException>().WithMessage("The transaction has already been commited, rolled back, or was never started.");
            }
        }

        /// <summary>
        /// Test that the UnitOfWork will rollback an empty transaction (i.e. no data manipulation occurred).
        /// </summary>
        [Test]
        public void Rollback_EmptyTransaction_NoExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                // Start a transaction and roll it back.
                uow.BeginTransaction();
                Action act = () => uow.Rollback();
                act.Should().NotThrow();
            }
        }

        /// <summary>
        /// Test that the UnitOfWork will throw an InvalidOperationException exception when trying to rollback
        /// a transaction that does not exist.
        /// </summary>
        [Test]
        public void Rollback_NoTransaction_InvalidOperationExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                Action act = () => uow.Rollback();
                act.Should().Throw<InvalidOperationException>().WithMessage("The transaction has already been rolled back, commited, or was never started.");
            }
        }

        /// <summary>
        /// Test that the UnitOfWork will create a valid command object with no enlisted transaction.
        /// </summary>
        [Test]
        public void CreateCommand_TransactionNotStarted_CommandDoesNotHaveTransaction()
        {
            using (var uow = Helper.CreateUnitOfWork())
            using (var cmd = uow.CreateCommand())
            {
                cmd.Transaction.Should().BeNull();
            }
        }

        /// <summary>
        /// Test that the UnitOfWork will create a valid command object with an enlisted transaction.
        /// </summary>
        [Test]
        public void CreateCommand_TransactionStarted_CommandHasTransaction()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();
                using (var cmd = uow.CreateCommand())
                {
                    cmd.Transaction.Should().NotBeNull();
                }
            }
        }
    }
}
