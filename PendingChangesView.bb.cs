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
    using System.Windows.Input;
    using System.Windows.Threading;

    using GitScc.Blinkbox;
    using GitScc.Blinkbox.Options;

    /// <summary>
    /// BB extensions to the PendingChangesView
    /// </summary>
    public partial class PendingChangesView
    {
        /// <summary>
        /// Static reference to the current instance of the PendingChangesView.
        /// </summary>
        private static PendingChangesView currentInstance;

        internal string reviewBranchName = null;

        /// <summary>
        /// Flag indicates that a review is currently in progress.
        /// </summary>
        public bool Reviewing { get; private set; }

        /// <summary>
        /// Show a list of files for review.
        /// </summary>
        /// <param name="changedFiles">
        /// The changed files.
        /// </param>
        public static void Review (List<GitFile> changedFiles, string branchName)
        {
            currentInstance.reviewBranchName = branchName;
            currentInstance.DisplayReview(changedFiles);
        }

        /// <summary>
        /// Cancel the current review and enable the pending changes list. 
        /// </summary>
        public static void CancelReview()
        {
            currentInstance.reviewBranchName = null;
            currentInstance.Reviewing = false;
        }

        /// <summary>
        /// List the provided files in the pending changes window for reivew. 
        /// </summary>
        /// <param name="changedFiles">
        /// The changed files.
        /// </param>
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

        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GetSelectedFileFullName(fileName =>
            {
                // Call tortoiseproc to compare.
                var workingDirectory = service.CurrentTracker.GitWorkingDirectory;
                var tfsRevision = GitTfs.GetLatestRevision(this.service.CurrentTracker.GitWorkingDirectory, BlinkboxSccOptions.Current.TfsMergeBranch);
                var command = Reviewing
                    ? string.Format("diff /path:{0} /startrev:{1} /endrev:{2}", fileName, "0000000000000000000000000000000000000000", tfsRevision)
                    : string.Format("diff /path:{0}", fileName);
                GitTfs.RunTortoise(command, workingDirectory);
            });
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
