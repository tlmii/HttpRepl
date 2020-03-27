using System.Collections.Generic;

namespace Microsoft.HttpRepl.Telemetry.Events
{
    public class ExecuteCommandEvent : ITelemetryEvent
    {
        public string CommandName { get; protected set; }
        public bool WasSuccessful { get; protected set; }
        public ExecuteCommandEvent(string commandName, bool wasSuccessful = true)
        {
            CommandName = commandName;
            WasSuccessful = wasSuccessful;
        }

        public string Name => "ExecuteCommand";

        public IDictionary<string, string> Properties => BuildProperties();

        public IDictionary<string, double> Metrics => BuildMetrics();

        protected virtual IDictionary<string, string> BuildProperties()
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[TelemetryEventProperties.CommandName] = CommandName;
            properties[TelemetryEventProperties.WasSuccessful] = WasSuccessful.ToString();

            return properties;
        }

        protected virtual IDictionary<string, double> BuildMetrics()
        {
            Dictionary<string, double> metrics = new Dictionary<string, double>();

            return metrics;
        }
    }
}