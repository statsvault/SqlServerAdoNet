using System.Collections.Generic;
using System.Data;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class representing the properties for the columns/values clause for an insert statement.
    /// </summary>
    internal class InsertProperties
    {
        /// <summary>
        /// Ctor.  New up the parameters collection.
        /// </summary>
        public InsertProperties() { Parameters = new List<IDbDataParameter>(); }

        /// <summary>
        /// Gets/sets the column clause for the insert statement.
        /// </summary>
        public string Columns { get; set; }

        /// <summary>
        /// Gets/sets the values clause for the insert statement.
        /// </summary>
        public string Values { get; set; }

        /// <summary>
        /// Gets/sets the parameters for the values clause.
        /// </summary>
        public List<IDbDataParameter> Parameters { get; set; }
    }
}
