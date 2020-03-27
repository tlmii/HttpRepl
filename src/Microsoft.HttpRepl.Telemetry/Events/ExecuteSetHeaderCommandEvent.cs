using System.Collections.Generic;

namespace Microsoft.HttpRepl.Telemetry.Events
{
    public class ExecuteSetHeaderCommandEvent : ExecuteCommandEvent
    {
        public string HeaderName { get; set; }
        public string HeaderValue { get;set; }
        public ExecuteSetHeaderCommandEvent(string headerName, string headerValue, bool wasSuccessful = true) : base("SetHeader", wasSuccessful)
        {
            HeaderName = headerName;
            HeaderValue = headerValue;
        }

        protected override IDictionary<string, string> BuildProperties()
        {
            IDictionary<string, string> properties = base.BuildProperties();
            properties[TelemetryEventProperties.HeaderName] = HeaderName;
            properties[TelemetryEventProperties.HeaderValue] = HeaderValue;

            return properties;
        }
    }
}
