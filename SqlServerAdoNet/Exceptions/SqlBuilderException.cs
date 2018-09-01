using System;
using System.Runtime.Serialization;

namespace StatKings.SqlServerAdoNet
{
    [Serializable]
    public class SqlBuilderException : Exception
    {
        public SqlBuilderException() : base() { }

        public SqlBuilderException(string message) : base(message) { }

        private SqlBuilderException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
