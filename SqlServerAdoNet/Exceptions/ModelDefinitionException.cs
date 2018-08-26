using System;

namespace StatKings.SqlServerAdoNet
{
    public class ModelDefinitionException : Exception
    {
        public ModelDefinitionException() : base() { }

        public ModelDefinitionException(string message) : base(message) { }
    }
}
