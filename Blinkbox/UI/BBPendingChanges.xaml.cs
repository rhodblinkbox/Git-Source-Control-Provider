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
    using System.Windows.Threading;

    using GitScc.Blinkbox.Data;

    /// <summary>
    /// Interaction logic for BBPendingChanges.xaml
    /// </summary>
    public partial class BBPendingChanges : UserControl
    {

        private GitFileStatusTracker tracker;

        public TabItem ReviewTab
        {
            get
            {
                return reviewTabItem;
            }
        }

        public TabItem GitTab
        {
            get
            {
                return gitTabItem;
            }
        }

        public TabItem DeployTab
        {
            get
            {
                return deployTabItem;
            }
        }

        public BBPendingChanges()
        {
            InitializeComponent();
        }

        public void Commit()
        {
            
        }

        public void AmendCommit()
        {
            
        }

        public void RefreshPendingChanges(GitFileStatusTracker tracker)
        {
            this.tracker = tracker;
        }

        public GitFileStatusTracker GetTracker()
        {
            return this.tracker;
        }

        public void OpenFile(string fileName)
        {
            // Call pending changes version
        }

        public void Review(List<GitFile> changedFiles, string branchName = null)
        {
            if (changedFiles.Any())
            {
                ////this.comparisonBranch = branchName;
                reviewTab.DisplayReview(changedFiles);
            }
            
        }

        public void EndReview()
        {
            reviewTab.EndReview();
        }

        /// <summary>
        /// Updates the TFS status.
        /// </summary>
        /// <param name="aheadBehind">The ahead behind.</param>
        public void UpdateTfsStatus(AheadBehind aheadBehind)
        {
            Action action = () =>
            {
                var text = string.Format("{0} ahead, {1} behind TFS", aheadBehind.Ahead, aheadBehind.Behind);
                TfsStatusLabel.Content = text;
                TfsStatusLabel.Foreground = aheadBehind.Behind == 0 ? System.Windows.Media.Brushes.DarkGreen : System.Windows.Media.Brushes.DarkRed;
            };
            this.Dispatcher.BeginInvoke(action, DispatcherPriority.ApplicationIdle);
        }
    }
}
