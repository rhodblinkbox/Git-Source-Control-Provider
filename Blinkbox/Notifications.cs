// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Notifications.cs" company="blinkbox">
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
    public class Notifications
    {
        /// <summary>
        /// Lock for singleton initialisation.
        /// </summary>
        private static readonly object Sync = new object();

        /// <summary>
        /// The singleton instance of the NotificationWriter.
        /// </summary>
        private static Notifications instance = null;

        /// <summary>
        /// A queue of the messages. 
        /// </summary>
        private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

        /// <summary>
        /// A thread for writing messages to the output window. 
        /// </summary>
        private readonly Thread processingThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="Notifications"/> class. 
        /// </summary>
        public Notifications()
        {
            this.processingThread = new Thread(this.ProcessMessages);
            this.processingThread.Start();
        }

        /// <summary>
        /// Gets a singleton Instance.
        /// </summary>
        private static Notifications Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (Sync)
                    {
                        instance = instance ?? new Notifications();
                    }
                }

                return instance;
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
            
            MessageBox.Show(message, title);

            Trace.WriteLine(title);
            Trace.WriteLine(message);
        }

        /// <summary>
        /// Writes a message into our output pane.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public static void AddMessage(string message)
        {
            Instance.messages.Enqueue(message);
        }

        /// <summary>
        /// Writes a new section to the output window.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public static void NewSection(string name)
        {
            AddMessage(Environment.NewLine + "#### " + name + " ############################################");
        }

        /// <summary>
        /// Clears the output pane. 
        /// </summary>
        public static void ClearMessages()
        {
            PendingChangesView.ClearDiffEditor();
        }

        /// <summary>
        /// Periodically checks for messages and writes them to the output window. 
        /// </summary>
        private void ProcessMessages()
        {
            while (true)
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
                    PendingChangesView.WriteToDiffWindow(message);
                }
            }
        }
    }
}