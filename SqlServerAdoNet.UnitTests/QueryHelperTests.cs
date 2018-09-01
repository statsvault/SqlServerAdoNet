using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;
using FluentAssertions;
using KellermanSoftware.CompareNetObjects;

namespace StatKings.SqlServerAdoNet.UnitTests
{
    /// <summary>
    /// Unit tests for StatKings.SqlServerAdoNet.QueryHelper.
    /// </summary>
    [TestFixture]
    public class QueryHelperTests
    {
        #region Private Members

        private const string TVP_PARAM_NAME = "@MyParam";
        private const string TVP_COL_NAME_DEFAULT = "Value";
        private const string TVP_COL_NAME_ID = "Id";
        private const string TVP_COL_NAME_MEMBER_A = "MemberA";
        private const string TVP_COL_NAME_MEMBER_B = "MemberB";

        private const string IN_CLAUSE_QUERY = "select * from MyTable where MyColumn in ({0});";
        private const string IN_CLAUSE_EXPECTED = "select * from MyTable where MyColumn in ({0},{1},{2});";
        private const string IN_CLAUSE_OUT_PRM_NAME_1 = "@paramtag0";
        private const string IN_CLAUSE_OUT_PRM_NAME_2 = "@paramtag1";
        private const string IN_CLAUSE_OUT_PRM_NAME_3 = "@paramtag2";

        private List<string> _inClauseInput;
        private List<int> _tvpInput;
        private List<PublicNotNullable> _twoPropertiesList;
        private List<PublicWithNullable> _nullablePropertyList;
        private List<PublicAndNonPublic> _privatePropertyList;
        private List<NoPublic> _noPublicPropertiesList;

        private DataTable _expectedDataTable;
        private DataTable _expectedEmptyDataTable;
        private DataTable _expectedTwoPropertiesDataTable;
        private DataTable _expectedNullableDataTable;
        private DataTable _expectedPrivatePropertyDataTable;

        #endregion

        /// <summary>
        /// Runs prior to each test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Make our list for testing parameterized in-clause queries.
            _inClauseInput = new List<string> { "apple", "banana", null };

            // Make our int for testing table-valued parameters.
            _tvpInput = new List<int> { 1, 2, 3 };

            // Make our expected simple int data table..
            _expectedDataTable = new DataTable();
            AddColumn(_expectedDataTable, TVP_COL_NAME_DEFAULT, typeof(int));
            AddRow(_expectedDataTable, _tvpInput[0]);
            AddRow(_expectedDataTable, _tvpInput[1]);
            AddRow(_expectedDataTable, _tvpInput[2]);

            // Make our simple empty data table.
            _expectedEmptyDataTable = new DataTable();
            AddColumn(_expectedEmptyDataTable, TVP_COL_NAME_DEFAULT, typeof(int));

            // Make our complex list using a class that does not have any nullable types.
            _twoPropertiesList = new List<PublicNotNullable>
            {
                new PublicNotNullable { MemberA = 1, MemberB = 2 },
                new PublicNotNullable { MemberA = 3, MemberB = 4 },
                new PublicNotNullable { MemberA = 5, MemberB = 6 }
            };

            // Make our complex non-nullable types data table.
            _expectedTwoPropertiesDataTable = new DataTable();
            AddColumn(_expectedTwoPropertiesDataTable, TVP_COL_NAME_MEMBER_A, typeof(int));
            AddColumn(_expectedTwoPropertiesDataTable, TVP_COL_NAME_MEMBER_B, typeof(int));
            AddRow(_expectedTwoPropertiesDataTable, _twoPropertiesList[0].MemberA, _twoPropertiesList[0].MemberB);
            AddRow(_expectedTwoPropertiesDataTable, _twoPropertiesList[1].MemberA, _twoPropertiesList[1].MemberB);
            AddRow(_expectedTwoPropertiesDataTable, _twoPropertiesList[2].MemberA, _twoPropertiesList[2].MemberB);

            // Make our complex list using a class that does have nullable types.
            _nullablePropertyList = new List<PublicWithNullable>
            {
                new PublicWithNullable { MemberA = 1, MemberB = null },
                new PublicWithNullable { MemberA = 3, MemberB = 4 },
                new PublicWithNullable { MemberA = 5, MemberB = null }
            };

