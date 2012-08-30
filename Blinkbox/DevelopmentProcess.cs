// -----------------------------------------------------------------------
// <copyright file="DevelopmentProcessService.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Linq;
    using System.Windows;

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
                if (!this.InitialChecks(OperationName))
                {
                    return;
                }

                // store the name of the current branch
                var currentBranch = this.SccHelper.GetCurrentBranch();

                // Switch to the tfs-merge branch 
                this.SccHelper.CheckOutBranch(BlinkboxSccOptions.Current.TfsMergeBranch);

                // Pull down changes into tfs remote branch, and tfs_merge branch
                SccHelperService.RunGitTfs("pull");

                this.CommitIfRequired();

                // Switch back to current branch
                this.SccHelper.CheckOutBranch(currentBranch);

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
                if (!this.InitialChecks(OperationName))
                {
                    return;
                }

                // store the name of the current branch
                var currentBranch = this.SccHelper.GetCurrentBranch();

                var diffText = SccHelperService.RunGitCommand("diff --name-status " + BlinkboxSccOptions.Current.TfsMergeBranch + ".." + currentBranch);
                var diffList = diffText.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var gitFiles = diffList.Select(GitFile.FromDiff);
                
                if (gitFiles.Any())
                {
                    var pendingChangesView = BasicSccProvider.GetServiceEx<PendingChangesView>();
                    if (pendingChangesView != null)
                    {
                        pendingChangesView.Review(gitFiles.ToList(), BlinkboxSccOptions.Current.TfsMergeBranch);
                    }
                }
                else
                {
                    this.NotificationService.AddMessage("No changes found to review");
                }
            }
            catch (Exception e)
            {
                NotificationService.DisplayException(e, "Get Latest Failed");
            }
        }

        /// <summary>
        /// Merges the current working branch into tfs_merge branch
        /// </summary>
        public void CompleteReview()
        {
            const string OperationName = "Complete Review";

            try
            {
                if (!this.InitialChecks(OperationName))
                {
                    return;
                }

                // store the name of the current branch
                var currentBranch = this.SccHelper.GetCurrentBranch();

                // Switch to the tfs-merge branch 
                this.SccHelper.CheckOutBranch(BlinkboxSccOptions.Current.TfsMergeBranch);

                // Merge without commit from tfs-merge to current branch. 
                SccHelperService.RunGitCommand("merge " + currentBranch, wait: true);

                this.CommitIfRequired();

                // Switch back to the current branch 
                this.SccHelper.CheckOutBranch(currentBranch);
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
                if (!this.InitialChecks(OperationName))
                {
                    return;
                }

                // store the name of the current branch
                var currentBranch = this.SccHelper.GetCurrentBranch();

                // Switch to the tfs-merge branch 
                this.SccHelper.CheckOutBranch(BlinkboxSccOptions.Current.TfsMergeBranch);

                // Checkin from tfs-merge branch
                SccHelperService.RunGitTfs("checkintool");

                // Switch back to the current Branch 
                this.SccHelper.CheckOutBranch(currentBranch);
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
        /// <param name="operation">The operation.</param>
        /// <returns>true if successful</returns>
        private bool InitialChecks(string operation)
        {
            if (!this.SccHelper.WorkingDirectoryClean())
            {
                NotificationService.DisplayError("Cannot " + operation + " - there are uncommitted changes in your working directory", "Cannot " + operation);
                return false;
            }

            // Create the tfs_merge branch (fails silently if it already exists)
            SccHelperService.RunGitCommand("branch refs/heads/" + BlinkboxSccOptions.Current.TfsMergeBranch, wait: true, silent: true);

            this.NotificationService.ClearMessages();
            this.NotificationService.NewSection("Start " + operation);

            return true;
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
