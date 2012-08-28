// -----------------------------------------------------------------------
// <copyright file="DevelopmentProcess.cs" company="blinkbox">
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
    /// Git Tfs class.
    /// </summary>
    public class DevelopmentProcess
    {
        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public static void GetLatest()
        {
            const string OperationName = "Get Latest";

            try
            {
                if (!InitialChecks(OperationName))
                {
                    return;
                }

                // store the name of the current branch
                var currentBranch = SourceControlHelper.GetCurrentBranch();

                // Switch to the tfs-merge branch 
                SourceControlHelper.CheckOutBranch(BlinkboxSccOptions.Current.TfsMergeBranch);

                // Pull down changes into tfs remote branch, and tfs_merge branch
                SourceControlHelper.RunGitTfs("pull");

                CommitIfRequired();

                if (!string.IsNullOrEmpty(currentBranch))
                {
                    // Switch back to current branch
                    SourceControlHelper.CheckOutBranch(currentBranch);

                    // Merge without commit from tfs-merge to current branch. 
                    SourceControlHelper.RunGitCommand("merge " + BlinkboxSccOptions.Current.TfsRemoteBranch + " --no-commit", wait: true);

                    CommitIfRequired();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Get Latest Failed");
            }
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public static void Review()
        {
            const string OperationName = "Review";

            try
            {
                if (!InitialChecks(OperationName))
                {
                    return;
                }

                // store the name of the current branch
                var currentBranch = SourceControlHelper.GetCurrentBranch();

                var diffText = SourceControlHelper.RunGitCommand("diff --name-status " + BlinkboxSccOptions.Current.TfsMergeBranch + ".." + currentBranch);
                var diffList = diffText.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var gitFiles = diffList.Select(GitFile.FromDiff);
                if (gitFiles.Count() > 0)
                {
                    PendingChangesView.Review(gitFiles.ToList(), BlinkboxSccOptions.Current.TfsMergeBranch);
                }
                else
                {
                    NotificationWriter.Write("No changes found to review");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Get Latest Failed");
            }
        }

        /// <summary>
        /// Merges the current working branch into tfs_merge branch
        /// </summary>
        public static void CompleteReview()
        {
            const string OperationName = "Complete Review";

            try
            {
                if (!InitialChecks(OperationName))
                {
                    return;
                }

                // store the name of the current branch
                var currentBranch = SourceControlHelper.GetCurrentBranch();

                // Switch to the tfs-merge branch 
                SourceControlHelper.CheckOutBranch(BlinkboxSccOptions.Current.TfsMergeBranch);

                // Merge without commit from tfs-merge to current branch. 
                SourceControlHelper.RunGitCommand("merge " + currentBranch, wait: true);

                CommitIfRequired();

                // Switch back to the current branch 
                SourceControlHelper.CheckOutBranch(currentBranch);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, OperationName + " Failed");
            }
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        public static void Checkin()
        {
            const string OperationName = "Check in";

            try
            {
                if (!InitialChecks(OperationName))
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
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, OperationName + " Failed");
            }
        }

        /// <summary>
        /// Runs initial checks
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns>true if successful</returns>
        private static bool InitialChecks(string operation)
        {
            if (!SourceControlHelper.WorkingDirectoryClean())
            {
                MessageBox.Show("Cannot " + operation + " - there are uncommitted changes in your working directory", "Cannot " + operation, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Create the tfs_merge branch (fails silently if it already exists)
            SourceControlHelper.RunGitCommand("branch refs/heads/" + BlinkboxSccOptions.Current.TfsMergeBranch, wait: true, silent: true);

            NotificationWriter.Clear();
            NotificationWriter.NewSection("Start " + operation);

            return true;
        }


        /// <summary>
        /// Merges if required.
        /// </summary>
        private static void CommitIfRequired()
        {
            if (!SourceControlHelper.WorkingDirectoryClean())
            {
                SourceControlHelper.RunTortoise("commit");
            }
        }
    }
}
