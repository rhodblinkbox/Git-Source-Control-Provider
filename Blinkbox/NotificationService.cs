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
        /// Flag indicating that notifications should be processed
        /// </summary>
        private static bool notificationsActive = false;

        /// <summary>
        /// A queue of the messages. 
        /// </summary>
        private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

        /// <summary>
        /// A thread for writing messages to the output window. 
        /// </summary>
        private readonly Thread processingThread;

        /// <summary>
        /// Instance of the pending changes view
        /// </summary>
        private PendingChangesView pendingChangesViewInstance = null;

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
        /// Initializes a new instance of the <see cref="NotificationService"/> class. 
        /// </summary>
        public NotificationService()
        {
            this.processingThread = new Thread(this.ProcessMessages);
            this.processingThread.Start();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Stop notification processing. 
            notificationsActive = false;
        }

        /// <summary>
        /// Displays the exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        public void DisplayException(Exception e, string title = null, string message = null)
        {
            message = (message ?? string.Empty) + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace;
            title = title ?? "An error occurred";
            
            MessageBox.Show(message, title);

            Trace.WriteLine(title);
            Trace.WriteLine(message);
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
            AddMessage(Environment.NewLine + "#### " + name + " ############################################");
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
            notificationsActive = true;
            // TODO: set notificationsActive to false when the solution closes. 
            while (notificationsActive)
            {
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