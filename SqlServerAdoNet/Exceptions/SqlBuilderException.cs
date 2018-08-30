using System;

namespace StatKings.SqlServerAdoNet
{
    [Serializable]
    public class SqlBuilderException : Exception
    {
        public SqlBuilderException() : base() { }

        public SqlBuilderException(string message) : base(message) { }
    }
}