            // Make our complex nullable types data table.
            _expectedNullableDataTable = new DataTable();
            AddColumn(_expectedNullableDataTable, TVP_COL_NAME_MEMBER_A, typeof(int));
            AddColumn(_expectedNullableDataTable, TVP_COL_NAME_MEMBER_B, typeof(int));
            AddRow(_expectedNullableDataTable, _nullablePropertyList[0].MemberA, _nullablePropertyList[0].MemberB);
            AddRow(_expectedNullableDataTable, _nullablePropertyList[1].MemberA, _nullablePropertyList[1].MemberB);
            AddRow(_expectedNullableDataTable, _nullablePropertyList[2].MemberA, _nullablePropertyList[2].MemberB);

            // Make our complex list using a class that has one public and one private property.
            _privatePropertyList = new List<PublicAndNonPublic>
            {
                new PublicAndNonPublic { MemberA = 1 },
                new PublicAndNonPublic { MemberA = 2 },
                new PublicAndNonPublic { MemberA = 3 }
            };

            // Make our complex one public/one private data table.
            _expectedPrivatePropertyDataTable = new DataTable();
            AddColumn(_expectedPrivatePropertyDataTable, TVP_COL_NAME_MEMBER_A, typeof(int));
            AddRow(_expectedPrivatePropertyDataTable, _privatePropertyList[0].MemberA);
            AddRow(_expectedPrivatePropertyDataTable, _privatePropertyList[1].MemberA);
            AddRow(_expectedPrivatePropertyDataTable, _privatePropertyList[2].MemberA);

            // Make our complex list using a class that has no public properties.
            _noPublicPropertiesList = new List<NoPublic>
            {
                new NoPublic(),
                new NoPublic(),
                new NoPublic(),
            };
        }

        #region MakeTabledValueParameter

