using System;

namespace StatKings.SqlServerAdoNet
{
    public class CommandSettingsException : Exception
    {
        public CommandSettingsException() : base() { }

        public CommandSettingsException(string message) : base(message) { }
    }
}
