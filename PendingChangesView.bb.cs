// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PendingChangesView.bb.cs" company="blinkbox">
//   blinkbox implementation of PendingChangesView.
// </copyright>
// <summary>
//   blinkbox implementation of PendingChangesView.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Controls;
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
        /// The name of the branch to be used for reviewing
        /// </summary>
        private string comparisonBranch = null;

        /// <summary>
        /// Gets a value indicating whether a review is currently in progress.
        /// </summary>
        /// <value><c>true</c> if reviewing; otherwise, <c>false</c>.</value>
        public bool Reviewing { get; private set; }

        /// <summary>
        /// Initialises the blinkbox extensions.
        /// </summary>
        public void InitialiseBlinkboxExtensions()
        {
            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
        }

        /// <summary>
        /// Show a list of files for review.
        /// </summary>
        /// <param name="changedFiles">
        /// The changed files.
        /// </param>
        /// <param name="branchName">
        /// The branch Name.
        /// </param>
        public void Review(List<GitFile> changedFiles, string branchName)
        {
            this.comparisonBranch = branchName;
            this.DisplayReview(changedFiles);
        }

        /// <summary>
        /// Cancel the current review and enable the pending changes list. 
        /// </summary>
        public void CancelReview()
        {
            this.comparisonBranch = null;
            this.Reviewing = false;
        }

        /// <summary>
        /// Writes a message to the diff editor
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteToDiffWindow(string message)
        {
            var action = new Action(() => this.DiffEditor.AppendText(Environment.NewLine + message));
            this.DiffEditor.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Clears the diff editor.
        /// </summary>
        public void ClearDiffEditor()
        {
            var action = new Action(() => this.DiffEditor.Clear());
            this.DiffEditor.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// List the provided files in the pending changes window for reivew. 
        /// </summary>
        /// <param name="changedFiles">
        /// The changed files.
        /// </param>
        internal void DisplayReview(List<GitFile> changedFiles)
        {
            Action act = () =>
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

                service.NoRefresh = true;
                ShowStatusMessage("Getting changed files ...");

                var selectedFile = GetSelectedFileName();
                var selectedFiles = this.dataGrid1.Items.Cast<GitFile>()
                    .Where(i => i.IsSelected)
                    .Select(i => i.FileName).ToList();

                this.dataGrid1.BeginInit();

                try
                {
                    this.dataGrid1.ItemsSource = changedFiles;

                    var view = CollectionViewSource.GetDefaultView(this.dataGrid1.ItemsSource);
                    if (view != null)
                    {
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription(sortMemberPath, sortDirection));
                        view.Refresh();
                    }

                    this.dataGrid1.SelectedValue = selectedFile;
                    selectedFiles.ForEach(fn =>
                    {
                        var item = this.dataGrid1.Items.Cast<GitFile>().FirstOrDefault(i => i.FileName == fn);
                        if (item != null)
                        {
                            item.IsSelected = true;
                        }
                    });

                    ShowStatusMessage(string.Empty);
                }
                catch (Exception ex)
                {
                    ShowStatusMessage(ex.Message);
                }

                this.dataGrid1.EndInit();

                service.NoRefresh = false;
                service.lastTimeRefresh = DateTime.Now;
            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Replaces the double-click functionality with a tortoise-git diff, if available.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!BlinkboxSccOptions.Current.TortoiseAvailable)
            {
                // Compare the file (differs from existing implementation.
                this.GetSelectedFileFullName(fileName =>
                {
                    var sccService = BasicSccProvider.GetServiceEx<SccProviderService>();
                    sccService.CompareFile(fileName, comparisonBranch);
                });
                return;
            }

            // Otherwise, use tortiose git to provide the diff.
            this.GetSelectedFileFullName(fileName =>
            {
                var sccHelper = BasicSccProvider.GetServiceEx<SccHelperService>();

                // Call tortoiseproc to compare.
                var tfsRevision = sccHelper.GetHeadRevisionHash(BlinkboxSccOptions.Current.TfsMergeBranch);
                var command = Reviewing
                    ? string.Format("diff /path:{0} /startrev:{1} /endrev:{2}", fileName, "0000000000000000000000000000000000000000", tfsRevision)
                    : string.Format("diff /path:{0}", fileName);
                sccHelper.RunTortoise(command);
            });
        }

        /// <summary>
        /// Handles the SelectionChanged event of the dataGrid1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.Reviewing)
            {
                // Use the existing implementation
                this.SelectionChanged(sender, e);
                return;
            }

            Action act = () =>
            {
                service.NoRefresh = true;
                try
                {
                    // Diff with TFS
                    var fileName = this.GetSelectedFileName();
                    if (fileName == null)
                    {
                        // file not found locally - clear editor
                        this.ClearEditor();
                        this.diffLines = new string[0];
                        return;
                    }

                    var sccHelper = BasicSccProvider.GetServiceEx<SccHelperService>();

                    var fileNameRel = tracker.GetRelativeFileName(fileName);
                    var tfsRevision = sccHelper.GetHeadRevisionHash(BlinkboxSccOptions.Current.TfsMergeBranch);

                    string diffCommand = string.Format("diff {0} \"{1}\"", tfsRevision, fileNameRel);
                    var diff = SccHelperService.RunGitCommand(diffCommand, silent: true);
                    
                    diffLines = diff.Split(Environment.NewLine.ToCharArray());
                    this.DiffEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(".diff");
                    this.ClearEditor();
                    this.DiffEditor.Text = diff;
                }
                catch (Exception ex)
                {
                    ShowStatusMessage(ex.Message);
                }

                service.NoRefresh = false;
            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
        }
    }
}
