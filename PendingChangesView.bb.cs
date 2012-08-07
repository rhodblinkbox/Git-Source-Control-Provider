// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PendingChangesView.bb.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   Additional implementation required by the BB version of Git Source Control.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Threading;

    /// <summary>
    /// BB extensions to the PendingChangesView
    /// </summary>
    public partial class PendingChangesView
    {
        /// <summary>
        /// Static reference to the current instance of the PendingChangesView.
        /// </summary>
        private static PendingChangesView currentInstance;

        /// <summary>
        /// Flag indicates that a review is currently in progress.
        /// </summary>
        public bool Reviewing { get; private set; }

        public static void Review (List<GitFile> changedFiles)
        {
            currentInstance.DisplayReview(changedFiles);
        }

        public static void CancelReview()
        {
            currentInstance.Reviewing = false;
        }
        
        internal void DisplayReview(List<GitFile> changedFiles)
        {
            // Set the reviewing flag to prevent refreshes over-writing the review list with the pending changes list.
            this.Reviewing = true;

            if (!GitBash.Exists)
            {
                Settings.Show();
                return;
            }

            Settings.Hide();

            if (tracker == null)
            {
                service.NoRefresh = true;
                ClearUI();
                service.NoRefresh = false;
                return;
            }

            Action act = () =>
            {

                service.NoRefresh = true;
                ShowStatusMessage("Getting changed files ...");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var selectedFile = GetSelectedFileName();
                var selectedFiles = this.dataGrid1.Items.Cast<GitFile>()
                    .Where(i => i.IsSelected)
                    .Select(i => i.FileName).ToList();

                this.dataGrid1.BeginInit();

                try
                {
                    this.dataGrid1.ItemsSource = changedFiles;

                    ICollectionView view = CollectionViewSource.GetDefaultView(this.dataGrid1.ItemsSource);
                    if (view != null)
                    {
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription(sortMemberPath, sortDirection));
                        view.Refresh();
                    }

                    this.dataGrid1.SelectedValue = selectedFile;
                    selectedFiles.ForEach(fn =>
                    {
                        var item = this.dataGrid1.Items.Cast<GitFile>()
                            .Where(i => i.FileName == fn)
                            .FirstOrDefault();
                        if (item != null) item.IsSelected = true;
                    });

                    ShowStatusMessage("");
                }
                catch (Exception ex)
                {
                    ShowStatusMessage(ex.Message);
                }
                this.dataGrid1.EndInit();

                stopwatch.Stop();
                Debug.WriteLine("**** PendingChangesView Refresh: " + stopwatch.ElapsedMilliseconds);

                if (!GitSccOptions.Current.DisableAutoRefresh && stopwatch.ElapsedMilliseconds > 1000)
                {
                    this.label4.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.label4.Visibility = System.Windows.Visibility.Collapsed;
                }

                service.NoRefresh = false;
                service.lastTimeRefresh = DateTime.Now;

            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
        }
        


        /// <summary>
        /// Writes a message to the diff editor
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public static void WriteToDiffWindow(string message)
        {
            if (currentInstance != null)
            {
                var action = new Action(() => currentInstance.DiffEditor.AppendText(Environment.NewLine + message));
                currentInstance.DiffEditor.Dispatcher.BeginInvoke(action);
            }
        }

        /// <summary>
        /// Clears the diff editor.
        /// </summary>
        public static void ClearDiffEditor()
        {
            if (currentInstance != null)
            {
                var action = new Action(() => currentInstance.DiffEditor.Clear());
                currentInstance.DiffEditor.Dispatcher.BeginInvoke(action);
            }
        }
    }
}
