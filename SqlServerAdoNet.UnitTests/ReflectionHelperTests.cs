using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using FluentAssertions;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatKings.SqlServerAdoNet.UnitTests
{
    /// <summary>
    /// Unit tests for StatKings.SqlServerAdoNet.ReflectionHelper.
    /// </summary>
    [TestFixture]
    public class ReflectionHelperTests
    {
        #region GetAttributeValue

        /// <summary>
        /// Test that the expected property value of a class attribute is returned.
        /// </summary>
        [Test]
        public void GetAttributeValue_ClassTableAttribute_ExpectedValueReturned()
        {
            var type = typeof(ClassA);
            var val = type.GetAttributeValue((TableAttribute a) => a.Name);
            val.Should().Be("MyTable");
        }

        /// <summary>
        /// Test that the expected property value of a class attribute is returned.
        /// </summary>
        [Test]
        public void GetAttributeValue_ClassScaffoldTableAttribute_ExpectedValueReturned()
        {
            var type = typeof(ClassA);
            var val = type.GetAttributeValue((ScaffoldTableAttribute a) => a.Scaffold);
            val.Should().BeFalse();
        }

        /// <summary>
        /// Test that the expected property value of a property attribute is returned.
        /// </summary>
        [Test]
        public void GetAttributeValue_PropertyDisplayAttribute_ExpectedValueReturned()
        {
            var prop = typeof(ClassB).GetProperty("MemberA");
            var val = prop.GetAttributeValue((DisplayAttribute a) => a.Name);
            val.Should().Be("Member A");
        }

        /// <summary>
        /// Test that the expected property value of a property attribute is returned.
        /// </summary>
        [Test]
        public void GetAttributeValue_PropertyMaxLengthAttribute_ExpectedValueReturned()
        {
            var prop = typeof(ClassB).GetProperty("MemberG");
            var val = prop.GetAttributeValue((MaxLengthAttribute a) => a.Length);
            val.Should().Be(100);
        }

        /// <summary>
        /// Test that a null value is returned for an attribute that does not exist.
        /// </summary>
        [Test]
        public void GetAttributeValue_AttributeDoesNotExist_NullValueReturned()
        {
            var type = typeof(ClassA);
            var val = type.GetAttributeValue((MetadataTypeAttribute a) => a.MetadataClassType);
            val.Should().BeNull();
        }

        /// <summary>
        /// Test that a null value is returned for an attribute property that does not exist.
        /// </summary>
        [Test]
        public void GetAttributeValue_AttributePropertyDoesNotExist_NullValueReturned()
        {
            var prop = typeof(ClassB).GetProperty("MemberB");
            var val = prop.GetAttributeValue((RequiredAttribute a) => a.ErrorMessage);
            val.Should().BeNull();
        }

        #endregion

        #region GetProperty

        /// <summary>
        /// Test that a valid property name returns the expected property.
        /// </summary>
        /// <param name="propName">Name of property to test.</param>
        [TestCase("MemberA")]
        [TestCase("MemberG")]
        public void GetProperty_ValidPropertyName_ExpectedPropertyReturned(string propName)
        {
            var prop = ReflectionHelper.GetProperty<ClassB>(propName);
            prop.Name.Should().Be(propName);
        }

        /// <summary>
        /// Test that an invalid property name returns null.
        /// </summary>
        /// <param name="propName"></param>
        [TestCase("MemberZ")]
        [TestCase("")]
        public void GetProperty_InvalidPropertyName_NullReturned(string propName)
        {
            var prop = ReflectionHelper.GetProperty<ClassB>(propName);
            prop.Should().BeNull();
        }

        /// <summary>
        /// Test that a null property name throws an ArgumentNullException.
        /// </summary>
        [Test]
        public void GetProperty_NullPropertyName_ArgumentNullExceptionThrown()
        {
            Action act = () => ReflectionHelper.GetProperty<ClassB>(null);
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region GetProperties

        /// <summary>
        /// Test that the expected number of properties are returned.
        /// </summary>
        [GenericTestCase(typeof(ClassA), 0)]
        [GenericTestCase(typeof(ClassB), 4)]
        public void GetProperties_HasExpectedPropertyCount<T>(int expectedResult)
        {
            ReflectionHelper.GetProperties<T>().Should().HaveCount(expectedResult);
        }

        #endregion

        #region GetPropertyValues

        /// <summary>
        /// Test that the expected properties and values are returned for an instance of a class.
        /// </summary>
        [Test]
        public void GetPropertyValues_ClassInstanceWithProperties_HasExpectedPropertiesAndValues()
        {
            var model = new ClassB { MemberA = 1, MemberB = "Hello", MemberG = "World" };
            var props = ReflectionHelper.GetPropertyValues(model);

            var expected = new Dictionary<string, object>()
            {
                { "MemberA", 1 },
                { "MemberB", "Hello" },
                { "MemberF", null },
                { "MemberG", "World" }
            };
            props.Should().Equal(expected);
        }

        /// <summary>
        /// Test that the expected properties and values are returned for an instance of a class.
        /// </summary>
        [Test]
        public void GetPropertyValues_ClassInstanceNoProperties_HasNoPropertiesAndValues()
        {
            var props = ReflectionHelper.GetPropertyValues(new ClassA());
            props.Should().Equal(new Dictionary<string, object>());
        }

        #endregion

        #region GetPropertyNames

        /// <summary>
        /// Test that the expected property names are returned for a class.
        /// </summary>
        [Test]
        public void GetPropertyNames_ClassInstanceWithProperties_HasExpectedNames()
        {
            var expected = new List<string> { "MemberA", "MemberB", "MemberF", "MemberG" };
            var actual = ReflectionHelper.GetPropertyNames<ClassB>();
            actual.Should().Equal(expected);
        }

        /// <summary>
        /// Test that no property names are returned for a class that has no public properties..
        /// </summary>
        [Test]
        public void GetPropertyNames_ClassInstanceNoProperties_HasNoPropertiesAndValues()
        {
            var names = ReflectionHelper.GetPropertyNames<ClassA>();
            names.Should().BeEmpty();
        }

        #endregion

        #region GetPropertyColumnNames

        /// <summary>
        /// Test that a type with public properties, some with Column attributes, returns the expected list of name pairs.
        /// </summary>
        [Test]
        public void GetPropertyColumnNames_TypeWithPublicProperties_ExpectedNamesReturned()
        {
            var expectedNames = new Dictionary<string, string>
            {
                { "MemberA", "Column1" },
                { "MemberB", "Column2" },
                { "MemberC", "MemberC" }
            };
            var actualNames = ReflectionHelper.GetPropertyColumnNames<ClassC>();
            actualNames.Should().BeEquivalentTo(expectedNames);
        }

        /// <summary>
        /// Test that a type with no public properties returns an empty list of name pairs.
        /// </summary>
        [Test]
        public void GetPropertyColumnNames_TypeWithNoPublicProperties_EmptyListReturned()
        {
            var actualNames = ReflectionHelper.GetPropertyColumnNames<ClassA>();
            actualNames.Should().BeEmpty();
        }

        #endregion

        /// <summary>
        /// Class with Table class attribute.
        /// </summary>
        [Table("MyTable")]
        private class ClassA
        {            
        }

        /// <summary>
        /// Class with multiple attributes, and properties with a mix of access modifiers.
        /// </summary>
        [ScaffoldTable(false)]
        private class ClassB
        {
            [Display(Name = "Member A")]
            public int MemberA { get; set; }

            [Required]
            public string MemberB { get; set; }

            internal int MemberC { get; set; }

            protected int MemberD { get; set; }

            private int MemberE { get; set; }

            public int? MemberF { get; set; }

            [MaxLength(100)]
            public string MemberG { get; set; }
        }

        /// <summary>
        /// Class with mix of properties with and without Column attribute.
        /// </summary>
        private class ClassC
        {
            [Column("Column1")]
            public int MemberA { get; set; }

            [Column("Column2")]
            public string MemberB { get; set; }

            public bool MemberC { get; set; }

            private int MemberD { get; set; }
        }
    }
}
