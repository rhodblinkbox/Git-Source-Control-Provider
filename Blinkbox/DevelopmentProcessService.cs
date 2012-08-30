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
    public class DevelopmentProcessService : IDisposable
    {
        /// <summary>
        /// Instance of the  <see cref="NotificationService"/>
        /// </summary>
        private NotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentProcessService"/> class.
        /// </summary>
        /// <param name="notificationService">The notification service.</param>
        public DevelopmentProcessService(NotificationService notificationService)
        {
            this.notificationService = notificationService;
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
                var currentBranch = SourceControlHelper.GetCurrentBranch();

                // Switch to the tfs-merge branch 
                SourceControlHelper.CheckOutBranch(BlinkboxSccOptions.Current.TfsMergeBranch);

                // Pull down changes into tfs remote branch, and tfs_merge branch
                SourceControlHelper.RunGitTfs("pull");

                this.CommitIfRequired();

                // Switch back to current branch
                SourceControlHelper.CheckOutBranch(currentBranch);

                // Merge without commit from tfs-merge to current branch. 
                SourceControlHelper.RunGitCommand("merge " + BlinkboxSccOptions.Current.TfsRemoteBranch + " --no-commit", wait: true);

                this.CommitIfRequired();
            }
            catch (Exception e)
            {
                this.notificationService.DisplayException(e, "Get Latest Failed");
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
                var currentBranch = SourceControlHelper.GetCurrentBranch();

                var diffText = SourceControlHelper.RunGitCommand("diff --name-status " + BlinkboxSccOptions.Current.TfsMergeBranch + ".." + currentBranch);
                var diffList = diffText.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var gitFiles = diffList.Select(GitFile.FromDiff);
                
                if (gitFiles.Any())
                {
                    PendingChangesView.Review(gitFiles.ToList(), BlinkboxSccOptions.Current.TfsMergeBranch);
                }
                else
                {
                    this.notificationService.AddMessage("No changes found to review");
                }
            }
            catch (Exception e)
            {
                this.notificationService.DisplayException(e, "Get Latest Failed");
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
                var currentBranch = SourceControlHelper.GetCurrentBranch();

                // Switch to the tfs-merge branch 
                SourceControlHelper.CheckOutBranch(BlinkboxSccOptions.Current.TfsMergeBranch);

                // Merge without commit from tfs-merge to current branch. 
                SourceControlHelper.RunGitCommand("merge " + currentBranch, wait: true);

                this.CommitIfRequired();

                // Switch back to the current branch 
                SourceControlHelper.CheckOutBranch(currentBranch);
            }
            catch (Exception e)
            {
                this.notificationService.DisplayException(e, OperationName + " Failed");
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
                var currentBranch = SourceControlHelper.GetCurrentBranch();

                // Switch to the tfs-merge branch 
                SourceControlHelper.CheckOutBranch(BlinkboxSccOptions.Current.TfsMergeBranch);

                // Checkin from tfs-merge branch
                SourceControlHelper.RunGitTfs("checkintool");

                // Switch back to the current Branch 
                SourceControlHelper.CheckOutBranch(currentBranch);
            }
            catch (Exception e)
            {
                this.notificationService.DisplayException(e, OperationName + " Failed");
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
            if (!SourceControlHelper.WorkingDirectoryClean())
            {
                MessageBox.Show("Cannot " + operation + " - there are uncommitted changes in your working directory", "Cannot " + operation, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Create the tfs_merge branch (fails silently if it already exists)
            SourceControlHelper.RunGitCommand("branch refs/heads/" + BlinkboxSccOptions.Current.TfsMergeBranch, wait: true, silent: true);

            this.notificationService.ClearMessages();
            this.notificationService.NewSection("Start " + operation);

            return true;
        }

        /// <summary>
        /// Merges if required.
        /// </summary>
        private void CommitIfRequired()
        {
            if (!SourceControlHelper.WorkingDirectoryClean())
            {
                SourceControlHelper.RunTortoise("commit");
            }
        }
    }
}
