using System;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.HttpRepl.Telemetry
{
    public class TelemetryClient : ITelemetryClient, IDisposable
    {
        private TelemetryConfiguration _configuration;
        private Microsoft.ApplicationInsights.TelemetryClient _client;

        public TelemetryClient()
        {
            _configuration = new TelemetryConfiguration("5b0e627e-f603-49ab-8c7a-284791dafd2a");
            _client = new ApplicationInsights.TelemetryClient(_configuration);
        }
       
        public void LogEvent(ITelemetryEvent telemetryEvent)
        {
            telemetryEvent = telemetryEvent ?? throw new ArgumentNullException(nameof(telemetryEvent));

            _client.TrackEvent(telemetryEvent.Name, telemetryEvent.Properties, telemetryEvent.Metrics);
        }

        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _client.Flush();
                    _configuration.Dispose();
                    _client = null;
                    _configuration = null;
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
