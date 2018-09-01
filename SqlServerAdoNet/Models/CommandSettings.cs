using System;
using System.Collections.Generic;
using System.Data;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class for database command properties.
    /// </summary>
    public class CommandSettings
    {
        private CommandType _commandType;
        private string _commandText;
        private IEnumerable<IDbDataParameter> _parameters;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="commandText">Text of the query to execute.</param>
        public CommandSettings(string commandText)
        {
            SetProperties(CommandType.Text, commandText, new List<IDbDataParameter>());
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="commandType">Type of the query to execute.</param>
        /// <param name="commandText">Text of the query to execute.</param>
        public CommandSettings(CommandType commandType, string commandText)
        {
            SetProperties(commandType, commandText, new List<IDbDataParameter>());
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="commandText">Text of the query to execute.</param>
        /// <param name="parameters">Parameters of the query.</param>
        public CommandSettings(string commandText, IEnumerable<IDbDataParameter> parameters)
        {
            SetProperties(CommandType.Text, commandText, parameters);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="commandType">Type of the query to execute.</param>
        /// <param name="commandText">Text of the query to execute.</param>
        /// <param name="parameters">Parameters of the query.</param>
        public CommandSettings(CommandType commandType, string commandText, IEnumerable<IDbDataParameter> parameters)
        {
            SetProperties(commandType, commandText, parameters);
        }

        /// <summary>
        /// Gets the type of the query to execute.
        /// </summary>
        public CommandType CommandType { get { return _commandType; } }

        /// <summary>
        /// Gets the text of the query to execute.
        /// </summary>
        public string CommandText { get { return _commandText; } }

        /// <summary>
        /// Gets the parameters used in the query being executed.
        /// </summary>
        public IEnumerable<IDbDataParameter> Parameters { get { return _parameters; } }

        /// <summary>
        /// Set the object properties after testing for validity.
        /// </summary>
        /// <param name="commandType">Command type.</param>
        /// <param name="commandText">Command text.</param>
        /// <param name="parameters">List of parameters.</param>
        private void SetProperties(CommandType commandType, string commandText, IEnumerable<IDbDataParameter> parameters)
        {
            if (commandType == CommandType.TableDirect)
            {
                throw new ArgumentException("CommandType.TableDirect not supported.", nameof(commandType));
            }

            if (string.IsNullOrWhiteSpace(commandText))
            {
                throw new ArgumentException($"Command text is required.", nameof(commandText));
            }

            _commandType = commandType;
            _commandText = commandText;
            _parameters = parameters;
        }
    }
}
