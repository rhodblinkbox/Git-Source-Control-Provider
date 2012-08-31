// -----------------------------------------------------------------------
// <copyright file="DevelopmentProcess.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Linq;

    using GitScc.Blinkbox.Data;
    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Implementation of common development processes.
    /// </summary>
    public class DevelopmentProcess : IDisposable
    {
        /// <summary>
        /// Instance of the  <see cref="NotificationService"/>
        /// </summary>
        private NotificationService notificationServiceInstance = null;

        /// <summary>
        /// Instance of the  <see cref="SccHelperService"/>
        /// </summary>
        private SccHelperService sccHelperInstance = null;

        /// <summary>
        /// Gets the notification service.
        /// </summary>
        /// <value>The notification service.</value>
        private NotificationService NotificationService
        {
            get
            {
                this.notificationServiceInstance = this.notificationServiceInstance ?? BasicSccProvider.GetServiceEx<NotificationService>();
                return this.notificationServiceInstance;
            }
        }

        /// <summary>
        /// Gets the sccHelper service.
        /// </summary>
        /// <value>The sccHelper service.</value>
        private SccHelperService SccHelper
        {
            get
            {
                this.sccHelperInstance = this.sccHelperInstance ?? BasicSccProvider.GetServiceEx<SccHelperService>();
                return this.sccHelperInstance;
            }
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public void GetLatest()
        {
            const string OperationName = "Get Latest";

            try
            {
                this.NotificationService.ClearMessages();
                this.NotificationService.NewSection("Start " + OperationName);

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
        /// Get the latest from TFS
        /// </summary>
        public void Review()
        {
            const string OperationName = "Review";

            try
            {

                this.NotificationService.ClearMessages();
                this.NotificationService.NewSection("Start " + OperationName);

                var currentBranch = this.SccHelper.GetCurrentBranch();

                if (!this.CheckWorkingDirectoryClean() || !this.CheckLatestFromTfs(currentBranch))
                {
                    return;
                }

                var diff = SccHelperService.DiffBranches(BlinkboxSccOptions.Current.TfsRemoteBranch, currentBranch);

                var pendingChangesView = BasicSccProvider.GetServiceEx<PendingChangesView>();
                if (pendingChangesView != null)
                {
                    pendingChangesView.Review(diff.ToList(), BlinkboxSccOptions.Current.TfsRemoteBranch);
                }
            }
            catch (Exception e)
            {
                NotificationService.DisplayException(e, OperationName + " Failed");
            }
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public void Checkin()
        {
            const string OperationName = "Check in";

            try
            {
                this.NotificationService.ClearMessages();
                this.NotificationService.NewSection("Start " + OperationName);

                var currentBranch = this.SccHelper.GetCurrentBranch();

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
            if (!this.SccHelper.WorkingDirectoryClean())
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

            NotificationService.AddMessage("current branch is " + aheadBehind.Ahead + " commits ahead of TFS");
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
            if (!this.SccHelper.WorkingDirectoryClean())
            {
                this.SccHelper.RunTortoise("commit");
            }
        }
    }
}
