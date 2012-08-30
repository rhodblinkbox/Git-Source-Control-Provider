// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationService.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Writes messages to the Pending changes window.
    /// </summary>
    public class NotificationService : IDisposable
    {
        /// <summary>
        /// Instance of the  <see cref="SccProviderService"/>
        /// </summary>
        private readonly SccProviderService sccProvider;

        /// <summary>
        /// A queue of the messages. 
        /// </summary>
        private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

        /// <summary>
        /// A thread for writing messages to the output window. 
        /// </summary>
        private readonly Thread processingThread;

        /// <summary>
        /// Flag indicating that notifications are being processed
        /// </summary>
        private bool processingMessages = false;

        /// <summary>
        /// Instance of the pending changes view
        /// </summary>
        private PendingChangesView pendingChangesViewInstance = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="sccProvider">The SCC provider.</param>
        public NotificationService(SccProviderService sccProvider)
        {
            this.sccProvider = sccProvider;
            this.processingThread = new Thread(this.ProcessMessages);
            this.processingThread.Start();

            // resume message processing whenever the source control is activated. 
            sccProvider.SourceControlActivatedOrDeactivated += (sender, args) => this.ProcessMessages();
            sccProvider.SolutionOpenedOrClosed += (sender, args) => this.ProcessMessages();
        }

        /// <summary>
        /// Gets the pending changes view.
        /// </summary>
        /// <value>The pending changes view.</value>
        private PendingChangesView PendingChangesView
        {
            get
            {
                this.pendingChangesViewInstance = this.pendingChangesViewInstance ?? BasicSccProvider.GetServiceEx<PendingChangesView>();
                return this.pendingChangesViewInstance;
            }
        }

        /// <summary>
        /// Displays the exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public static void DisplayException(Exception e, string title = null, string message = null)
        {
            message = (message ?? string.Empty) + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace;
            title = title ?? "An error occurred";
            DisplayError(message, title);
        }

        /// <summary>
        /// Displays an error in a message box.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public static void DisplayError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            Trace.WriteLine(title);
            Trace.WriteLine(message);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Noop
        }

        /// <summary>
        /// Writes a message into our output pane.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public void AddMessage(string message)
        {
            this.messages.Enqueue(message);
        }

        /// <summary>
        /// Writes a new section to the output window.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public void NewSection(string name)
        {
            this.AddMessage(Environment.NewLine + "#### " + name + " ############################################");
        }

        /// <summary>
        /// Clears the output pane. 
        /// </summary>
        public void ClearMessages()
        {
            PendingChangesView.ClearDiffEditor();
        }

        /// <summary>
        /// Periodically checks for messages and writes them to the output window. 
        /// </summary>
        private void ProcessMessages()
        {
            if (this.processingMessages)
            {
                // Already processing
                return;
            }

            while (this.sccProvider.Active && this.sccProvider.SolutionOpen)
            {
                // only process messages if the solution is open. 
                this.processingMessages = true;
                this.WriteMessageQueue();
                System.Threading.Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Writes all mesages in the queue to the output window. 
        /// </summary>
        private void WriteMessageQueue()
        {
            while (!this.messages.IsEmpty)
            {
                string message;
                this.messages.TryDequeue(out message);
                if (!string.IsNullOrEmpty(message))
                {
                    this.PendingChangesView.WriteToDiffWindow(message);
                }
            }
        }
    }
}