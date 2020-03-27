using System.Collections.Generic;

namespace Microsoft.HttpRepl.Telemetry.Events
{
    public abstract class ExecutePrefCommandEvent : ExecuteCommandEvent
    {
        public string PreferenceName { get; }
        protected ExecutePrefCommandEvent(string preferenceName, string commandName, bool wasSuccessful) : base(commandName, wasSuccessful)
        {
            PreferenceName = preferenceName;
        }

        protected override IDictionary<string, string> BuildProperties()
        {
            IDictionary<string, string> properties = base.BuildProperties();

            properties[TelemetryEventProperties.PreferenceName] = PreferenceName;

            return properties;
        }
    }

    public class ExecutePrefSetCommandEvent : ExecutePrefCommandEvent
    {
        public ExecutePrefSetCommandEvent(string preferenceName, bool wasSuccessful = true)
            : base(preferenceName, "PrefSet", wasSuccessful)
        {

        }
    }

    public class ExecutePrefGetCommandEvent : ExecutePrefCommandEvent
    {
        public ExecutePrefGetCommandEvent(string preferenceName, bool wasSuccessful = true)
            : base(preferenceName, "PrefGet", wasSuccessful)
        {

        }
    }
}
