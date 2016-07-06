namespace GitRebase.VisualStudio.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Microsoft.ApplicationInsights;

    using TeamExplorer.Common;

    /// <summary>
    /// Logs to our telemetry application insights.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Initializes static members of the <see cref="Logger"/> class.
        /// </summary>
        static Logger()
        {
            TelemetryClient = new TelemetryClient
            {
                InstrumentationKey = "0b2258d0-45b4-405b-9c01-53bbfff9721d"
            };
            TelemetryClient.Context.Properties["VisualStudioVersion"] = VSVersion.FullVersion.ToString();
            TelemetryClient.Context.Component.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            TelemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            TelemetryClient.Context.User.Id = UserSettings.UserId;
        }

        /// <summary>
        /// Gets or sets the client which communicates with the telemetry client.
        /// </summary>
        private static TelemetryClient TelemetryClient { get; set; }

        /// <summary>
        /// Indicate we are going to a particular page.
        /// </summary>
        /// <param name="page">The page we are visiting.</param>
        public static void PageView(string page)
        {
            TelemetryClient.TrackPageView(page);
        }

        /// <summary>
        /// Indicate that we have had an event occur.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="properties">The properties of the event.</param>
        public static void Event(string eventName, IDictionary<string, string> properties = null)
        {
            if (properties == null)
            {
                TelemetryClient.TrackEvent(eventName);
            }
            else
            {
                TelemetryClient.TrackEvent(eventName, properties);
            }
        }

        /// <summary>
        /// Indicate a particular metric has occurred.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="value">The value of the metric.</param>
        public static void Metric(string name, double value)
        {
            TelemetryClient.TrackMetric(name, value);
        }

        /// <summary>
        /// Indicate a exception has occurred. 
        /// </summary>
        /// <param name="ex">The exception that has occurred.</param>
        public static void Exception(Exception ex)
        {
            TelemetryClient.TrackException(ex);
        }
    }
}