        /// <summary>
        /// Test that the table-valued parameter has the expected name.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_ParameterName_HasExpectedName()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, _tvpInput) as SqlParameter;
            prm.ParameterName.Should().Be(TVP_PARAM_NAME);
        }

        /// <summary>
        /// Test that the table-valued parameter has the expected type.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_SqlDbTypeStructured_HasExpectedType()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, _tvpInput) as SqlParameter;
            prm.SqlDbType.Should().Be(SqlDbType.Structured);
        }

        /// <summary>
        /// Test that the table-valued parameter has a non-null data table.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_DataTable_HasDataTable()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, _tvpInput) as SqlParameter;
            prm.Value.Should().BeOfType<DataTable>().And.NotBeNull();
        }

        /// <summary>
        /// Test that the table-valued parameter data table has one column when based on a value type.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_ValueColumnCount_HasExpectedColumnCount()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, _tvpInput) as SqlParameter;
            ((DataTable)prm.Value).Columns.Should().HaveCount(1);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has one column when based on a class with one public property.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_OnePropertyColumnCount_HasExpectedColumnCount()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicAndNonPublic>(TVP_PARAM_NAME, _privatePropertyList) as SqlParameter;
            ((DataTable)prm.Value).Columns.Should().HaveCount(1);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has two columns when based on a class with two public properties.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_TwoPropertiesColumnCount_HasExpectedColumnCount()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicNotNullable>(TVP_PARAM_NAME, _twoPropertiesList) as SqlParameter;
            ((DataTable)prm.Value).Columns.Should().HaveCount(2);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has zero columns when based on a class with no public properties.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_NoPropertiesColumnCount_HasExpectedColumnCount()
        {
            var prm = QueryHelper.MakeTableValuedParameter<NoPublic>(TVP_PARAM_NAME, _noPublicPropertiesList) as SqlParameter;
            ((DataTable)prm.Value).Columns.Should().HaveCount(0);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has default column name when based on a value type.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_ValueColumnNameDefault_HasExpectedName()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, _tvpInput) as SqlParameter;
            AssertColumnNames((DataTable)prm.Value, TVP_COL_NAME_DEFAULT);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has specified column name when based on a value type.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_ValueColumnNameSpecified_HasExpectedName()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, _tvpInput, TVP_COL_NAME_ID) as SqlParameter;
            AssertColumnNames((DataTable)prm.Value, TVP_COL_NAME_ID);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has expected column name when based on a class with one public property.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_OnePropertyColumnName_HasExpectedName()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicAndNonPublic>(TVP_PARAM_NAME, _privatePropertyList) as SqlParameter;
            AssertColumnNames((DataTable)prm.Value, TVP_COL_NAME_MEMBER_A);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has expected column names when based on a class with two public properties.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_TwoPropertiesColumnName_HasExpectedNames()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicNotNullable>(TVP_PARAM_NAME, _twoPropertiesList) as SqlParameter;
            AssertColumnNames((DataTable)prm.Value, TVP_COL_NAME_MEMBER_A, TVP_COL_NAME_MEMBER_B);
        }

        /// <summary>
        /// Test that a single-column int, with default column name, table-valued parameter is returned with the correct rows.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_ListDefaultColumn_ExpectedDataTableReturned()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, _tvpInput) as SqlParameter;
            AssertDataTablesEqual((DataTable)prm.Value, _expectedDataTable);
        }

        /// <summary>
        /// Test that a single-column int, with specified column name, table-valued parameter is returned with the correct rows.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_ListSpecifiedColumn_ExpectedDataTableReturned()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, _tvpInput, TVP_COL_NAME_ID) as SqlParameter;
            AssertDataTablesEqual((DataTable)prm.Value, _expectedDataTable);
        }

        /// <summary>
        /// Test that an empty table-valued parameter is returned.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_EmptyList_ExpectedDataTableReturned()
        {
            var prm = QueryHelper.MakeTableValuedParameter<int>(TVP_PARAM_NAME, new List<int>()) as SqlParameter;
            AssertDataTablesEqual((DataTable)prm.Value, _expectedEmptyDataTable);
        }

        /// <summary>
        /// Test that a table-valued parameter based on a class with no nullable types is returned with the correct rows.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_TwoPropertiesList_ExpectedDataTableReturned()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicNotNullable>(TVP_PARAM_NAME, _twoPropertiesList) as SqlParameter;
            AssertDataTablesEqual((DataTable)prm.Value, _expectedTwoPropertiesDataTable);
        }

        /// <summary>
        /// Test that a table-valued parameter based on a class with nullable types is returned with the correct rows.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_NullablePropertyList_ExpectedDataTableReturned()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicWithNullable>(TVP_PARAM_NAME, _nullablePropertyList) as SqlParameter;
            AssertDataTablesEqual((DataTable)prm.Value, _expectedNullableDataTable);
        }

        /// <summary>
        /// Test that a table-valued parameter based on a class with one public property and one private property
        /// is returned with the correct rows.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_PrivatePropertyList_ExpectedDataTableReturned()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicAndNonPublic>(TVP_PARAM_NAME, _privatePropertyList) as SqlParameter;
            AssertDataTablesEqual((DataTable)prm.Value, _expectedPrivatePropertyDataTable);
        }

        /// <summary>
        /// Test that a table-valued parameter based on a class with no public properties is returned with the correct rows.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_NoPublicPropertiesList_ExpectedDataTableReturned()
        {
            var prm = QueryHelper.MakeTableValuedParameter<NoPublic>(TVP_PARAM_NAME, _noPublicPropertiesList) as SqlParameter;
            var table = prm.Value as DataTable;
            table.Columns.Should().HaveCount(0);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has expected column names when the columns are specified.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_TwoPropertiesOneName_HasExpectedNames()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicNotNullable>(TVP_PARAM_NAME, _twoPropertiesList, "MemberB") as SqlParameter;
            AssertColumnNames((DataTable)prm.Value, TVP_COL_NAME_MEMBER_B);
        }

        /// <summary>
        /// Test that the table-valued parameter data table has expected column names, and in a specific order, when the columns are specified.
        /// </summary>
        [Test]
        public void MakeTableValuedParameter_TwoPropertiesNamesInOrder_HasExpectedNames()
        {
            var prm = QueryHelper.MakeTableValuedParameter<PublicNotNullable>(TVP_PARAM_NAME, _twoPropertiesList, "MemberB", "MemberA") as SqlParameter;
            AssertColumnNames((DataTable)prm.Value, TVP_COL_NAME_MEMBER_B, TVP_COL_NAME_MEMBER_A);
        }

        /// <summary>
        /// Add data column to data table.
        /// </summary>
        /// <typeparam name="T">Data type of the column.</typeparam>
        /// <param name="table">Data table.</param>
        /// <param name="columnName">Column name.</param>
        private void AddColumn(DataTable table, string columnName, Type columnType)
        {
            var dc = new DataColumn(columnName, columnType);
            table.Columns.Add(dc);
        }

        /// <summary>
        /// Add data row with single column to data table.
        /// </summary>
        /// <param name="table">Data table.</param>
        /// <param name="value">Value of column.</param>
        private void AddRow(DataTable table, object value)
        {
            var dr = table.NewRow();
            dr[0] = value;
            table.Rows.Add(dr);
        }

        /// <summary>
        /// Add data row with two int columns to data table.
        /// </summary>
        /// <param name="table">Data table.</param>
        /// <param name="col1Value">Value of column 1.</param>
        /// <param name="col2Value">Value of column 2.</param>
        private void AddRow(DataTable table, int col1Value, int col2Value)
        {
            var dr = table.NewRow();
            dr[0] = col1Value;
            dr[1] = col2Value;
            table.Rows.Add(dr);
        }

        /// <summary>
        /// Add data row with one int column and one nullable int column to data table.
        /// </summary>
        /// <param name="table">Data table.</param>
        /// <param name="col1Value">Value of column 1.</param>
        /// <param name="col2Value">Value of column 2.</param>
        private void AddRow(DataTable table, int col1Value, int? col2Value)
        {
            var dr = table.NewRow();
            dr[0] = col1Value;
            dr[1] = (col2Value.HasValue) ? (object)col2Value : DBNull.Value;
            table.Rows.Add(dr);
        }

        /// <summary>
        /// Compare that the data in the actual data table match the data in the expected table.
        /// </summary>
        /// <param name="actual">Actual data table.</param>
        /// <param name="expected">Expected data table.</param>
        /// <returns>bool</returns>
        private bool CompareRows(DataTable actual, DataTable expected)
        {
            if (actual.Rows.Count != expected.Rows.Count)
            {
                return false;
            }

            if (actual.Rows.Count == 0 && expected.Rows.Count == 0)
            {
                return true;
            }

            for (var i = 0; i < actual.Rows.Count; i++)
            {
                var array1 = actual.Rows[i].ItemArray;
                var array2 = expected.Rows[i].ItemArray;

                if (!array1.SequenceEqual(array2))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Assert that the data tabe column names match the expected column names.
        /// </summary>
        /// <param name="table">Data table.</param>
        /// <param name="expectedColumnNamesInOrder">Expected column names in order that they should appear in the data table.</param>
        /// <returns>IEnumerable<string></returns>
        private void AssertColumnNames(DataTable table, params string[] expectedColumnNamesInOrder)
        {
            var columnNames = table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
            columnNames.Should().ContainInOrder(expectedColumnNamesInOrder);
        }

        /// <summary>
        /// Assert that the actual data table data matches the expected data table data.
        /// </summary>
        /// <param name="actual">Actual data table.</param>
        /// <param name="expected">Expected data table.</param>
        private void AssertDataTablesEqual(DataTable actual, DataTable expected)
        {
            CompareRows(actual, expected).Should().BeTrue();
        }

        /// <summary>
        /// Class with no nullable types.
        /// </summary>
        private class PublicNotNullable
        {
            public int MemberA { get; set; }

            public int MemberB { get; set; }
        }

        /// <summary>
        /// Class with a nullable type.
        /// </summary>
        private class PublicWithNullable
        {
            public int MemberA { get; set; }

            public int? MemberB { get; set; }
        }

        /// <summary>
        /// Class with one public property and one non-public property.
        /// </summary>
        private class PublicAndNonPublic
        {
            public int MemberA { get; set; }

            private int MemberB { get; set; }
        }

        /// <summary>
        /// Class with no public properties.
        /// </summary>
        private class NoPublic
        {
            private int MemberA { get; set; }

            private int MemberB { get; set; }
        }

        #endregion

        #region ParameterizeInClauseQuery

        /// <summary>
        /// Test that an ArgumentException is thrown when an invalid query or list of parameters is supplied.
        /// </summary>
        [TestCaseSource("ParameterizeInClauseQueryExceptionTestCases")]
        public void ParameterizeInClauseQuery_ArgumentExceptionThrown(string query, List<object> inData, string expectedResult)
        {
            Action act = () => QueryHelper.ParameterizeInClauseQuery(query, inData);
            act.Should().Throw<ArgumentException>().WithMessage(expectedResult);
        }

        /// <summary>
        /// Test cases for testing ArgumentExceptions for the ParameterizeInClauseQuery method.
        /// </summary>
        private static IEnumerable<TestCaseData> ParameterizeInClauseQueryExceptionTestCases()
        {
            var prmVals = new List<object> { "apple", "banana", "orange" };
            yield return new TestCaseData("", prmVals, "A query is required.\r\nParameter name: query");
            yield return new TestCaseData("  ", prmVals, "A query is required.\r\nParameter name: query");
            yield return new TestCaseData(null, prmVals, "A query is required.\r\nParameter name: query");
            yield return new TestCaseData("select * from MyTable;", prmVals, "The query does not contain an IN clause.\r\nParameter name: query");
            yield return new TestCaseData(IN_CLAUSE_QUERY, new List<object>(), "Data for the IN clause is required.\r\nParameter name: inClauseData");
            yield return new TestCaseData(IN_CLAUSE_QUERY, null, "Data for the IN clause is required.\r\nParameter name: inClauseData");
        }

        /// <summary>
        /// Test that the expected query is returned containing a parameterized IN clause.
        /// </summary>
        [Test]
        public void ParameterizeInClauseQuery_InClause_ExpectedQueryReturned()
        {
            var props = QueryHelper.ParameterizeInClauseQuery(IN_CLAUSE_QUERY, _inClauseInput);
            var expected = string.Format(IN_CLAUSE_EXPECTED, IN_CLAUSE_OUT_PRM_NAME_1, IN_CLAUSE_OUT_PRM_NAME_2, IN_CLAUSE_OUT_PRM_NAME_3);
            props.Query.Should().Be(expected);
        }

        /// <summary>
        /// Test that the expected parameters are returned for the parameterized IN clause.
        /// </summary>
        [Test]
        public void ParameterizeInClauseQuery_InClause_ExpectedParametersReturned()
        {
            var props = QueryHelper.ParameterizeInClauseQuery(IN_CLAUSE_QUERY, _inClauseInput);
            var expected = new List<IDbDataParameter>
            {
                new SqlParameter(IN_CLAUSE_OUT_PRM_NAME_1, _inClauseInput[0]),
                new SqlParameter(IN_CLAUSE_OUT_PRM_NAME_2, _inClauseInput[1]),
                new SqlParameter(IN_CLAUSE_OUT_PRM_NAME_3, DBNull.Value)
            };
            
            var compareLogic = new CompareLogic();
            var result = compareLogic.Compare(props.Parameters, expected);
            result.AreEqual.Should().BeTrue();
        }

        #endregion

        #region ToDBNull

        /// <summary>
        /// Test that the expected value is returned when converting to DBNull.Value if the value is null.
        /// </summary>
        [TestCaseSource("ToDBNullTestCases")]
        public void ToDBNull_ExpectedValueReturned(object value, object expectedResult)
        {
            value.ToDBNull().Should().Be(expectedResult);
        }

        /// <summary>
        /// ToDBNull test cases.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<TestCaseData> ToDBNullTestCases()
        {
            yield return new TestCaseData(1, 1);
            yield return new TestCaseData(1.5M, 1.5M);
            yield return new TestCaseData("hello world", "hello world");
            yield return new TestCaseData(null, DBNull.Value);
        }

        #endregion
    }
}
