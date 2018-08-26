using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using NUnit.Framework;
using FluentAssertions;

namespace StatKings.SqlServerAdoNet.UnitTests
{
    /// <summary>
    /// Unit tests for StatKings.SqlServerAdoNet.ModelHelper.
    /// </summary>
    [TestFixture]
    public class ModelHelperTests
    {
        #region GetTableName

        /// <summary>
        /// Test that the expected name is returned for the model.
        /// </summary>
        [GenericTestCase(typeof(TableNameA), ExpectedResult = "MyTable")]
        [GenericTestCase(typeof(TableNameB), ExpectedResult = "MyTable")]
        [GenericTestCase(typeof(TableNameC), ExpectedResult = "TableNameC")]
        [GenericTestCase(typeof(TableNameD), ExpectedResult = "TableNameD")]
        [GenericTestCase(typeof(TableNameE), ExpectedResult = "TableNameE")]
        [GenericTestCase(typeof(TableNameF), ExpectedResult = "TableNameF")]
        [GenericTestCase(typeof(TableNameDto), ExpectedResult = "TableName")]
        public string GetTableName_ExpectedNameReturned<T>()
        {
            return ModelHelper.GetTableName<T>().Name;
        }

        /// <summary>
        /// Test that the expected name is returned for the model.
        /// </summary>
        [GenericTestCase(typeof(TableNameA), ExpectedResult = null)]
        [GenericTestCase(typeof(TableNameB), ExpectedResult = "myschema")]
        [GenericTestCase(typeof(TableNameF), ExpectedResult = null)]
        public string GetTableName_ExpectedSchemaReturned<T>()
        {
            return ModelHelper.GetTableName<T>().Schema;
        }

        /// <summary>
        /// Has Table attribute which will be used.
        /// </summary>
        [Table("MyTable")]
        private class TableNameA { }

        /// <summary>
        /// Has Table attribute with schema.
        /// </summary>
        [Table("MyTable", Schema = "myschema")]
        private class TableNameB { }

        /// <summary>
        /// Has null Table attribute so model name will be used.
        /// </summary>
        [Table(null)]
        private class TableNameC { }

        /// <summary>
        /// Has empty string Table attribute so model name will be used.
        /// </summary>
        [Table("")]
        private class TableNameD { }

        /// <summary>
        /// Has whitespace Table attribute so model name will be used.
        /// </summary>
        [Table(" ")]
        private class TableNameE { }

        /// <summary>
        /// Model name will be used.
        /// </summary>
        private class TableNameF { }

        /// <summary>
        /// Model name will be used after stripping off "Dto".
        /// </summary>
        private class TableNameDto { }

        #endregion

        #region GetTableColumns

        /// <summary>
        /// Test that the expected column count is returned.
        /// </summary>
        [GenericTestCase(typeof(TableColumnsA), "KeyA", "MemberA", "ComputedA")]
        [GenericTestCase(typeof(TableColumnsB), "KeyA", "KeyB", "ComputedA", "ComputedB", "MemberA")]
        [GenericTestCase(typeof(TableColumnsF), "MemberA", "MemberB", "MemberC", "MemberD")]
        [GenericTestCase(typeof(TableColumnsE))]
        [GenericTestCase(typeof(TableColumnsH))]
        public void GetTableColumns_HasExpectedColumns<T>(params string[] expectedResult)
        {
            var cols = ModelHelper.GetTableColumns<T>();
            cols.Select(y => y.Id).Should().Equal(expectedResult);
        }

        /// <summary>
        /// Test that the expected list of keys is returned.
        /// </summary>
        [GenericTestCase(typeof(TableColumnsA), "KeyA")]
        [GenericTestCase(typeof(TableColumnsB), "KeyA", "KeyB")]
        [GenericTestCase(typeof(TableColumnsF))]
        [GenericTestCase(typeof(TableColumnsH))]
        public void GetTableColumns_HasExpectedKeyColumns<T>(params string[] expectedResult)
        {
            var cols = ModelHelper.GetTableColumns<T>();
            cols.Where(x => x.IsPrimaryKey).Select(y => y.Id).Should().Equal(expectedResult);
        }

        /// <summary>
        /// Test that the expected list of identity columns is returned.
        /// </summary>
        [GenericTestCase(typeof(TableColumnsA), "KeyA")]
        [GenericTestCase(typeof(TableColumnsF))]
        [GenericTestCase(typeof(TableColumnsH))]
        public void GetTableColumns_HasExpectedIdentityColumns<T>(params string[] expectedResult)
        {
            var cols = ModelHelper.GetTableColumns<T>();
            cols.Where(x => x.IsIdentity).Select(y => y.Id).Should().Equal(expectedResult);
        }

