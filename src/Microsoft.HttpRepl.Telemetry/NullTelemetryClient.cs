using System;

namespace Microsoft.HttpRepl.Telemetry
{
    public class NullTelemetryClient : ITelemetryClient
    {
        public void LogEvent(ITelemetryEvent telemetryEvent) { }

        public void Dispose() { }
    }
}
