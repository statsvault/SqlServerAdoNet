using System;
using System.Collections.Generic;
using System.Data;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class representing the properties for the set clause for an update statement.
    /// </summary>
    internal class UpdateProperties
    {
        /// <summary>
        /// Ctor.  New up the parameters collection
        /// </summary>
        public UpdateProperties() { Parameters = new List<IDbDataParameter>(); }

        /// <summary>
        /// Gets/sets the set clause for the update statement.
        /// </summary>
        public string SetClause { get; set; }

        /// <summary>
        /// Gets/sets the parameters for the set clause.
        /// </summary>
        public List<IDbDataParameter> Parameters { get; set; }
    }
}
