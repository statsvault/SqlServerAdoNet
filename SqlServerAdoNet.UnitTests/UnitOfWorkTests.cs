using System;
using NUnit.Framework;
using FluentAssertions;

namespace StatKings.SqlServerAdoNet.UnitTests
{
    /// <summary>
    /// Unit tests for StatKings.SqlServerAdoNet.UnitOfWork.
    /// </summary>
    [TestFixture]
    public class UnitOfWorkTests
    {
        /// <summary>
        /// Test that an ArgumentException is thrown when a null or empty connection string is passed in.
        /// </summary>
        /// <param name="connectionString"></param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Create_NoConnectionString_ArgumentExceptionThrown(string connectionString)
        {
            Action act = () => UnitOfWork.Create(connectionString);
            act.Should().Throw<ArgumentException>("Connection string is requried.");
        }
    }
}
