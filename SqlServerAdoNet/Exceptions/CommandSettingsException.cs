using System;
using System.Runtime.Serialization;

namespace StatKings.SqlServerAdoNet
{
    [Serializable]
    public class CommandSettingsException : Exception
    {
        public CommandSettingsException() : base() { }

        public CommandSettingsException(string message) : base(message) { }

        public CommandSettingsException(string message, Exception innerException)
            : base(message, innerException) { }

        // Without this constructor, deserialization will fail
        protected CommandSettingsException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
