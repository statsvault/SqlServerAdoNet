using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

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
            var name = type.GetAttributeValue((TableAttribute a) => a.Name);
            var schema = type.GetAttributeValue((TableAttribute a) => a.Schema);

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
                var tableColumn = new Column { Id = prop.Name };

                // See if the property has a Key attibute.
                SetIsPrimaryKey(type, prop, tableColumn);

                // See if the property has a DatabaseGeneratedAttribute attribute.
                SetDatabaseGeneratedFlags(type, prop, tableColumn);

                // See if the property has a Column attribute.
                SetColumnAttributes(type, prop, tableColumn);

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
        /// Set whether or not the column is a primary key.
        /// </summary>
        /// <param name="type">Type of the object.</param>
        /// <param name="prop">Property to check for KeyAttribute.</param>
        /// <param name="tableColumn">Column to set the IsPrimaryKey flag.</param>
        private static void SetIsPrimaryKey(Type type, PropertyInfo prop, Column tableColumn)
        {
            tableColumn.IsPrimaryKey = false;

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
        }

        /// <summary>
        /// Set the column database generated flags.
        /// </summary>
        /// <param name="type"Type of the object.></param>
        /// <param name="prop">Property to check for DatabaseGeneratedAttribute</param>
        /// <param name="tableColumn">Column to set database generated flags.</param>
        private static void SetDatabaseGeneratedFlags(Type type, PropertyInfo prop, Column tableColumn)
        {
            tableColumn.IsIdentity = false;
            tableColumn.IsComputed = false;

            var dbGenAttr = prop.GetAttributeValue((DatabaseGeneratedAttribute a) => a);
            if (dbGenAttr != null)
            {
                // It does, so get its constructor argument value.
                var dbGenOpt = dbGenAttr.DatabaseGeneratedOption;

                // Flag if the property is an identity or computed column.
                tableColumn.IsIdentity = dbGenOpt == DatabaseGeneratedOption.Identity;
                tableColumn.IsComputed = dbGenOpt == DatabaseGeneratedOption.Computed;
            }
        }

        /// <summary>
        /// Set the other column attributes like Name and SqlDbType.
        /// </summary>
        /// <param name="type">Type of the object.</param>
        /// <param name="prop">Property to check for ColumnAttribute.</param>
        /// <param name="tableColumn">Column to set column attribute values.</param>
        private static void SetColumnAttributes(Type type, PropertyInfo prop, Column tableColumn)
        {
            tableColumn.Name = prop.Name;
            tableColumn.SqlDbType = null;

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
        }

        /// <summary>
        /// Get the sql database type from the TypeName property.
        /// </summary>
        /// <param name="typeName">String version of SqlDbType.</param>
        /// <returns>SqlDbType</returns>
        private static SqlDbType? GetDbType(string typeName)
        {
            int value;
            if (int.TryParse(typeName, out value) &&
                !Enum.IsDefined(typeof(SqlDbType), value))
            {
                return null;
            }

            SqlDbType sqlType;
            if (Enum.TryParse<SqlDbType>(typeName, true, out sqlType))
            {
                return sqlType;
            }
            return null;
        }
    }
}
