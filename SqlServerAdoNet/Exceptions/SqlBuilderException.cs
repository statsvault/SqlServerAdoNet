using System;
using System.Runtime.Serialization;

namespace StatKings.SqlServerAdoNet
{
    [Serializable]
    public class SqlBuilderException : Exception
    {
        public SqlBuilderException() : base() { }

        public SqlBuilderException(string message) : base(message) { }

        public SqlBuilderException(string message, Exception innerException)
            : base(message, innerException) { }

        // Without this constructor, deserialization will fail
        protected SqlBuilderException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
