using FluentAssertions;
using NUnit.Framework;
using System;

namespace StatKings.SqlServerAdoNet.UnitTests
{
    /// <summary>
    /// Unit tests for StatKings.SqlServerAdoNet.CommonHelper.
    /// </summary>
    [TestFixture]
    public class CommonHelperTests
    {
        private const string THE_FOX = "The quick brown fox jumps over the lazy dog";

        /// <summary>
        /// Test that an insensitive string replacement returns the expected string.
        /// </summary>
        /// <param name="stringToSearch">String to search.</param>
        /// <param name="oldValue">Value to be replaced.</param>
        /// <param name="newValue">Value used for replacement.</param>
        [TestCase(null, "the", "a", ExpectedResult = null)]
        [TestCase("", "the", "a", ExpectedResult = "")]
        [TestCase("  ", " ", "a", ExpectedResult = "aa")]
        [TestCase(THE_FOX, null, "a", ExpectedResult = THE_FOX)]
        [TestCase(THE_FOX, "", "a", ExpectedResult = THE_FOX)]
        [TestCase(THE_FOX, "a", null, ExpectedResult = THE_FOX)]
        [TestCase(THE_FOX, "the", "", ExpectedResult = " quick brown fox jumps over  lazy dog")]
        [TestCase(THE_FOX, "BROWN Fox", "", ExpectedResult = "The quick  jumps over the lazy dog")]
        [TestCase(THE_FOX, "x j", "g h", ExpectedResult = "The quick brown fog humps over the lazy dog")]
        [TestCase(THE_FOX, "brOwn", "yellow", ExpectedResult = "The quick yellow fox jumps over the lazy dog")]
        public string ReplaceInsensitive_ExpectedStringReturned(string stringToSearch, string oldValue, string newValue)
        {
            return CommonHelper.ReplaceInsensitive(stringToSearch, oldValue, newValue);
        }

        /// <summary>
        /// Test that a string is removed from the end of another string.
        /// </summary>
        /// <param name="source">String whose end should be removed.</param>
        /// <param name="value">Value to remove.</param>
        /// <returns></returns>
        [TestCase(null, "Dto", ExpectedResult = null)]
        [TestCase("", "Dto", ExpectedResult = "")]
        [TestCase("SomeEntityDto", null, ExpectedResult = "SomeEntityDto")]
        [TestCase("SomeEntityDto", "", ExpectedResult = "SomeEntityDto")]
        [TestCase("SomeEntityDto", "What", ExpectedResult = "SomeEntityDto")]
        [TestCase("SomeEntityDTO", "Dto", ExpectedResult = "SomeEntity")]
        [TestCase("SomeEntitydto", "Dto", ExpectedResult = "SomeEntity")]
        [TestCase("SomeEntityDtO", "Dto", ExpectedResult = "SomeEntity")]        
        public string RemoveFromEnd_ExpectedStringReturned(string source, string value)
        {
            return CommonHelper.RemoveFromEnd(source, value);
        }

        /// <summary>
        /// Test that the expected nullable decimal value is returned for the given input.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [TestCase(null, ExpectedResult = null)]
        [TestCase("", ExpectedResult = null)]
        [TestCase("0", ExpectedResult = 0)]
        [TestCase(0, ExpectedResult = 0)]
        [TestCase("25.50", ExpectedResult = 25.50)]
        [TestCase(25.50, ExpectedResult = 25.50)]
        public decimal? ToDecimal_ExpectedValueReturned(object value)
        {
            return value.ToDecimal();
        }

        /// <summary>
        /// Test that a DBNull value returns a null decimal.
        /// </summary>
        [Test]
        public void ToDecimal_DBNullValue_NullValueReturned()
        {
            var actualValue = DBNull.Value.ToDecimal();
            actualValue.Should().BeNull();
        }
    }
}