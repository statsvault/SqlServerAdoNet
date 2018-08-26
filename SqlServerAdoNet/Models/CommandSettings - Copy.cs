using System.Collections.Generic;
using System.Data;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class for database command properties.
    /// </summary>
    public class CommandSettings
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="commandText">Text of the query to execute.</param>
        public CommandSettings(string commandText)
        {
            Type = CommandType.Text;
            Text = commandText;
            Parameters = new List<IDbDataParameter>();
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="commandType">Type of the query to execute.</param>
        /// <param name="commandText">Text of the query to execute.</param>
        public CommandSettings(CommandType commandType, string commandText)
        {
            Type = commandType;
            Text = commandText;
            Parameters = new List<IDbDataParameter>();
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="commandText">Text of the query to execute.</param>
        /// <param name="parameters">Parameters of the query.</param>
        public CommandSettings(string commandText, IEnumerable<IDbDataParameter> parameters)
        {
            Type = CommandType.Text;
            Text = commandText;
            Parameters = parameters;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="commandType">Type of the query to execute.</param>
        /// <param name="commandText">Text of the query to execute.</param>
        /// <param name="parameters">Parameters of the query.</param>
        public CommandSettings(CommandType commandType, string commandText, IEnumerable<IDbDataParameter> parameters)
        {
            Type = commandType;
            Text = commandText;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the type of the query to execute.
        /// </summary>
        public CommandType Type { get; set; }

        /// <summary>
        /// Gets the text of the query to execute.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets the parameters used in the query being executed.
        /// </summary>
        public IEnumerable<IDbDataParameter> Parameters { get; set; }
    }
}
