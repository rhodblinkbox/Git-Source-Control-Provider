// -----------------------------------------------------------------------
// <copyright file="DevelopmentService.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Linq;

    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Implementation of common development processes.
    /// </summary>
    public class DevelopmentService : IDisposable
    {
        /// <summary>
        /// Instance of the  <see cref="notificationService"/>
        /// </summary>
        private readonly NotificationService notificationService;

        /// <summary>
        /// Instance of the  <see cref="SccHelperService"/>
        /// </summary>
        private readonly SccHelperService sccHelper;

        /// <summary>
        /// Instance of the  <see cref="SccProviderService"/>
        /// </summary>
        private readonly SccProviderService sccProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentService"/> class.
        /// </summary>
        /// <param name="sccProvider">The SCC provider.</param>
        /// <param name="notificationService">The notification service.</param>
        /// <param name="sccHelper">The SCC helper.</param>
        public DevelopmentService(SccProviderService sccProvider, NotificationService notificationService, SccHelperService sccHelper)
        {
            this.sccProvider = sccProvider;
            this.notificationService = notificationService;
            this.sccHelper = sccHelper;
        }

        /// <summary>
        /// Describes the current use of the plugin.
        /// </summary>
        public enum DevMode
        {
            /// <summary>
            /// normal developing use
            /// </summary>
            Working = 0,

            /// <summary>
            /// Review in progress
            /// </summary>
            Reviewing = 1,

            /// <summary>
            /// Checking into TFS
            /// </summary>
            Checkin = 2
        }

        /// <summary>
        /// Gets or sets the current mode.
        /// </summary>
        /// <value>The current mode.</value>
        public DevMode CurrentMode { get; set; }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public void GetLatest()
        {
            const string OperationName = "Get Latest";

            try
            {
                this.notificationService.ClearMessages();
                this.notificationService.NewSection("Start " + OperationName);

                if (!this.CheckWorkingDirectoryClean())
                {
                    return;
                }

                // Pull down changes into tfs/default remote branch, and tfs_merge branch
                SccHelperService.RunGitTfs("fetch");

                // Merge without commit from tfs-merge to current branch. 
                SccHelperService.RunGitCommand("merge " + BlinkboxSccOptions.Current.TfsRemoteBranch + " --no-commit", wait: true);

                this.CommitIfRequired();
            }
            catch (Exception e)
            {
                NotificationService.DisplayException(e, "Get Latest Failed");
            }
        }

        /// <summary>
        /// Compare working directory with TFS
        /// </summary>
        public void Review()
        {
            const string OperationName = "Review";

            try
            {
                this.notificationService.ClearMessages();
                this.notificationService.NewSection("Start " + OperationName);

                var currentBranch = this.sccHelper.GetCurrentBranch();

                if (!this.CheckWorkingDirectoryClean() || !this.CheckLatestFromTfs(currentBranch))
                {
                    return;
                }

                // Switch to reviewing mode
                this.CurrentMode = DevMode.Reviewing;

                var diff = SccHelperService.DiffBranches(BlinkboxSccOptions.Current.TfsRemoteBranch, currentBranch);

                var pendingChangesView = BasicSccProvider.GetServiceEx<PendingChangesView>();
                if (pendingChangesView != null)
                {
                    pendingChangesView.Review(diff.ToList(), BlinkboxSccOptions.Current.TfsRemoteBranch);
                }
            }
            catch (Exception e)
            {
                this.CancelReview();
                NotificationService.DisplayException(e, OperationName + " Failed");
            }
        }

        /// <summary>
        /// Cancels a review.
        /// </summary>
        public void CancelReview()
        {
            var pendingChanges = BasicSccProvider.GetServiceEx<PendingChangesView>();
            pendingChanges.EndReview();
            this.CurrentMode = DevMode.Working;
           
            this.sccProvider.RefreshToolWindows();
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public void Checkin()
        {
            const string OperationName = "Check in";

            try
            {
                this.notificationService.ClearMessages();
                this.notificationService.NewSection("Start " + OperationName);

                var currentBranch = this.sccHelper.GetCurrentBranch();

                if (!this.CheckWorkingDirectoryClean() || !this.CheckLatestFromTfs(currentBranch))
                {
                    return;
                }

                // Checkin from tfs-merge branch
                var checkin = SccHelperService.RunGitTfs("checkintool");
            }
            catch (Exception e)
            {
                NotificationService.DisplayException(e, OperationName + " Failed");
            }
            finally
            {
                CancelReview();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // NOop
        }

        /// <summary>
        /// Runs initial checks
        /// </summary>
        /// <returns>true if successful</returns>
        private bool CheckWorkingDirectoryClean()
        {
            if (!this.sccHelper.WorkingDirectoryClean())
            {
                NotificationService.DisplayError("Cannot proceed", "There are uncommitted changes in your working directory");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether the provided branch is ahead and/or behind tfs.
        /// </summary>
        /// <param name="currentBranch">The current branch.</param>
        /// <returns> true if the current branch has the latest revisions from tfs.</returns>
        private bool CheckLatestFromTfs(string currentBranch)
        {
            this.FetchFromTfs();
            var aheadBehind = SccHelperService.BranchAheadOrBehind(currentBranch, BlinkboxSccOptions.Current.TfsRemoteBranch);
            if (aheadBehind.Behind > 0)
            {
                NotificationService.DisplayError(
                    "Cannot proceed", 
                    "The current branch \""  + currentBranch + "\" is " + aheadBehind.Behind + " commits behind TFS. " + Environment.NewLine + "Please Get Latest and then try again.");

                return false;
            }

            this.notificationService.AddMessage("current branch is " + aheadBehind.Ahead + " commits ahead of TFS");
            return true;
        }

        /// <summary>
        /// Fetches from TFS into the tfs/default remote branch.
        /// </summary>
        /// <returns>the output from the git-tfs fetch command.</returns>
        private string FetchFromTfs()
        {
            return SccHelperService.RunGitTfs("fetch", wait: true);
        }

        /// <summary>
        /// Merges if required.
        /// </summary>
        private void CommitIfRequired()
        {
            if (!this.sccHelper.WorkingDirectoryClean())
            {
                this.sccHelper.RunTortoise("commit");
            }
        }
    }
}
