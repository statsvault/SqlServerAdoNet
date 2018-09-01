using System;
using System.Runtime.Serialization;

namespace StatKings.SqlServerAdoNet
{
    [Serializable]
    public class CommandSettingsException : Exception
    {
        public CommandSettingsException() : base() { }

        public CommandSettingsException(string message) : base(message) { }

        private CommandSettingsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
