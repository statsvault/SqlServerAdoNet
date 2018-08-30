using System;

namespace StatKings.SqlServerAdoNet
{
    [Serializable]
    public class ModelDefinitionException : Exception
    {
        public ModelDefinitionException() : base() { }

        public ModelDefinitionException(string message) : base(message) { }
    }
}
