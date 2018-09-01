using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using FluentAssertions;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using StatKings.SqlServerAdoNet;

namespace StatKngs.SqlServerAdoNet.UnitTests
{
    /// <summary>
    /// Unit tests for StatKings.SqlServerAdoNet.CommandSttings.
    /// </summary>
    [TestFixture]
    public class CommandSettingsTests
    {
        private const string QUERY = "select * from table";

        /// <summary>
        /// Test that a CommandSettings object is created when a valid command type is specified.
        /// </summary>
        /// <param name="commandType">Command type to test.</param>
        [TestCase(CommandType.Text)]
        [TestCase(CommandType.StoredProcedure)]
        public void CommandSettings_ValidCommandType_CommandSettingsCreated(CommandType commandType)
        {
            var cmdSettings = new CommandSettings(commandType, QUERY);
            cmdSettings.CommandType.Should().Be(commandType);
        }
        
        /// <summary>
        /// Test that an ArgumentException is thrown when an invalid command type is specified.
        /// </summary>
        [Test]
        public void CommandSettings_InvalidCommandType_ArgumentExceptionThrown()
        {
            Action act = () => new CommandSettings(CommandType.TableDirect, QUERY);
            act.Should().Throw<ArgumentException>().WithMessage("CommandType.TableDirect not supported.\r\nParameter name: commandType");
        }

        /// <summary>
        /// Test that a CommandSettings object is created when the command text is specified.
        /// </summary>
        [Test]
        public void CommandSettings_ValidCommandText_CommandSettingsCreated()
        {
            var cmdSettings = new CommandSettings(CommandType.Text, QUERY);
            cmdSettings.CommandText.Should().Be(QUERY);
        }

        /// <summary>
        /// Test that an ArgumentException is thrown when a command text is not specified.
        /// </summary>
        /// <param name="commandText">Command text to test.</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CommandSettings_EmptyCommandText_ArgumentExceptionThrown(string commandText)
        {
            Action act = () => new CommandSettings(commandText);
            act.Should().Throw<ArgumentException>().WithMessage("Command text is required.\r\nParameter name: commandText");
        }

        /// <summary>
        /// Test that the parameters are correctly set on the CommandSettings object.
        /// </summary>
        public void CommandSettings_WithParameters_HasExpectedParameters()
        {
            var prms = new List<IDbDataParameter>();
            prms.Add(new SqlParameter("@ArtistId", 1));
            var cmdSettings = new CommandSettings(QUERY, prms);
            var compareLogic = new CompareLogic();
            var result = compareLogic.Compare(cmdSettings.Parameters, prms);
            result.AreEqual.Should().BeTrue();
        }
    }
}
