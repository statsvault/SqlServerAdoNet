using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using FastMember;

namespace StatKings.SqlServerAdoNet
{
    /// <summary>
    /// Helpers for building Sql Server queries.
    /// </summary>
    public static class QueryHelper
    {
        /// <summary>
        /// Convert an IEnumerable into a Sql Server table-valued parameter.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="data">Data values for the table.</param>
        /// <param name="columnNames">List of columns to add to the data.  Allows you to not include all of the classes columns.</param>
        /// <returns>IDbDataParameter</returns>
        public static IDbDataParameter MakeTableValuedParameter<T>(string paramName, IEnumerable<T> data, params string[] columnNames)
        {
            var prm = new SqlParameter(paramName, SqlDbType.Structured);

            using (var table = new DataTable())
            {
                // For simple types, create the data table ourselves, otherwise use FastMember.
                if (typeof(T).IsValueType || typeof(T).Equals(typeof(string)))
                {
                    var colName = (columnNames.Length > 0) ? columnNames[0] : "Value";
                    var dc = new DataColumn(colName);
                    dc.DataType = typeof(T);
                    table.Columns.Add(dc);

                    foreach (var item in data)
                    {
                        var dr = table.NewRow();
                        dr[0] = item;
                        table.Rows.Add(dr);
                    }
                }
                else
                {
                    // If the columns aren't specified then grab the list of columns from the model
                    // and pass them into FastMember.  If you don't specify the columns then FastMember
                    // will always put them in alpha order and we don't want that.
                    if (columnNames.Length == 0)
                    {
                        columnNames = ReflectionHelper.GetPropertyNames<T>().ToArray();
                    }

                    using (var reader = ObjectReader.Create(data, columnNames))
                    {
                        table.Load(reader);
                    }
                }

                prm.Value = table;
            }

            return prm;
        }

        /// <summary>
        /// Create a parameterized IN clause query.
        /// </summary>
        /// <typeparam name="T">Type of IN clause values.</typeparam>
        /// <param name="query">Query to parameterize.  Format should be "select * from table where column in ({0})".</param>
        /// <param name="inClauseData">Data values for IN clause parameters.</param>
        /// <returns>InClauseProperties</returns>
        public static InClauseProperties ParameterizeInClauseQuery<T>(string query, IEnumerable<T> inClauseData)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("A query is required.", nameof(query));
            }
            if (query.IndexOf("in ({0})", StringComparison.OrdinalIgnoreCase) == -1)
            {
                throw new ArgumentException("The query does not contain an IN clause.", nameof(query));
            }
            if (inClauseData == null || !inClauseData.Any())
            {
                throw new ArgumentException("Data for the IN clause is required.", nameof(inClauseData));
            }

            // Create the parameter names for the IN clause.
            var paramNames = inClauseData.Select((s, i) => MakeParameterName("paramtag" + i.ToString())).ToArray();

            // Join the parameter names with commas and replace them in the query placeholder.
            var outQuery = string.Format(query, String.Join(",", paramNames));

            // Create the IN clause parameters.
            var prms = new List<IDbDataParameter>();
            for (int i = 0; i < paramNames.Length; i++)
            {
                prms.Add(new SqlParameter(paramNames[i], inClauseData.ElementAt(i).ToDBNull()));
            }

            return new InClauseProperties() { Query = outQuery, Parameters = prms };
        }

        /// <summary>
        /// Convert null to DBNull.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>object</returns>
        public static object ToDBNull(this object value)
        {
            return value ?? DBNull.Value;
        }

        /// <summary>
        /// Make sql parameter name.
        /// </summary>
        /// <param name="paramName">Parameter name.</param>
        /// <returns>string</returns>
        internal static string MakeParameterName(string paramName)
        {
            return "@" + paramName;
        }
    }
}
