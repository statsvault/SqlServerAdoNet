using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class for simple sql query building based on an entity model.
    /// </summary>
    /// <typeparam name="T">Entity model type.</typeparam>
    internal class QueryBuilder<T>
    {
        private readonly QueryType _queryType;
        private readonly string _tableName;
        private readonly List<Column> _entityColumns;
        private Dictionary<string, object> _entityPropertyValues = new Dictionary<string, object>();
        private Dictionary<string, object> _primaryKeyValues = new Dictionary<string, object>();
        private WhereProperties _whereProperties;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="queryType">Type of sql query to be built.</param>
        public QueryBuilder(QueryType queryType)
        {
            _queryType = queryType;

            // Get the column definitions from the entity.
            _entityColumns = ModelHelper.GetTableColumns<T>().ToList();
            
            if (!_entityColumns.Any())
            {
                throw new ModelDefinitionException("The model does not contain any public properties.");
            }

            // Get the table name from the entity.
            var tableName = ModelHelper.GetTableName<T>();
            _tableName = FormatTableName(tableName.Name, tableName.Schema);
        }

        /// <summary>
        /// Set the primary key values for the query.  Used for Select and Delete queries.
        /// </summary>
        /// <param name="primaryKeyValues">Primary key values, in the order they appear in the model.</param>
        public void SetPrimaryKeyValues(params object[] primaryKeyValues)
        {
            // This method is only used for Selects and Deletes.
            if (_queryType == QueryType.Insert || _queryType == QueryType.Update)
            {
                throw new InvalidOperationException("SetPrimaryKeyValues is not valid for Insert and Update.  Use SetEntityInstance instead.");
            }

            // Make sure we have at least one value.
            if (!primaryKeyValues.Any())
            {
                throw new ArgumentException("At least one primary key value is required.", nameof(primaryKeyValues));
            }

            // Get the model's primary key fields.
            var primaryKeyColumns = _entityColumns.Where(x => x.IsPrimaryKey).ToList();

            // Make sure the model has primary keys and that we have values for them.
            if (primaryKeyColumns.Count == 0)
            {
                throw new ModelDefinitionException("The model does not contain any primary keys.");
            }
            if (primaryKeyColumns.Count != primaryKeyValues.Length)
            {
                throw new ArgumentException("The number of primary key values supplied does not match the number of primary keys in the model.", nameof(primaryKeyValues));
            }

            // Build the collection of primary key/value pairs.
            _primaryKeyValues = new Dictionary<string, object>(primaryKeyColumns.Count);
            for (var i = 0; i < primaryKeyColumns.Count; i++)
            {
                _primaryKeyValues.Add(primaryKeyColumns[i].Id, primaryKeyValues[i]);
            }

            // Make the primary key where condition.
            MakeWhereProperties();
        }

        /// <summary>
        /// Set the entity instance to use for the query.  Used for Delete, Update, and Insert queries.
        /// </summary>
        /// <param name="entityInstance"></param>
        public void SetEntityInstance(T entityInstance)
        {
            // Method not valid for Selects.
            if (_queryType == QueryType.Select)
            {
                throw new InvalidOperationException("SetEntityInstance is not valid for Select.  Use SetPrimaryKeyValues instead.");
            }

            // Make sure an entity was passed in since we'll be using its property values.
            if (entityInstance == null)
            {
                throw new ArgumentNullException("A non-null entity instance is required.", nameof(entityInstance));
            }

            // Get the properties and their values from the entity instance.
            _entityPropertyValues = ReflectionHelper.GetPropertyValues(entityInstance);

            // Build the collection of primary key/value pairs.
            var primaryKeyColumns = _entityColumns.Where(x => x.IsPrimaryKey).ToList();

            _primaryKeyValues = new Dictionary<string, object>(primaryKeyColumns.Count);
            foreach (var keyCol in primaryKeyColumns)
            {
                _primaryKeyValues.Add(keyCol.Id, _entityPropertyValues[keyCol.Id]);
            }

            // Make the primary key where condition.
            MakeWhereProperties();
        }

        /// <summary>
        /// Build the query and parameters to use for the database command.
        /// </summary>
        /// <returns>CommandSettings</returns>
        public CommandSettings MakeCommandSettings()
        {
            switch (_queryType)
            {
                case QueryType.Delete: return MakeDeleteSettings();
                case QueryType.Insert: return MakeInsertSettings();
                case QueryType.Select: return MakeSelectSettings();
                case QueryType.Update: return MakeUpdateSettings();
            }
            throw new SqlBuilderException("Query type is not supported.");
        }

        /// <summary>
        /// Build the query and parameters to use for a Select command.
        /// </summary>
        /// <returns>CommandSettings</returns>
        private CommandSettings MakeSelectSettings()
        {
            // Get the column names to return in the select statement.
            var colNames = string.Join(", ", _entityColumns.Select(x => AddBrackets(x.Name))).RemoveFromEnd(", ");

            // If we don't have any primary key values then return a select all statement.
            if (_primaryKeyValues.Count == 0)
            {
                return new CommandSettings(string.Format("select {0} from {1};", colNames, _tableName));
            }

            return new CommandSettings(string.Format("select {0} from {1} where {2};",
                colNames, _tableName, _whereProperties.ConditionClause), _whereProperties.Parameters);
        }

        /// <summary>
        /// Build the query and parameters to use for a Delete command.
        /// </summary>
        /// <returns>CommandSettings</returns>
        private CommandSettings MakeDeleteSettings()
        {
            // The builder only allows deletes for primary keys.
            if (_primaryKeyValues.Count == 0)
            {
                throw new SqlBuilderException("Delete requires primary key columns and their values.");
            }
            return new CommandSettings(string.Format("delete from {0} where {1};",
                _tableName, _whereProperties.ConditionClause), _whereProperties.Parameters);
        }

        /// <summary>
        /// Build the query and parameters to use for an Update command.
        /// </summary>
        /// <returns>CommandSettings</returns>
        private CommandSettings MakeUpdateSettings()
        {
            // The builder only allows updates for primary keys.
            if (_primaryKeyValues.Count == 0)
            {
                throw new SqlBuilderException("Update requires primary key columns and their values.");
            }

            // Get the columns and parameters for the Set columns.
            var props = MakeUpdateProperties();

            var prms = new List<IDbDataParameter>(props.Parameters);
            prms.AddRange(_whereProperties.Parameters);

            return new CommandSettings(string.Format("update {0} set {1} where {2};",
                _tableName, props.SetClause, _whereProperties.ConditionClause), prms);
        }

        /// <summary>
        /// Build the query and parameters to use for an Insert command.
        /// </summary>
        /// <returns>CommandSettings</returns>
        private CommandSettings MakeInsertSettings()
        {
            // Get the columns/values and their parameters.
            var props = MakeInsertProperties();

            // See if we're dealing with a table that has an identity primary key.  If so, we'll
            // send back a different query.
            var identityColumn = _entityColumns.FirstOrDefault(x => x.IsPrimaryKey && x.IsIdentity);

            if (identityColumn != null)
            {
                return new CommandSettings(string.Format("insert into {0} ({1}) output inserted.{2} values ({3});",
                    _tableName, props.ColumnsClause, AddBrackets(identityColumn.Name), props.ValuesClause), props.Parameters);
            }

            return new CommandSettings(string.Format("insert into {0} ({1}) values ({2});",
                _tableName, props.ColumnsClause, props.ValuesClause), props.Parameters);
        }

        /// <summary>
        /// Make the update statement properties.
        /// </summary>
        /// <returns>UpdateProperties</returns>
        private UpdateProperties MakeUpdateProperties()
        {
            var props = new UpdateProperties();
            var setClause = new StringBuilder();

            foreach (var col in _entityColumns)
            {
                // Dont' update computed, identity, or primary key columns.
                if (col.IsComputed || col.IsIdentity || col.IsPrimaryKey)
                {
                    continue;
                }

                var prmName = QueryHelper.MakeParameterName(col.Name);
                setClause.Append(string.Format($"{AddBrackets(col.Name)} = {prmName}, "));
                props.Parameters.Add(MakeSqlParameter(prmName, _entityPropertyValues[col.Id].ToDBNull(), col.SqlDbType));
            }
            props.SetClause = setClause.ToString().RemoveFromEnd(", ");

            // Are there columns to update.
            if (string.IsNullOrEmpty(props.SetClause))
            {
                throw new SqlBuilderException("The model does not contain any updateable columns.");
            }
            return props;
        }

        /// <summary>
        /// Make the insert statement properties.
        /// </summary>
        /// <returns></returns>
        private InsertProperties MakeInsertProperties()
        {
            var props = new InsertProperties();
            var columnsClause = new StringBuilder();
            var valuesClause = new StringBuilder();

            foreach (var col in _entityColumns)
            {
                // Don't insert computed or identity columns.
                if (col.IsComputed || col.IsIdentity)
                {
                    continue;
                }

                var prmName = QueryHelper.MakeParameterName(col.Name);
                columnsClause.Append(AddCommaDelimiter(AddBrackets(col.Name)));
                valuesClause.Append(AddCommaDelimiter(prmName));
                props.Parameters.Add(MakeSqlParameter(prmName, _entityPropertyValues[col.Id].ToDBNull(), col.SqlDbType));
            }
            props.ColumnsClause = columnsClause.ToString().RemoveFromEnd(", ");
            props.ValuesClause = valuesClause.ToString().RemoveFromEnd(", ");

            // Are there columns to insert.
            if (string.IsNullOrEmpty(props.ColumnsClause))
            {
                throw new SqlBuilderException("The model does not contain any insertable columns.");
            }
            return props;
        }

        /// <summary>
        /// Make the where condition for the primary keys.
        /// </summary>
        private void MakeWhereProperties()
        {
            _whereProperties = new WhereProperties();
            var conditionClause = new StringBuilder();

            foreach (var primaryKey in _primaryKeyValues)
            {
                var col = _entityColumns.First(x => x.Id == primaryKey.Key);

                var prmName = QueryHelper.MakeParameterName(col.Name);
                conditionClause.Append($"{AddBrackets(col.Name)} = {prmName} and ");
                                
                _whereProperties.Parameters.Add(MakeSqlParameter(prmName, primaryKey.Value.ToDBNull(), col.SqlDbType));
            }
            _whereProperties.ConditionClause = conditionClause.ToString().RemoveFromEnd(" and ");
        }

        /// <summary>
        /// Add brackets around the value.
        /// </summary>
        /// <param name="val">Value to add brackets around.</param>
        /// <returns>string</returns>
        private string AddBrackets(string val)
        {
            return $"[{val}]";
        }

        /// <summary>
        /// Add a comma and space to the end of the value.
        /// </summary>
        /// <param name="val">Values to add delimiter.</param>
        /// <returns>string</returns>
        private string AddCommaDelimiter(string val)
        {
            return $"{val}, ";
        }

        /// <summary>
        /// Format the table name by appending the schema and wrapping in brackets.
        /// </summary>
        /// <param name="name">Table name.</param>
        /// <param name="schema">Table schema.</param>
        /// <returns>string</returns>
        private string FormatTableName(string name, string schema)
        {
            if (string.IsNullOrWhiteSpace(schema))
            {
                return AddBrackets(name); 
            }
            else
            {
                return $"{AddBrackets(schema)}.{AddBrackets(name)}";
            }
        }

        /// <summary>
        /// Make a sql parameter.
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <param name="value">Value of parameter.</param>
        /// <param name="dbType">Database type of parameter.</param>
        /// <returns>SqlParameter</returns>
        private SqlParameter MakeSqlParameter(string name, object value, SqlDbType? sqlDbType)
        {
            var param = new SqlParameter(name, value);
            if (sqlDbType.HasValue) param.SqlDbType = sqlDbType.Value;
            return param;
        }
    }
}
