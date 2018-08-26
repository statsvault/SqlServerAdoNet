using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Helpers for dealing with entity models.
    /// </summary>
    internal static class ModelHelper
    {
        /// <summary>
        /// Get the name of the database table associated with the Entity.
        /// </summary>
        /// <typeparam name="T">Type of the Entity.</typeparam>
        /// <returns>string</returns>
        public static TableName GetTableName<T>()
        {
            var type = typeof(T);

            var name = "";
            var schema = "";

            // First check if the entity is using the TableAttribute annotation.
            try
            {
                name = type.GetAttributeValue((TableAttribute a) => a.Name);
                schema = type.GetAttributeValue((TableAttribute a) => a.Schema);
            }
            catch
            {
                // Maybe the value was empty so just keep going.
            }

            // Next, see if the entity is postfixed with "DTO", and if so strip it off.
            if (string.IsNullOrWhiteSpace(name))
            {
                name = type.Name.RemoveFromEnd("dto");
            }

            return new TableName { Name = name, Schema = schema };
        }

        /// <summary>
        /// Get list of table columns based on the specified model's properties and annotations.
        /// </summary>
        /// <typeparam name="T">Type of the Entity.</typeparam>
        /// <returns>IEnumerable of Column</returns>
        public static IEnumerable<Column> GetTableColumns<T>()
        {
            var tableColumns = new List<Column>();

            // Get the entity's properties.
            var props = ReflectionHelper.GetProperties<T>();

            // Loop through the properties and create a Column object for each one that defines it characteristics.
            foreach (var prop in props)
            {
                var type = prop.PropertyType;

                var tableColumn = new Column { Id = prop.Name, Name = prop.Name };

                // See if the property has a Key attibute.
                var keyAttr = prop.GetAttributeValue((KeyAttribute a) => a);
                if (keyAttr != null)
                {
                    // Make sure the primary key isn't nullable. 
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        throw new ModelDefinitionException("The model has a nullable primary key.");
                    }

                    // This is a primary key so flag as such.
                    tableColumn.IsPrimaryKey = true;
                }

                // See if the property has a DatabaseGeneratedAttribute attribute.
                var dbGenAttr = prop.GetAttributeValue((DatabaseGeneratedAttribute a) => a);
                if (dbGenAttr != null)
                {
                    // It does, so get its constructor argument value.
                    var dbGenOpt = dbGenAttr.DatabaseGeneratedOption;
                
                    // Flag if the property is an identity or computed column.
                    if (dbGenOpt == DatabaseGeneratedOption.Identity)
                    {
                        tableColumn.IsIdentity = true;
                    }
                    else if (dbGenOpt == DatabaseGeneratedOption.Computed)
                    {
                        tableColumn.IsComputed = true;
                    }
                }

                // See if the property has a Column attribute.
                var colAttr = prop.GetAttributeValue((ColumnAttribute a) => a);
                if (colAttr != null)
                {
                    if (!string.IsNullOrWhiteSpace(colAttr.Name))
                    {
                        tableColumn.Name = colAttr.Name;
                    }

                    // See if it specifies the database type.
                    if (!string.IsNullOrWhiteSpace(colAttr.TypeName))
                    {
                        tableColumn.SqlDbType = GetDbType(colAttr.TypeName);
                    }
                }

                tableColumns.Add(tableColumn);
            }

            // Throw an exception if the model has more than one identity primary key.
            if (tableColumns.Count(x => x.IsPrimaryKey && x.IsIdentity) > 1)
            {
                throw new ModelDefinitionException("The model has multiple identity primary keys.");
            }

            // Throw an exception if the model has more than one column with the same name.
            if (tableColumns.GroupBy(x => x.Name).Any(g => g.Count() > 1))
            {
                throw new ModelDefinitionException("The model uses the same column name for multiple properties.");
            }

            return tableColumns;
        }

        /// <summary>
        /// Get the sql database type from the TypeName property.
        /// </summary>
        /// <param name="type">String version of SqlDbType.</param>
        /// <returns>SqlDbType</returns>
        private static SqlDbType? GetDbType(string type)
        {
            SqlDbType sqlType;
            if (Enum.TryParse<SqlDbType>(type, true, out sqlType))
            {
                return sqlType;
            }
            return null;
        }
    }
}
