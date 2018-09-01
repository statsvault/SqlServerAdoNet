using System;
using System.Runtime.Serialization;

namespace StatKings.SqlServerAdoNet
{
    [Serializable]
    public class ModelDefinitionException : Exception
    {
        public ModelDefinitionException() : base() { }

        public ModelDefinitionException(string message) : base(message) { }

        public ModelDefinitionException(string message, Exception innerException)
            : base(message, innerException) { }

        // Without this constructor, deserialization will fail
        protected ModelDefinitionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
