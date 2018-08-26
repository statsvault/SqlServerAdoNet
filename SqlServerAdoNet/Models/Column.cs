using System.Data;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class for entity model column definition.
    /// </summary>
    internal class Column
    {
        /// <summary>
        /// Gets/sets the id of the column which is just the name of the property.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the name of the column.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets whether or not the column is a primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Gets/sets whether or not the column is an identity column.
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Gets/sets whether or not the column is a computed column.
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// Gets/sets the database type.
        /// </summary>
        public SqlDbType? SqlDbType { get; set; }
    }
}
