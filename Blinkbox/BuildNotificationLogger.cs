// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildNotificationLogger.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System.Diagnostics;

    using Microsoft.Build.Framework;

    /// <summary>
    /// Sends build messages to the VS output window 
    /// </summary>
    public class BuildNotificationLogger : ILogger
    {
        /// <summary>
        /// Gets or sets Verbosity.
        /// </summary>
        public LoggerVerbosity Verbosity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Parameters.
        /// </summary>
        public string Parameters
        {
            get;
            set;
        }


        /// <summary>
        /// Initialises the logger.
        /// </summary>
        /// <param name="buildEventSource">
        /// The event source.
        /// </param>
        public void Initialize(IEventSource buildEventSource)
        {
            if (this.Verbosity == LoggerVerbosity.Diagnostic)
            {
                buildEventSource.MessageRaised += (sender, args) => NotificationWriter.Write(args.Message);
                buildEventSource.TargetFinished += (sender, args) => Trace.WriteLine(args.Message);
            }

            buildEventSource.WarningRaised += (sender, args) => NotificationWriter.Write(args.Message);
            buildEventSource.ErrorRaised += (sender, args) => NotificationWriter.Write(args.Message);
            buildEventSource.BuildStarted += (sender, args) => NotificationWriter.Write(args.Message);
            buildEventSource.BuildFinished += (sender, args) => NotificationWriter.Write(args.Message);
        }

        /// <summary>
        /// Cleans up.
        /// </summary>
        public void Shutdown()
        {
        }
    }
}