        /// <summary>
        /// Test that the expected list of computed columns is returned.
        /// </summary>
        [GenericTestCase(typeof(TableColumnsA), "ComputedA")]
        [GenericTestCase(typeof(TableColumnsB), "ComputedA", "ComputedB")]
        [GenericTestCase(typeof(TableColumnsF))]
        [GenericTestCase(typeof(TableColumnsH))]
        public void GetTableColumns_HasExpectedComputedColumns<T>(params string[] expectedResult)
        {
            var cols = ModelHelper.GetTableColumns<T>();
            cols.Where(x => x.IsComputed).Select(y => y.Id).Should().Equal(expectedResult);
        }

        /// <summary>
        /// Test that the columns have the expected names.  If the column is annotated with a 
        /// different name then that name will be returned.
        /// </summary>
        [GenericTestCase(typeof(TableColumnsA), "KeyA", "MemberA", "ComputedA")]
        [GenericTestCase(typeof(TableColumnsF), "MemberA", "PropB", "PropC", "PropD")]
        public void GetTableColumns_HasExpectedNames<T>(params string[] expectedResult)
        {
            var cols = ModelHelper.GetTableColumns<T>();
            cols.Select(x => x.Name).Should().Equal(expectedResult);
        }

        /// <summary>
        /// Test that the columns have the expected SqlDbType, if the column annotated it.
        /// </summary>
        [GenericTestCase(typeof(TableColumnsA), null, null, null)]
        [GenericTestCase(typeof(TableColumnsF), SqlDbType.Int, SqlDbType.VarChar, null, null)]
        public void GetTableColumns_HasExpectedColumnDbTypes<T>(params SqlDbType?[] expectedResult)
        {
            var cols = ModelHelper.GetTableColumns<T>();
            cols.Select(x => x.SqlDbType).Should().Equal(expectedResult);
        }

        /// <summary>
        /// Test that a ModelAnnotationException is thrown when the model breaks the annotations rules.
        /// </summary>
        [GenericTestCase(typeof(TableColumnsC), "The model has multiple identity primary keys.")]
        [GenericTestCase(typeof(TableColumnsD), "The model has a nullable primary key.")]
        [GenericTestCase(typeof(TableColumnsG), "The model uses the same column name for multiple properties.")]
        public void GetTableColumns_ModelAnnotationExceptionThrown<T>(string expectedResult)
        {
            Action act = () => ModelHelper.GetTableColumns<T>();
            act.Should().Throw<ModelDefinitionException>(expectedResult);
        }

        /// <summary>
        /// One identity key and one computed column.
        /// </summary>
        private class TableColumnsA
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int KeyA { get; set; }

            public string MemberA { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public int ComputedA { get; set; }
        }

        /// <summary>
        /// Two keys and two computed columns.
        /// </summary>
        private class TableColumnsB
        {
            [Key]
            public int KeyA { get; set; }

            [Key]
            public int KeyB { get; set; }
            
            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public int ComputedA { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public int ComputedB { get; set; }

            public string MemberA { get; set; }
        }
        
        /// <summary>
        /// Two identity keys which results in an exception.
        /// </summary>
        private class TableColumnsC
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int KeyA { get; set; }

            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int KeyB { get; set; }
        }

        /// <summary>
        /// Nullable key which results in an exception.
        /// </summary>
        private class TableColumnsD
        {
            [Key]
            public int KeyA { get; set; }

            [Key]
            public int? KeyB { get; set; }
        }

        /// <summary>
        /// No public properties.
        /// </summary>
        private class TableColumnsE
        {
            internal int MemberA { get; set; }

            protected int MemberB { get; set; }

            private int MemberC { get; set; }
        }

        /// <summary>
        /// Column attributes.
        /// </summary>
        private class TableColumnsF
        {
            [Column(TypeName = "int")]
            public int MemberA { get; set; }

            [Column("PropB", TypeName = "varchar")]
            public string MemberB { get; set; }

            [Column("PropC")]
            public string MemberC { get; set; }

            [Column(name: "PropD")]
            public int MemberD { get; set; }
        }

        /// <summary>
        /// Duplicate column name.
        /// </summary>
        private class TableColumnsG
        {
            [Column("Column1")]
            public int MemberA { get; set; }

            [Column("Column1")]
            public int MemberB { get; set; }
        }

        /// <summary>
        /// No properties.
        /// </summary>
        private class TableColumnsH
        {
        }

        #endregion
    }
}
