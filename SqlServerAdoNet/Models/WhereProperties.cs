using System;
using System.Collections.Generic;
using System.Data;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class representing the properties for a where clause condition.
    /// </summary>
    internal class WhereProperties
    {
        /// <summary>
        /// Ctor.  New up the parameters collection.
        /// </summary>
        public WhereProperties() { Parameters = new List<IDbDataParameter>(); }

        /// <summary>
        /// Gets/sets the where clause condition.
        /// </summary>
        public string ConditionClause { get; set; }

        /// <summary>
        /// Gets/sets the paramters for the where clause condition.
        /// </summary>
        public List<IDbDataParameter> Parameters { get; set; }
    }
}
