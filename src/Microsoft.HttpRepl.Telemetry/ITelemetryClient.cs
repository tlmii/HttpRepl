using System.Collections.Generic;

namespace Microsoft.HttpRepl.Telemetry
{
    public interface ITelemetryClient
    {
        void LogEvent(ITelemetryEvent telemetryEvent);

        void Dispose();
    }

    public interface ITelemetryEvent
    {
        string Name { get; }
        IDictionary<string, string> Properties { get; }

        IDictionary<string, double> Metrics { get; }
    }
}
