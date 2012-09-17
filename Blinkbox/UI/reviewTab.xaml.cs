using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitScc.Blinkbox.UI
{
    /// <summary>
    /// Interaction logic for reviewTab.xaml
    /// </summary>
    public partial class reviewTab : UserControl
    {
        public reviewTab()
        {
            InitializeComponent();
        }

        private void reviewList_Sorting(object sender, DataGridSortingEventArgs e)
        {

        }

        /// <summary>
        /// List the provided files in the pending changes window for reivew. 
        /// </summary>
        /// <param name="changedFiles">
        /// The changed files.
        /// </param>
        internal void DisplayReview(List<GitFile> changedFiles)
        {
            /*
            Action act = () =>
            {
                if (!GitBash.Exists)
                {
                    Settings.Show();
                    return;
                }

                Settings.Hide();
                reviewTab.IsEnabled = true;

                //////if (tracker == null)
                //////{
                //////    service.NoRefresh = true;
                //////    ClearUI();
                //////    service.NoRefresh = false;
                //////    return;
                //////}

                ////service.NoRefresh = true;

                ////var selectedFile = GetSelectedFileName();
                ////var selectedFiles = this.dataGrid1.Items.Cast<GitFile>()
                ////    .Where(i => i.IsSelected)
                ////    .Select(i => i.FileName).ToList();

                this.reviewList.BeginInit();

                try
                {
                    this.reviewList.ItemsSource = changedFiles;

                    var view = CollectionViewSource.GetDefaultView(this.reviewList.ItemsSource);
                    if (view != null)
                    {
                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription(sortMemberPath, sortDirection));
                        view.Refresh();
                    }

                    ////this.reviewList.SelectedValue = selectedFile;
                    ////selectedFiles.ForEach(fn =>
                    ////{
                    ////    var item = this.reviewList.Items.Cast<GitFile>().FirstOrDefault(i => i.FileName == fn);
                    ////    if (item != null)
                    ////    {
                    ////        item.IsSelected = true;
                    ////    }
                    ////});

                    ShowStatusMessage(string.Empty);
                }
                catch (Exception ex)
                {
                    ShowStatusMessage(ex.Message);
                }

                this.reviewList.EndInit();

                reviewTab.Focus();

                service.NoRefresh = false;
                service.lastTimeRefresh = DateTime.Now;
            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
             * */
        }

        /// <summary>
        /// Ends the review.
        /// </summary>
        public void EndReview()
        {
            /*
            Action act = () =>
            {
                reviewTab.IsEnabled = false;
                gitTab.Focus();
            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
             * */
        }

        /// <summary>
        /// Handles the SelectionChanged event of the Review control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the event data.</param>
        public void Review_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            Action act = () =>
            {
                service.NoRefresh = true;
                try
                {
                    this.ReviewDiff.Text = string.Empty;
                    this.diffLines = new string[0];

                    var reviewItem = this.GetSelectedReviewItem();
                    if (reviewItem == null || string.IsNullOrEmpty(reviewItem.FileName))
                    {
                        return;
                    }

                    // Diff the selected file against the tfs version using git
                    var sccHelper = BasicSccProvider.GetServiceEx<SccHelperService>();
                    var fileNameRel = tracker.GetRelativeFileName(reviewItem.FileName);
                    var tfsRevision = sccHelper.GetHeadRevisionHash(BlinkboxSccOptions.Current.TfsRemoteBranch);
                    var diff = sccHelper.DiffFileWithGit(fileNameRel, tfsRevision);

                    // Show in the diff window
                    diffLines = diff.Split(Environment.NewLine.ToCharArray());
                    this.ReviewDiff.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(".diff");
                    this.ReviewDiff.Text = diff;
                }
                catch (Exception ex)
                {
                    ShowStatusMessage(ex.Message);
                }

                service.NoRefresh = false;
            };

            this.Dispatcher.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
             * */
        }

        /// <summary>
        /// Opens a tortoise diff.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Review_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*
            // Otherwise, use tortiose git to provide the diff.
            var reviewItem = this.GetSelectedReviewItem();
            if (reviewItem != null && !string.IsNullOrEmpty(reviewItem.FileName))
            {
                var fileName = System.IO.Path.Combine(this.tracker.GitWorkingDirectory, reviewItem.FileName);

                if (reviewItem.Status == GitFileStatus.Added)
                {
                    this.OpenFile(fileName);
                    return;
                }

                var sccHelper = BasicSccProvider.GetServiceEx<SccHelperService>();

                // Diff the file in tortoise
                var tfsRevision = sccHelper.GetHeadRevisionHash(BlinkboxSccOptions.Current.TfsRemoteBranch);
                sccHelper.DiffFileInTortoise(fileName, tfsRevision, BlinkboxSccOptions.WorkingDirectoryRevision);
            }
             * */
        }

        private void DiffEditor_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*
            int start = 1, column = 1;
            try
            {
                if (diffLines != null && diffLines.Length > 0)
                {
                    int line = this.DiffEditor.TextArea.Caret.Line;
                    column = this.DiffEditor.TextArea.Caret.Column;

                    string text = diffLines[line];
                    while (line >= 0)
                    {
                        var match = Regex.Match(text, "^@@(.+)@@");
                        if (match.Success)
                        {
                            var s = match.Groups[1].Value;
                            s = s.Substring(s.IndexOf('+') + 1);
                            s = s.Substring(0, s.IndexOf(','));
                            start += Convert.ToInt32(s) - 2;
                            break;
                        }
                        else if (text.StartsWith("-"))
                        {
                            start--;
                        }

                        start++;
                        --line;
                        text = line >= 0 ? diffLines[line] : "";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.Message);
                Log.WriteLine("Pending Changes View - DiffEditor_MouseDoubleClick: {0}", ex.ToString());
            }
            GetSelectedFileFullName((fileName) =>
            {
                OpenFile(fileName);
                var dte = BasicSccProvider.GetServiceEx<EnvDTE.DTE>();
                var selection = dte.ActiveDocument.Selection as EnvDTE.TextSelection;
                selection.MoveToLineAndOffset(start - 1, column);
            });
            */
        }

        /// <summary>
        /// Gets the selected review item.
        /// </summary>
        /// <returns></returns>
        private GitFile GetSelectedReviewItem()
        {
            if (this.reviewList.SelectedCells.Any())
            {
                var selectedItem = this.reviewList.SelectedCells[0].Item as GitFile;
                if (selectedItem != null)
                {
                    return selectedItem;
                }
            }

            return null;
        }
    }
}
