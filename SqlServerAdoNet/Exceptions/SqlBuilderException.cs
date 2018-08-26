using System;

namespace StatKings.SqlServerAdoNet
{
    public class SqlBuilderException : Exception
    {
        public SqlBuilderException() : base() { }

        public SqlBuilderException(string message) : base(message) { }
    }
}
