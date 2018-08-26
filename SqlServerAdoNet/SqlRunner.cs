using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Data.SqlClient;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Class for executing database commands.
    /// </summary>
    public class SqlRunner : ISqlRunner
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="unitOfWork">Unit of work.</param>
        public SqlRunner(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }
            _unitOfWork = unitOfWork;
        }
        
        /// <summary>
        /// Executes the specified database command.  Typically used for saving data.
        /// </summary>
        /// <param name="commandSettings">Specifies the database command, along with it's parameters, to execute.</param>
        public void ExecuteNonQuery(CommandSettings commandSettings)
        {
            IDbCommand cmd = CreateCommand(commandSettings);
            try
            {
                cmd.ExecuteNonQuery();
                SetOutputParameters(cmd, commandSettings.Parameters);
            }
            catch (Exception ex)
            {
                throw HandleException(cmd, ex);
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Executes the specified database command and returns the value of the first column in the first row.
        /// </summary>
        /// <param name="commandSettings">Specifies the database command, along with it's parameters, to execute.</param>
        /// <returns>object</returns>
        public object ExecuteScalar(CommandSettings commandSettings)
        {
            IDbCommand cmd = CreateCommand(commandSettings);
            try
            {   
                var scalar = cmd.ExecuteScalar();
                SetOutputParameters(cmd, commandSettings.Parameters);
                return scalar;
            }
            catch (Exception ex)
            {
                throw HandleException(cmd, ex);
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a list of Entities for the specified database command.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="commandSettings">Specifies the database command, along with it's parameters, to execute.</param>
        /// <returns>IEnumerable of T</returns>
        public IEnumerable<T> ExecuteReader<T>(CommandSettings commandSettings)
        {
            IDbCommand cmd = CreateCommand(commandSettings);
            try
            {
                IEnumerable<T> results;
                using (var reader = cmd.ExecuteReader())
                {
                    results = MapToList<T>(reader);
                }
                SetOutputParameters(cmd, commandSettings.Parameters);
                return results;
            }
            catch (Exception ex)
            {
                throw HandleException(cmd, ex);
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a single Entity for the specified database command.  This will retrieve the first Entity found.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="commandSettings">Specifies the database command, along with it's parameters, to execute.</param>
        /// <returns>T</returns>
        public T ExecuteReaderFirst<T>(CommandSettings commandSettings)
        {
            return ExecuteReader<T>(commandSettings).FirstOrDefault();
        }

        /// <summary>
        /// Select a record.  The primary key values must be supplied in the order they appear in the entity.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="primaryKeyValues">Primary key values.</param>
        /// <returns>T</returns>
        public T Get<T>(params object[] primaryKeyValues)
        {
            var builder = new QueryBuilder<T>(QueryType.Select);
            builder.SetPrimaryKeyValues(primaryKeyValues);
            return ExecuteReaderFirst<T>(builder.MakeCommandSettings());
        }

        /// <summary>
        /// Select all records.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <returns>IEnumerable of T</returns>
        public IEnumerable<T> GetAll<T>()
        {
            var builder = new QueryBuilder<T>(QueryType.Select);
            return ExecuteReader<T>(builder.MakeCommandSettings());
        }

        /// <summary>
        /// Delete a record.  The primary key values must be supplied in the order they appear in the entity.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="primaryKeyValues">Primary key values.</param>
        public void Delete<T>(params object[] primaryKeyValues)
        {
            var builder = new QueryBuilder<T>(QueryType.Delete);
            builder.SetPrimaryKeyValues(primaryKeyValues);
            ExecuteNonQuery(builder.MakeCommandSettings());
        }

        /// <summary>
        /// Insert a record and get back the newly inserted identity id.  Calls ExecuteScalar.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="entity">Entity instance.</param>
        /// <returns>int</returns>
        public int InsertForId<T>(T entity)
        {
            var builder = new QueryBuilder<T>(QueryType.Insert);
            builder.SetEntityInstance(entity);
            var scalar = ExecuteScalar(builder.MakeCommandSettings());
            return Convert.ToInt32(scalar);
        }

        /// <summary>
        /// Insert a record.  Calls ExecuteNonQuery.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="entity">Entity instance.</param>
        public void Insert<T>(T entity)
        {
            var builder = new QueryBuilder<T>(QueryType.Insert);
            builder.SetEntityInstance(entity);
            ExecuteNonQuery(builder.MakeCommandSettings());
        }

        /// <summary>
        /// Update a record.  Calls ExecuteNonQuery.
        /// </summary>
        /// <typeparam name="T">Entity type.</typeparam>
        /// <param name="entity">Entity instance.</param>
        public void Update<T>(T entity)
        {
            var builder = new QueryBuilder<T>(QueryType.Update);
            builder.SetEntityInstance(entity);
            ExecuteNonQuery(builder.MakeCommandSettings());
        }

        /// <summary>
        /// Creates the database command object based on the specified settings.
        /// </summary>
        /// <param name="commandSettings">Specifies the database command, along with it's parameters, to execute.</param>
        /// <returns>IDbCommand</returns>
        private IDbCommand CreateCommand(CommandSettings commandSettings)
        {
            // Make sure the incoming command settings is valid.
            if (commandSettings == null)
            {
                throw new ArgumentNullException(nameof(commandSettings), $"{nameof(commandSettings)} is required.");
            }

            // Create the command object.
            var cmd = _unitOfWork.CreateCommand();
            cmd.CommandType = commandSettings.CommandType;
            cmd.CommandText = commandSettings.CommandText;

            // Add the parameters.
            foreach (var prm in commandSettings.Parameters)
            {
                cmd.Parameters.Add(prm);
            }

            return cmd;
        }

        /// <summary>
        /// Create the exception to send back to the caller.
        /// </summary>
        /// <param name="command">Database command object.</param>
        /// <param name="exception">Exception that occurred.</param>
        /// <returns>DataException</returns>
        private DataException HandleException(IDbCommand command, Exception exception)
        {
            var msg = new StringBuilder();
            msg.AppendLine("***** Exception Type *****");
            msg.AppendLine(exception.ToString());
            msg.AppendLine();
            msg.AppendLine("***** Message *****");
            msg.AppendLine(exception.Message);
            msg.AppendLine();
            msg.AppendLine("***** Stack Trace *****");
            msg.AppendLine(exception.StackTrace);
            msg.AppendLine();

            var inner = exception.InnerException;
            while (inner != null)
            {
                msg.AppendLine("***** Inner Exception *****");
                msg.AppendLine(inner.Message);
                msg.AppendLine();
                inner = inner.InnerException;
            }

            if (command != null && !string.IsNullOrWhiteSpace(command.CommandText))
            {
                msg.AppendLine("***** Query *****");
                msg.AppendLine(command.CommandText);
                msg.AppendLine();
                msg.AppendLine("***** Parameters *****");
                foreach (IDataParameter parameter in command.Parameters)
                {
                    msg.AppendFormat("{0}: {1}", parameter.ParameterName, parameter.Value);
                    msg.AppendLine();
                }
            }

            // Create a new DataException to send back, and add the detail to the Data collection.
            var de = new DataException(exception.Message);
            de.Data.Add("Detail", msg.ToString());
            return de;
        }

        /// <summary>
        /// Maps a data reader to a list of Entities.
        /// </summary>
        /// <typeparam name="T">Entity to map to.</typeparam>
        /// <param name="reader">Data reader to map from.</param>
        /// <returns>IEnumerable of T</returns>
        private IEnumerable<T> MapToList<T>(IDataReader reader)
        {
            // Loop through the reader, mapping each record.
            var items = new List<T>();
            while (reader.Read())
            {
                items.Add(Map<T>(reader));
            }
            return items;
        }

        /// <summary>
        /// Maps the data record to an Entity.
        /// </summary>
        /// <typeparam name="T">Entity to map to.</typeparam>
        /// <param name="record">Data record to map from.</param>
        /// <returns>T</returns>
        private T Map<T>(IDataRecord record)
        {
            var entity = Activator.CreateInstance<T>();
            var propColNames = ReflectionHelper.GetPropertyColumnNames<T>();

            // Loop through the columns of the record and set the Entity's properties.
            for (var i = 0; i < record.FieldCount; i++)
            {
                var recordName = record.GetName(i);
                var name = propColNames.First(x => x.Value == recordName).Key;

                // If the property doesn't exist, then move along.                
                var prop = ReflectionHelper.GetProperty<T>(name);
                if (prop == null) continue;

                // If the column value is null, then move along.
                var value = record.GetValue(i);
                if (value == DBNull.Value) continue;

                // Everything's good so set the property value.
                prop.SetValue(entity, value);
            }

            return entity;
        }

        /// <summary>
        /// Set the output parameters that came back from running the command.
        /// </summary>
        /// <param name="command">Database command.</param>
        /// <param name="parameters">Parameters collection that will be returned back to the caller.</param>
        private void SetOutputParameters(IDbCommand command, IEnumerable<IDbDataParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (var prm in parameters)
                {
                    if (prm.Direction != ParameterDirection.Input)
                    {
                        prm.Value = ((SqlParameter)command.Parameters[prm.ParameterName]).Value;
                    }
                }
            }
        }
    }
}
