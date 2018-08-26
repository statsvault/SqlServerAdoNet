using System;
using System.Data;
using NUnit.Framework;
using FluentAssertions;

namespace StatKings.SqlServerAdoNet.UnitTests
{
    /// <summary>
    /// Unit tests for StatKings.SqlServerAdoNet.SqlRunner.
    /// </summary>
    [TestFixture]
    public class SqlRunnerTests
    {
        private const string CMD_EX = "commandSettings is required.";
        private const string CMD_TEXT_EX = "commandSettings.Text is required.";

        /// <summary>
        /// Test that an exception is not thrown when an initialized unit of work is passed into constructor.
        /// </summary>
        [Test]
        public void Ctor_NonNullUnitOfWork_NoExceptionThrown()
        {
            Action act = () => new SqlRunner(new MyUnitOfWork());
            act.Should().NotThrow();
        }

        /// <summary>
        /// Test that an ArgumentNullException is thrown when a null unit of work is passed into constructor.
        /// </summary>
        [Test]
        public void Ctor_NullUnitOfWork_ArgumentNullExceptionThrown()
        {
            Action act = () => new SqlRunner(null);
            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Test that an ArgumentNullException is thrown when a null CommandSettings is passed into ExecuteNonQuery.
        /// </summary>
        [Test]
        public void ExecuteNonQuery_NullCommandSettings_ArgumentNullExceptionThrown()
        {
            var runner = new SqlRunner(new MyUnitOfWork());
            Action act = () => runner.ExecuteNonQuery(null);
            act.Should().Throw<ArgumentNullException>(CMD_EX);
        }

        /// <summary>
        /// Test that an ArgumentNullException is thrown when a null CommandSettings is passed into ExecuteScalar.
        /// </summary>
        [Test]
        public void ExecuteScalar_NullCommandSettings_ArgumentNullExceptionThrown()
        {
            var runner = new SqlRunner(new MyUnitOfWork());
            Action act = () => runner.ExecuteScalar(null);
            act.Should().Throw<ArgumentNullException>(CMD_EX);
        }

        /// <summary>
        /// Test that an ArgumentNullException is thrown when a null CommandSettings is passed into ExecuteReader.
        /// </summary>
        [Test]
        public void ExecuteReader_NullCommandSettings_ArgumentNullExceptionThrown()
        {
            var runner = new SqlRunner(new MyUnitOfWork());
            Action act = () => runner.ExecuteReader<TestModel>(null);
            act.Should().Throw<ArgumentNullException>(CMD_EX);
        }

        /// <summary>
        /// Test that an ArgumentNullException is thrown when a null CommandSettings is passed into ExecuteReaderFirst.
        /// </summary>
        [Test]
        public void ExecuteReaderFirst_NullCommandSettings_ArgumentNullExceptionThrown()
        {
            var runner = new SqlRunner(new MyUnitOfWork());
            Action act = () => runner.ExecuteReaderFirst<TestModel>(null);
            act.Should().Throw<ArgumentNullException>(CMD_EX);
        }

        /// <summary>
        /// Test that an ArgumentException is thrown when a null or empty CommandSettings.Text is passed into ExecuteNonQuery.
        /// </summary>
        /// <param name="commandText">Command text to test.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ExecuteNonQuery_EmptyCommandText_ArgumentExceptionThrown(string commandText)
        {
            var runner = new SqlRunner(new MyUnitOfWork());
            Action act = () => runner.ExecuteNonQuery(new CommandSettings(commandText));
            act.Should().Throw<ArgumentException>(CMD_TEXT_EX);
        }

        /// <summary>
        /// Test that an ArgumentException is thrown when a null or empty CommandSettings.Text is passed into ExecuteScalar.
        /// </summary>
        /// <param name="commandText">Command text to test.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ExecuteScalar_EmptyCommandText_ArgumentNullExceptionThrown(string commandText)
        {
            var runner = new SqlRunner(new MyUnitOfWork());
            Action act = () => runner.ExecuteScalar(new CommandSettings(commandText));
            act.Should().Throw<ArgumentException>(CMD_TEXT_EX);
        }

        /// <summary>
        /// Test that an ArgumentException is thrown when a null or empty CommandSettings.Text is passed into ExecuteReader.
        /// </summary>
        /// <param name="commandText">Command text to test.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ExecuteReader_EmptyCommandText_ArgumentNullExceptionThrown(string commandText)
        {
            var runner = new SqlRunner(new MyUnitOfWork());
            Action act = () => runner.ExecuteReader<TestModel>(new CommandSettings(commandText));
            act.Should().Throw<ArgumentException>(CMD_TEXT_EX);
        }

        /// <summary>
        /// Test that an ArgumentException is thrown when a null or empty CommandSettings.Text is passed into ExecuteReaderFirst.
        /// </summary>
        /// <param name="commandText">Command text to test.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ExecuteReaderFirst_EmptyCommandText_ArgumentNullExceptionThrown(string commandText)
        {
            var runner = new SqlRunner(new MyUnitOfWork());
            Action act = () => runner.ExecuteReaderFirst<TestModel>(new CommandSettings(commandText));
            act.Should().Throw<ArgumentException>(CMD_TEXT_EX);
        }

        /// <summary>
        /// Test class.
        /// </summary>
        private class TestModel
        {
            public int MemberA { get; set; }
        }

        /// <summary>
        /// Unit of work for SqlRunner ctor.
        /// </summary>
        private class MyUnitOfWork : IUnitOfWork
        {
            public void BeginTransaction() { }
            public void Commit() { }
            public IDbCommand CreateCommand() { return null; }
            public void Rollback() { }
            public void Dispose() { }
        }
    }
}
