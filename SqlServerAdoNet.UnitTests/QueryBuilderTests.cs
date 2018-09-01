using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NUnit.Framework;
using FluentAssertions;
using KellermanSoftware.CompareNetObjects;

namespace StatKings.SqlServerAdoNet.UnitTests
{
    /// <summary>
    /// Unit tests for the StatKings.SqlServerAdoNet.QueryBuilder class.
    /// </summary>
    [TestFixture]
    public class QueryBuilderTests
    {
        /// <summary>
        /// Test that a ModelAnnotationException is thrown for a model that does not have any public properties.
        /// </summary>
        [TestCase(QueryType.Select)]
        [TestCase(QueryType.Delete)]
        [TestCase(QueryType.Insert)]
        [TestCase(QueryType.Update)]
        public void Ctor_NoPublicPropertiesModel_ModelAnnotationExceptionThrown(QueryType queryType)
        {
            Action act = () => new QueryBuilder<NoPublicProperties>(queryType);
            act.Should().Throw<ModelDefinitionException>().WithMessage(TestData.ERR_NO_PUB_PROPS);
        }
        
        #region SetPrimaryKeyValues

        /// <summary>
        /// Test that an exception is not thrown when calling SetPrimaryKeyValues with the correct number of values.
        /// </summary>
        [GenericTestCase(typeof(SinglePrimaryKey), QueryType.Select, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(SinglePrimaryKey), QueryType.Delete, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), QueryType.Select, TestData.KEY_1_VAL, TestData.KEY_2_VAL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), QueryType.Delete, TestData.KEY_1_VAL, TestData.KEY_2_VAL)]
        [GenericTestCase(typeof(WithColumnNames), QueryType.Select, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(WithColumnNames), QueryType.Delete, TestData.KEY_1_VAL)]
        public void SetPrimaryKeyValues_NoExceptionThrown<T>(QueryType queryType, params object[] primaryKeyValues)
        {
            Action act = SetPrimaryKeyValues_BuildAction<T>(queryType, primaryKeyValues);
            act.Should().NotThrow();
        }

        /// <summary>
        /// Test that a ModelAnnotationException is thrown when calling SetPrimaryKeyValues for a model with no primary key.
        /// </summary>
        [TestCase(QueryType.Select)]
        [TestCase(QueryType.Delete)]
        public void SetPrimaryKeyValues_ModelAnnotationExceptionThrown(QueryType queryType)
        {
            Action act = SetPrimaryKeyValues_BuildAction<NoPrimaryKey>(QueryType.Select, TestData.KEY_1_VAL);
            act.Should().Throw<ModelDefinitionException>().WithMessage(TestData.ERR_NO_KEYS);
        }

        /// <summary>
        /// Test that an ArgumentException is thrown when the setting the primary key values if no values are supplied, or
        /// the number of values does not match the number of keys in the model.
        /// </summary>
        [GenericTestCase(typeof(SinglePrimaryKey), QueryType.Select)]
        [GenericTestCase(typeof(SinglePrimaryKey), QueryType.Delete)]
        [GenericTestCase(typeof(SinglePrimaryKey), QueryType.Select, TestData.KEY_1_VAL, TestData.KEY_2_VAL)]
        [GenericTestCase(typeof(SinglePrimaryKey), QueryType.Delete, TestData.KEY_1_VAL, TestData.KEY_2_VAL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), QueryType.Select)]
        [GenericTestCase(typeof(MultiplePrimaryKey), QueryType.Delete)]
        [GenericTestCase(typeof(MultiplePrimaryKey), QueryType.Select, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), QueryType.Delete, TestData.KEY_1_VAL)]
        public void SetPrimaryKeyValues_ArgumentExceptionThrown<T>(QueryType queryType, params object[] primaryKeyValues)
        {
            // There are two argument exceptions that could be returned.
            var expectedMessage = (primaryKeyValues.Length == 0) ? TestData.ERR_KEY_REQ : TestData.ERR_KEY_MISMATCH;
            Action act = SetPrimaryKeyValues_BuildAction<T>(queryType, primaryKeyValues);
            act.Should().Throw<ArgumentException>().WithMessage(expectedMessage);
        }

        /// <summary>
        /// Test that an InvalidOperationException is thrown when calling SetPrimaryKeyValues for Inserts and Updates.
        /// </summary>
        [TestCase(QueryType.Insert)]
        [TestCase(QueryType.Update)]
        public void SetPrimaryKeyValues_InvalidOperationExceptionThrown(QueryType queryType)
        {
            Action act = SetPrimaryKeyValues_BuildAction<SinglePrimaryKey>(queryType, TestData.KEY_1_VAL);
            act.Should().Throw<InvalidOperationException>().WithMessage(TestData.ERR_SET_PRIMARY_KEY);
        }

        /// <summary>
        /// Make Action delegate for SetPrimaryKeyValues tests.
        /// </summary>
        private Action SetPrimaryKeyValues_BuildAction<T>(QueryType queryType, params object[] primaryKeyValues)
        {
            var builder = new QueryBuilder<T>(queryType);
            Action act = () => builder.SetPrimaryKeyValues(primaryKeyValues);
            return act;
        }

        #endregion

        #region SetEntityInstance

        /// <summary>
        /// Test that an exception is not thrown when calling SetEntityInstance with an initialized entity instance.
        /// </summary>
        [TestCase(QueryType.Insert)]
        [TestCase(QueryType.Update)]
        [TestCase(QueryType.Delete)]
        public void SetEntityInstance_NoExceptionThrown(QueryType queryType)
        {
            Action act = SetEntityInstance_BuildAction(queryType, TestData.MakeSinglePrimaryKeyInstance());
            act.Should().NotThrow();
        }

        /// <summary>
        /// Test that an ArgumentNullException is thrown when calling SetEntityInstance with a null instance.
        /// </summary>
        [TestCase(QueryType.Insert)]
        [TestCase(QueryType.Update)]
        [TestCase(QueryType.Delete)]
        public void SetEntityInstance_ArgumentNullExceptionThrown(QueryType queryType)
        {
            Action act = SetEntityInstance_BuildAction(queryType, null);
            act.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Test that an InvalidOperationException is thrown when calling SetEntityInstance for Selects.
        /// </summary>
        [Test]
        public void SetEntityInstance_InvalidOperationExceptionThrown()
        {
            Action act = SetEntityInstance_BuildAction(QueryType.Select, TestData.MakeSinglePrimaryKeyInstance());
            act.Should().Throw<InvalidOperationException>().WithMessage(TestData.ERR_SET_INSTANCE);
        }

        /// <summary>
        /// Make Action delegate for SetEntityInstance tests.
        /// </summary>
        private Action SetEntityInstance_BuildAction(QueryType queryType, SinglePrimaryKey entity)
        {
            var builder = new QueryBuilder<SinglePrimaryKey>(queryType);
            Action act = () => builder.SetEntityInstance(entity);
            return act;
        }

        #endregion

        #region MakeCommandSettings

        /// <summary>
        /// Test that the expected command text is returned.
        /// </summary>
        [TestCaseSource(typeof(TestData), "MakeExpectedTextTestCases")]
        public void MakeCommandSettings_SetEntityInstance_ExpectedTextReturned<T>(QueryType queryType, T entityInstance, string expectedResult)
        {
            var settings = GetEntityInstanceCommandSettings<T>(queryType, entityInstance);
            settings.CommandText.Should().Be(expectedResult);
        }

        /// <summary>
        /// Test that the expected command parameters are returned.
        /// </summary>
        [TestCaseSource(typeof(TestData), "MakeExpectedParametersTestCases")]
        public void MakeCommandSettings_SetEntityInstance_ExpectedParametersReturned<T>(QueryType queryType, T entityInstance, List<IDbDataParameter> expectedResult)
        {
            var settings = GetEntityInstanceCommandSettings<T>(queryType, entityInstance);
            AreEqual(settings.Parameters, expectedResult).Should().BeTrue();
        }

        /// <summary>
        /// Test that the expected command text is returned for a Select command.
        /// </summary>
        [GenericTestCase(typeof(SinglePrimaryKey), TestData.SEL_SINGLE_KEY_ALL)]
        [GenericTestCase(typeof(SinglePrimaryKey), TestData.SEL_SINGLE_KEY, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), TestData.SEL_MULTI_KEY_ALL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), TestData.SEL_MULTI_KEY, TestData.KEY_1_VAL, TestData.KEY_2_VAL)]
        [GenericTestCase(typeof(IdentityComputedColumns), TestData.SEL_IDENT_COMP_ALL)]
        [GenericTestCase(typeof(IdentityComputedColumns), TestData.SEL_IDENT_COMP, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(WithTableSchema), TestData.SEL_WITH_SCHEMA_ALL)]
        [GenericTestCase(typeof(WithColumnNames), TestData.SEL_WITH_COL_NAMES, TestData.KEY_1_VAL)]
        public void MakeCommandSettings_Select_SetPrimaryKeyValues_ExpectedCommandTextReturned<T>(string expectedResult, params object[] primaryKeyValues)
        {
            var settings = GetPrimaryKeyCommandSettings<T>(QueryType.Select, primaryKeyValues);
            settings.CommandText.Should().Be(expectedResult);
        }

        /// <summary>
        /// Test that the expected command parameters are returned for a Select command.
        /// </summary>
        [GenericTestCase(typeof(NoPrimaryKey), null)]
        [GenericTestCase(typeof(SinglePrimaryKey), null)]
        [GenericTestCase(typeof(SinglePrimaryKey), TestData.KEY_SINGLE_LIST, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), TestData.KEY_MULTI_LIST, TestData.KEY_1_VAL, TestData.KEY_2_VAL)]
        [GenericTestCase(typeof(WithColumnNames), TestData.KEY_SINGLE_LIST_OTH, TestData.KEY_1_VAL)]
        public void MakeCommandSettings_Select_SetPrimaryKeyValues_ExpectedParametersReturned<T>(string expectedParamNames, params object[] primaryKeyValues)
        {
            var settings = GetPrimaryKeyCommandSettings<T>(QueryType.Select, primaryKeyValues);
            var expectedResult = GetPrimaryKeyExpectedParameters(expectedParamNames, primaryKeyValues);
            AreEqual(settings.Parameters, expectedResult).Should().BeTrue();
        }

        /// <summary>
        /// Test that the expected command text is returned for a Delete command where the primary key values are passed in.
        /// </summary>
        [GenericTestCase(typeof(SinglePrimaryKey), TestData.DEL_SINGLE_KEY, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), TestData.DEL_MULTI_KEY, TestData.KEY_1_VAL, TestData.KEY_2_VAL)]
        [GenericTestCase(typeof(WithTableSchema), TestData.DEL_WITH_SCHEMA, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(WithColumnNames), TestData.DEL_WITH_COL_NAMES, TestData.KEY_1_VAL)]
        public void MakeCommandSettings_Delete_SetPrimaryKeyValues_ExpectedCommandTextReturned<T>(string expectedResult, params object[] primaryKeyValues)
        {
            var settings = GetPrimaryKeyCommandSettings<T>(QueryType.Delete, primaryKeyValues);
            settings.CommandText.Should().Be(expectedResult);
        }

        /// <summary>
        /// Test that the expected command parameters are returned for a Delete command where the primary key values are passed in.
        /// </summary>
        [GenericTestCase(typeof(SinglePrimaryKey), TestData.KEY_SINGLE_LIST, TestData.KEY_1_VAL)]
        [GenericTestCase(typeof(MultiplePrimaryKey), TestData.KEY_MULTI_LIST, TestData.KEY_1_VAL, TestData.KEY_2_VAL)]
        [GenericTestCase(typeof(WithColumnNames), TestData.KEY_SINGLE_LIST_OTH, TestData.KEY_1_VAL)]
        public void MakeCommandSettings_Delete_SetPrimaryKeyValues_ExpectedParametersReturned<T>(string expectedParamNames, params object[] primaryKeyValues)
        {
            var settings = GetPrimaryKeyCommandSettings<T>(QueryType.Delete, primaryKeyValues);
            var expectedResult = GetPrimaryKeyExpectedParameters(expectedParamNames, primaryKeyValues);
            AreEqual(settings.Parameters, expectedResult).Should().BeTrue();
        }

        #region Exceptions

        /// <summary>
        /// Test that a SqlBuilderException is thrown when no primary keys are supplied.
        /// </summary>
        [GenericTestCase(typeof(NoPrimaryKey))]
        [GenericTestCase(typeof(SinglePrimaryKey))]
        [GenericTestCase(typeof(MultiplePrimaryKey))]
        public void MakeCommandSettings_Delete_SqlBuilderExceptionThrown<T>()
        {
            var builder = new QueryBuilder<T>(QueryType.Delete);
            Action act = () => builder.MakeCommandSettings();
            act.Should().Throw<SqlBuilderException>().WithMessage(TestData.ERR_DEL_KEY_REQ);
        }

        /// <summary>
        /// Test that a SqlBuilderException is thrown for an Update command when an entity instance has not been set.
        /// </summary>
        [Test]
        public void MakeCommandSettings_Update_SinglePrimaryKey_SqlBuilderExceptionThrown()
        {
            var builder = new QueryBuilder<SinglePrimaryKey>(QueryType.Update);
            Action act = () => builder.MakeCommandSettings();
            act.Should().Throw<SqlBuilderException>().WithMessage(TestData.ERR_UPD_KEY);
        }

        /// <summary>
        /// Test that a SqlBuilderException is thrown for an Update command when for an entity instance that
        /// does not have any primary keys.
        /// </summary>
        [Test]
        public void MakeCommandSettings_Update_NoPrimaryKey_SqlBuilderExceptionThrown()
        {
            Action act = () => GetEntityInstanceCommandSettings<NoPrimaryKey>(QueryType.Update, new NoPrimaryKey());
            act.Should().Throw<SqlBuilderException>().WithMessage(TestData.ERR_UPD_KEY);
        }

        /// <summary>
        /// Test that a SqlBuilderException is thrown for a class instance that only has identity and computed properties,
        /// and hence, nothing to update.
        /// </summary>
        [Test]
        public void MakeCommandSettings_Update_IdentityComputedOnlyWithKey_SqlBuilderExceptionThrown()
        {
            Action act = () => GetEntityInstanceCommandSettings<IdentityComputedOnlyWithKey>(QueryType.Update, TestData.MakeIdentityComputedOnlyWithKeyInstance());
            act.Should().Throw<SqlBuilderException>().WithMessage(TestData.ERR_UPD_NO_COLS);
        }

        /// <summary>
        /// Insert query.  Test that a SqlBuilderException is thrown for a class instance that only has identity and computed properties,
        /// and hence, nothing to insert.
        /// </summary>
        [Test]
        public void MakeCommandSettings_Insert_IdentityComputedOnlyWithoutKey_SqlBuilderExceptionThrown()
        {
            Action act = () => GetEntityInstanceCommandSettings(QueryType.Insert, TestData.MakeIdentityComputedOnlyWithoutKeyInstance());
            act.Should().Throw<SqlBuilderException>().WithMessage(TestData.ERR_INS_NO_COLS);
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the CommandSettings for a command when calling SetPrimaryKeyValues.
        /// </summary>
        private CommandSettings GetPrimaryKeyCommandSettings<T>(QueryType queryType, params object[] primaryKeyValues)
        {
            var builder = new QueryBuilder<T>(queryType);
            if (primaryKeyValues.Length > 0)
            {
                builder.SetPrimaryKeyValues(primaryKeyValues);
            }
            return builder.MakeCommandSettings();
        }

        /// <summary>
        /// Get the list of expected primary key values.
        /// </summary>
        private List<IDbDataParameter> GetPrimaryKeyExpectedParameters(string paramNames, params object[] primaryKeyValues)
        {
            var list = new List<IDbDataParameter>();
            if (primaryKeyValues.Length > 0)
            {
                // paramNames is expected to be a comma-delimited list of parameter names.
                var names = paramNames.Split(',');

                for (var i = 0; i < primaryKeyValues.Length; i++)
                {
                    list.Add(new SqlParameter
                    {
                        ParameterName = names[i].Trim(),
                        Value = primaryKeyValues[i]
                    });
                }
            }
            return list;
        }
        
        /// <summary>
        /// Get CommandSettings for an entity instance.
        /// </summary>
        private CommandSettings GetEntityInstanceCommandSettings<T>(QueryType queryType, T entity)
        {
            var builder = new QueryBuilder<T>(queryType);
            builder.SetEntityInstance(entity);
            return builder.MakeCommandSettings();
        }

        private bool AreEqual<TA, TE>(TA actual, TE expected)
        {
            var compareLogic = new CompareLogic();
            var result = compareLogic.Compare(actual, expected);
            return result.AreEqual;
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Class containing no public properties.
        /// </summary>
        [Table("MyTable")]
        private class NoPublicProperties
        {
            private string Prop1 { get; set; }
            internal string Prop2 { get; set; }
        }

        /// <summary>
        /// Class containing no primary key properties.
        /// </summary>
        [Table("MyTable")]
        private class NoPrimaryKey
        {
            public string Prop1 { get; set; }

            public string Prop2 { get; set; }
        }

        /// <summary>
        /// Class containing a single, identity primary key property.
        /// </summary>
        [Table("MyTable")]
        private class SinglePrimaryKey
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Key1 { get; set; }

            public string Prop1 { get; set; }

            public string Prop2 { get; set; }
        }

        /// <summary>
        /// Class containing multiple primary key properties.
        /// </summary>
        [Table("MyTable")]
        private class MultiplePrimaryKey
        {
            [Key]
            public int Key1 { get; set; }

            [Key]
            public int Key2 { get; set; }

            public string Prop1 { get; set; }

            public string Prop2 { get; set; }
        }

        /// <summary>
        /// Class containing identity and computed properties.
        /// </summary>
        [Table("MyTable")]
        private class IdentityComputedColumns
        {
            [Key]
            public int Key1 { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Ident1 { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public int Comp1 { get; set; }

            public string Prop1 { get; set; }

        }

        /// <summary>
        /// Class containing only identity and computed properties, with exception of primary key.
        /// </summary>
        [Table("MyTable")]
        private class IdentityComputedOnlyWithKey
        {
            [Key]
            public int Key1 { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Ident1 { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public int Comp1 { get; set; }
        }

        /// <summary>
        /// Class containing only identity and computed properties.
        /// </summary>
        [Table("MyTable")]
        private class IdentityComputedOnlyWithoutKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Ident1 { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public int Comp1 { get; set; }
        }

        /// <summary>
        /// Class containing a table schema attribute.
        /// </summary>
        [Table("MyTable", Schema = "myschema")]
        private class WithTableSchema
        {
            [Key]
            public int Key1 { get; set; }

            public string Prop1 { get; set; }
        }

        /// <summary>
        /// Class containing propeties with the ColumnAttribute Name property set.
        /// </summary>
        [Table("MyTable")]
        private class WithColumnNames
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column("Column1")]
            public int Key1 { get; set; }

            [Column("Column2", TypeName = "varchar")]
            public string Prop1 { get; set; }

            [Column(name: "Column3", TypeName = "nvarchar")]
            public string Prop2 { get; set; }
        }

        /// <summary>
        /// Class for test data.
        /// </summary>
        private static class TestData
        {
            public const string KEY_SINGLE_LIST = "@Key1";
            public const string KEY_SINGLE_LIST_OTH = "@Column1";
            public const string KEY_MULTI_LIST = "@Key1,@Key2";
            public const string KEY_1_PRM = "@Key1";
            public const string KEY_1_PRM_OTH = "@Column1";
            public const int KEY_1_VAL = 1;
            public const string KEY_2_PRM = "@Key2";
            public const int KEY_2_VAL = 2;
            public const string PROP_1_PRM = "@Prop1";
            public const string PROP_1_PRM_OTH = "@Column2";
            public const string PROP_1_VAL = "some_string";
            public const string PROP_2_PRM = "@Prop2";
            public const string PROP_2_PRM_OTH = "@Column3";
            public const string PROP_2_VAL = "another_string";
            public const int IDENT_1_VAL = 100;
            public const int COMP_1_VAL = 200;

            public const string ERR_NO_PUB_PROPS = "The model does not contain any public properties.";
            public const string ERR_NO_KEYS = "The model does not contain any primary keys.";
            public const string ERR_KEY_REQ = "At least one primary key value is required.\r\nParameter name: primaryKeyValues";
            public const string ERR_KEY_MISMATCH = "The number of primary key values supplied does not match the number of primary keys in the model.\r\nParameter name: primaryKeyValues";
            public const string ERR_SET_PRIMARY_KEY = "SetPrimaryKeyValues is not valid for Insert and Update.  Use SetEntityInstance instead.";
            public const string ERR_SET_INSTANCE = "SetEntityInstance is not valid for Select.  Use SetPrimaryKeyValues instead.";
            public const string ERR_UPD_KEY = "Update requires primary key columns and their values.";
            public const string ERR_UPD_NO_COLS = "The model does not contain any updateable columns.";
            public const string ERR_INS_NO_COLS = "The model does not contain any insertable columns.";
            public const string ERR_DEL_KEY_REQ = "Delete requires primary key columns and their values.";

            public const string SEL_SINGLE_KEY_ALL = "select [Key1], [Prop1], [Prop2] from [MyTable];";
            public const string SEL_SINGLE_KEY = "select [Key1], [Prop1], [Prop2] from [MyTable] where [Key1] = @Key1;";
            public const string SEL_MULTI_KEY_ALL = "select [Key1], [Key2], [Prop1], [Prop2] from [MyTable];";
            public const string SEL_MULTI_KEY = "select [Key1], [Key2], [Prop1], [Prop2] from [MyTable] where [Key1] = @Key1 and [Key2] = @Key2;";
            public const string SEL_IDENT_COMP_ALL = "select [Key1], [Ident1], [Comp1], [Prop1] from [MyTable];";
            public const string SEL_IDENT_COMP = "select [Key1], [Ident1], [Comp1], [Prop1] from [MyTable] where [Key1] = @Key1;";
            public const string SEL_WITH_SCHEMA_ALL = "select [Key1], [Prop1] from [myschema].[MyTable];";
            public const string SEL_WITH_COL_NAMES = "select [Column1], [Column2], [Column3] from [MyTable] where [Column1] = @Column1;";

            public const string DEL_SINGLE_KEY = "delete from [MyTable] where [Key1] = @Key1;";
            public const string DEL_MULTI_KEY = "delete from [MyTable] where [Key1] = @Key1 and [Key2] = @Key2;";
            public const string DEL_WITH_SCHEMA = "delete from [myschema].[MyTable] where [Key1] = @Key1;";
            public const string DEL_WITH_COL_NAMES = "delete from [MyTable] where [Column1] = @Column1;";
            public const string UPD_SINGLE_KEY = "update [MyTable] set [Prop1] = @Prop1, [Prop2] = @Prop2 where [Key1] = @Key1;";
            public const string UPD_MULTI_KEY = "update [MyTable] set [Prop1] = @Prop1, [Prop2] = @Prop2 where [Key1] = @Key1 and [Key2] = @Key2;";
            public const string UPD_IDENT_COMP = "update [MyTable] set [Prop1] = @Prop1 where [Key1] = @Key1;";
            public const string UPD_WITH_SCHEMA = "update [myschema].[MyTable] set [Prop1] = @Prop1 where [Key1] = @Key1;";
            public const string UPD_WITH_COL_NAMES = "update [MyTable] set [Column2] = @Column2, [Column3] = @Column3 where [Column1] = @Column1;";
            public const string INS_SINGLE_KEY = "insert into [MyTable] ([Prop1], [Prop2]) output inserted.[Key1] values (@Prop1, @Prop2);";
            public const string INS_MULTI_KEY = "insert into [MyTable] ([Key1], [Key2], [Prop1], [Prop2]) values (@Key1, @Key2, @Prop1, @Prop2);";
            public const string INS_IDENT_COMP = "insert into [MyTable] ([Key1], [Prop1]) values (@Key1, @Prop1);";
            public const string INS_WITH_SCHEMA = "insert into [myschema].[MyTable] ([Key1], [Prop1]) values (@Key1, @Prop1);";
            public const string INS_WITH_COL_NAMES = "insert into [MyTable] ([Column2], [Column3]) output inserted.[Column1] values (@Column2, @Column3);";
            
            public static IEnumerable<TestCaseData> MakeExpectedTextTestCases()
            {
                yield return new TestCaseData(QueryType.Delete, MakeSinglePrimaryKeyInstance(), DEL_SINGLE_KEY);
                yield return new TestCaseData(QueryType.Delete, MakeMultiplePrimaryKeyInstance(), DEL_MULTI_KEY);
                yield return new TestCaseData(QueryType.Delete, MakeWithTableSchemaInstance(), DEL_WITH_SCHEMA);
                yield return new TestCaseData(QueryType.Delete, MakeWithColumnNamesInstance(), DEL_WITH_COL_NAMES);
                yield return new TestCaseData(QueryType.Update, MakeSinglePrimaryKeyInstance(), UPD_SINGLE_KEY);
                yield return new TestCaseData(QueryType.Update, MakeMultiplePrimaryKeyInstance(), UPD_MULTI_KEY);
                yield return new TestCaseData(QueryType.Update, MakeIdentityComputedColumnsInstance(), UPD_IDENT_COMP);
                yield return new TestCaseData(QueryType.Update, MakeWithTableSchemaInstance(), UPD_WITH_SCHEMA);
                yield return new TestCaseData(QueryType.Update, MakeWithColumnNamesInstance(), UPD_WITH_COL_NAMES);
                yield return new TestCaseData(QueryType.Insert, MakeSinglePrimaryKeyInstance(), INS_SINGLE_KEY);
                yield return new TestCaseData(QueryType.Insert, MakeMultiplePrimaryKeyInstance(), INS_MULTI_KEY);
                yield return new TestCaseData(QueryType.Insert, MakeIdentityComputedColumnsInstance(), INS_IDENT_COMP);
                yield return new TestCaseData(QueryType.Insert, MakeWithTableSchemaInstance(), INS_WITH_SCHEMA);
                yield return new TestCaseData(QueryType.Insert, MakeSinglePrimaryKeyWithNullInstance(), INS_SINGLE_KEY);
                yield return new TestCaseData(QueryType.Insert, MakeWithColumnNamesInstance(), INS_WITH_COL_NAMES);
            }

            public static IEnumerable<TestCaseData> MakeExpectedParametersTestCases()
            {
                yield return new TestCaseData(QueryType.Delete, MakeSinglePrimaryKeyInstance(), MakeExpectedSinglePrimaryKeyParams());
                yield return new TestCaseData(QueryType.Delete, MakeMultiplePrimaryKeyInstance(), MakeExpectedMultiplePrimaryKeyParams());
                yield return new TestCaseData(QueryType.Delete, MakeWithColumnNamesInstance(), MakeExpectedWithColumnNamesParams());
                yield return new TestCaseData(QueryType.Update, MakeSinglePrimaryKeyInstance(), MakeExpectedSinglePrimaryKeyParamsUpd());
                yield return new TestCaseData(QueryType.Update, MakeMultiplePrimaryKeyInstance(), MakeExpectedMultiplePrimaryKeyParamsUpd());
                yield return new TestCaseData(QueryType.Update, MakeIdentityComputedColumnsInstance(), MakeExpectedIdentityComputedColumnsParamsUpd());
                yield return new TestCaseData(QueryType.Update, MakeSinglePrimaryKeyWithNullInstance(), MakeExpectedSinglePrimaryKeyWithNullParamsUpd());
                yield return new TestCaseData(QueryType.Update, MakeWithColumnNamesInstance(), MakeExpectedWithColumnNamesParamsUpd());
                yield return new TestCaseData(QueryType.Insert, MakeSinglePrimaryKeyInstance(), MakeExpectedSinglePrimaryKeyParamsIns());
                yield return new TestCaseData(QueryType.Insert, MakeMultiplePrimaryKeyInstance(), MakeExpectedMultiplePrimaryKeyParamsIns());
                yield return new TestCaseData(QueryType.Insert, MakeIdentityComputedColumnsInstance(), MakeExpectedIdentityComputedColumnsParamsIns());
                yield return new TestCaseData(QueryType.Insert, MakeSinglePrimaryKeyWithNullInstance(), MakeExpectedSinglePrimaryKeyWithNullParamsIns());
                yield return new TestCaseData(QueryType.Insert, MakeWithColumnNamesInstance(), MakeExpectedWithColumnNamesParamsIns());
            }

            public static SinglePrimaryKey MakeSinglePrimaryKeyInstance()
            {
                return new SinglePrimaryKey
                {
                    Key1 = KEY_1_VAL,
                    Prop1 = PROP_1_VAL,
                    Prop2 = PROP_2_VAL
                };
            }

            public static SinglePrimaryKey MakeSinglePrimaryKeyWithNullInstance()
            {
                return new SinglePrimaryKey
                {
                    Key1 = KEY_1_VAL,
                    Prop1 = PROP_1_VAL,
                    Prop2 = null
                };
            }

            public static MultiplePrimaryKey MakeMultiplePrimaryKeyInstance()
            {
                return new MultiplePrimaryKey
                {
                    Key1 = KEY_1_VAL,
                    Key2 = KEY_2_VAL,
                    Prop1 = PROP_1_VAL,
                    Prop2 = PROP_2_VAL
                };
            }

            public static IdentityComputedColumns MakeIdentityComputedColumnsInstance()
            {
                return new IdentityComputedColumns
                {
                    Key1 = KEY_1_VAL,
                    Ident1 = IDENT_1_VAL,
                    Comp1 = COMP_1_VAL,
                    Prop1 = PROP_1_VAL
                };
            }

            public static IdentityComputedOnlyWithKey MakeIdentityComputedOnlyWithKeyInstance()
            {
                return new IdentityComputedOnlyWithKey
                {
                    Key1 = KEY_1_VAL,
                    Ident1 = IDENT_1_VAL,
                    Comp1 = COMP_1_VAL
                };
            }

            public static IdentityComputedOnlyWithoutKey MakeIdentityComputedOnlyWithoutKeyInstance()
            {
                return new IdentityComputedOnlyWithoutKey
                {
                    Ident1 = IDENT_1_VAL,
                    Comp1 = COMP_1_VAL
                };
            }

            public static WithTableSchema MakeWithTableSchemaInstance()
            {
                return new WithTableSchema
                {
                    Key1 = KEY_1_VAL,
                    Prop1 = PROP_1_VAL
                };
            }

            public static WithColumnNames MakeWithColumnNamesInstance()
            {
                return new WithColumnNames
                {
                    Key1 = KEY_1_VAL,
                    Prop1 = PROP_1_VAL,
                    Prop2 = PROP_2_VAL
                };
            }

            public static List<IDbDataParameter> MakeExpectedSinglePrimaryKeyParams()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(KEY_1_PRM, KEY_1_VAL)
                };
            }
            
            public static List<IDbDataParameter> MakeExpectedSinglePrimaryKeyParamsIns()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(PROP_1_PRM, PROP_1_VAL),
                    new SqlParameter(PROP_2_PRM, PROP_2_VAL)
                };
            }

            public static List<IDbDataParameter> MakeExpectedSinglePrimaryKeyWithNullParamsIns()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(PROP_1_PRM, PROP_1_VAL),
                    new SqlParameter(PROP_2_PRM, DBNull.Value)
                };
            }

            public static List<IDbDataParameter> MakeExpectedSinglePrimaryKeyWithNullParamsUpd()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(PROP_1_PRM, PROP_1_VAL),
                    new SqlParameter(PROP_2_PRM, DBNull.Value),
                    new SqlParameter(KEY_1_PRM, KEY_1_VAL)
                };
            }

            public static List<IDbDataParameter> MakeExpectedSinglePrimaryKeyParamsUpd()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(PROP_1_PRM, PROP_1_VAL),
                    new SqlParameter(PROP_2_PRM, PROP_2_VAL),
                    new SqlParameter(KEY_1_PRM, KEY_1_VAL)
                };
            }

            public static List<IDbDataParameter> MakeExpectedMultiplePrimaryKeyParams()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(KEY_1_PRM, KEY_1_VAL),
                    new SqlParameter(KEY_2_PRM, KEY_2_VAL)
                };
            }

            public static List<IDbDataParameter> MakeExpectedMultiplePrimaryKeyParamsUpd()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(PROP_1_PRM, PROP_1_VAL),
                    new SqlParameter(PROP_2_PRM, PROP_2_VAL),
                    new SqlParameter(KEY_1_PRM, KEY_1_VAL),
                    new SqlParameter(KEY_2_PRM, KEY_2_VAL)
                };
            }

            public static List<IDbDataParameter> MakeExpectedMultiplePrimaryKeyParamsIns()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(KEY_1_PRM, KEY_1_VAL),
                    new SqlParameter(KEY_2_PRM, KEY_2_VAL),
                    new SqlParameter(PROP_1_PRM, PROP_1_VAL),
                    new SqlParameter(PROP_2_PRM, PROP_2_VAL)
                };
            }

            public static List<IDbDataParameter> MakeExpectedIdentityComputedColumnsParamsUpd()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(PROP_1_PRM, PROP_1_VAL),
                    new SqlParameter(KEY_1_PRM, KEY_1_VAL)
                };
            }

            public static List<IDbDataParameter> MakeExpectedIdentityComputedColumnsParamsIns()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter(KEY_1_PRM, KEY_1_VAL),
                    new SqlParameter(PROP_1_PRM, PROP_1_VAL)
                };
            }

            public static List<IDbDataParameter> MakeExpectedWithColumnNamesParams()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter { ParameterName = KEY_1_PRM_OTH, Value = KEY_1_VAL, SqlDbType = SqlDbType.Int }
                };
            }

            public static List<IDbDataParameter> MakeExpectedWithColumnNamesParamsUpd()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter { ParameterName = PROP_1_PRM_OTH, Value = PROP_1_VAL, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter { ParameterName = PROP_2_PRM_OTH, Value = PROP_2_VAL, SqlDbType = SqlDbType.NVarChar },
                    new SqlParameter { ParameterName = KEY_1_PRM_OTH, Value = KEY_1_VAL, SqlDbType = SqlDbType.Int }
                };
            }

            public static List<IDbDataParameter> MakeExpectedWithColumnNamesParamsIns()
            {
                return new List<IDbDataParameter>
                {
                    new SqlParameter { ParameterName = PROP_1_PRM_OTH, Value = PROP_1_VAL, SqlDbType = SqlDbType.VarChar },
                    new SqlParameter { ParameterName = PROP_2_PRM_OTH, Value = PROP_2_VAL, SqlDbType = SqlDbType.NVarChar }
                };
            }
        }

        #endregion
    }
}
