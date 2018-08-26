using System.Collections.Generic;
using System.Data;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class for "IN" clause properties.
    /// </summary>
    public class InClauseProperties
    {
        public InClauseProperties()
        {
            Parameters = new List<IDbDataParameter>();
        }

        /// <summary>
        /// Gets/sets the resulting query after parameterizing the "IN" clause.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets/sets the parameters for the "IN" clause.
        /// </summary>
        public IEnumerable<IDbDataParameter> Parameters { get; set; }
    }
}
