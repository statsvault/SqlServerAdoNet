using System;
using System.Runtime.Serialization;

namespace StatKings.SqlServerAdoNet
{
    [Serializable]
    public class ModelDefinitionException : Exception
    {
        public ModelDefinitionException() : base() { }

        public ModelDefinitionException(string message) : base(message) { }

        private ModelDefinitionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
