// -----------------------------------------------------------------------
// <copyright file="DevelopmentProcess.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace GitScc.Blinkbox
{
    using System;
    using System.Windows;
    using System.Linq;

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
           if (!InitialChecks("Get Latest"))
           {
               return;
           }

           // store the name of the current branch
           var currentBranch = SourceControlHelper.GetCurrentBranch();

           // Switch to the tfs-merge branch 
           SourceControlHelper.RunGitCommand("checkout " + BlinkboxSccOptions.Current.TfsMergeBranch, wait: true);

           // Pull down changes into tfs remote branch, and tfs_merge branch
           SourceControlHelper.RunGitTfs("pull");

           CommitIfRequired();

           if (!string.IsNullOrEmpty(currentBranch))
           {
               // Switch back to current branch
               SourceControlHelper.RunGitCommand("checkout " + currentBranch, wait: true);

               // Merge without commit from tfs-merge to current branch. 
               SourceControlHelper.RunGitCommand("merge " + BlinkboxSccOptions.Current.TfsRemoteBranch + " --no-commit", wait:true);

               CommitIfRequired();
           }
       }

       /// <summary>
       /// Get the latest from TFS
       /// </summary>
       public static void Review()
       {
           if (!InitialChecks("Review"))
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

       /// <summary>
       /// Merges the current working branch into tfs_merge branch
       /// </summary>
       public static void CompleteReview()
       {
           if (!InitialChecks("Complete Review"))
           {
               return;
           }

           // store the name of the current branch
           var currentBranch = SourceControlHelper.GetCurrentBranch();

           // Switch to the tfs-merge branch 
           SourceControlHelper.RunGitCommand("checkout " + BlinkboxSccOptions.Current.TfsMergeBranch, wait: true);

           // Merge without commit from tfs-merge to current branch. 
           SourceControlHelper.RunGitCommand("merge " + currentBranch, wait: true);

           CommitIfRequired();

           // Switch back to the current branch 
           SourceControlHelper.RunGitCommand("checkout " + currentBranch, wait: true);
       }

       /// <summary>
       /// Get the latest from TFS
       /// </summary>
       public static void Checkin()
       {
           if (!InitialChecks("Checkin"))
           {
               return;
           }

           // store the name of the current branch
           var currentBranch = SourceControlHelper.GetCurrentBranch();

           // Switch to the tfs-merge branch 
           SourceControlHelper.RunGitCommand("checkout " + BlinkboxSccOptions.Current.TfsMergeBranch, wait:true);

           // Checkin from tfs-merge branch
           SourceControlHelper.RunGitTfs("checkintool");

           // Switch back to the current Branch 
           SourceControlHelper.RunGitCommand("checkout " + currentBranch, wait: true);
       }

        /// <summary>
        /// Runs initial checks
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns>true if successful</returns>
        private static bool InitialChecks(string operation)
        {
            try
            {
                if (!SourceControlHelper.WorkingDirectoryClean())
                {
                    MessageBox.Show("Cannot " + operation + " - there are uncommitted changes in your working directory", "Cannot " + operation, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Create the tfs_merge branch (fails silently if it already exists)
                SourceControlHelper.RunGitCommand("branch refs/heads/" + BlinkboxSccOptions.Current.TfsMergeBranch, wait: true);

                NotificationWriter.Clear();
                NotificationWriter.NewSection("Start " + operation);
            }
            catch (Exception e)
            {
                return false;
            }

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
