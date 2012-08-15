// -----------------------------------------------------------------------
// <copyright file="DevelopmentProcess.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Linq;

    using GitScc.Blinkbox.Commands;
    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Git Tfs class.
    /// </summary>
    public class DevelopmentProcess
    {
        private string workingDirectory;

       public DevelopmentProcess(string workingDirectory)
       {
           this.workingDirectory = workingDirectory;
       }



       /// <summary>
       /// Get the latest from TFS
       /// </summary>
       public void GetLatest()
       {
           if (!this.InitialChecks("Get Latest"))
           {
               return;
           }

           // store the name of the current branch
           var currentBranch = this.GetCurrentBranch();

           // Switch to the tfs-merge branch 
           SourceControlHelper.RunGitCommand("checkout " + BlinkboxSccOptions.Current.TfsMergeBranch, this.workingDirectory, wait: true);

           // Pull down changes into tfs remote branch, and tfs_merge branch
           SourceControlHelper.RunGitTfs("pull", this.workingDirectory);

           this.CommitIfRequired();

           if (!string.IsNullOrEmpty(currentBranch))
           {
               // Switch back to current branch
               SourceControlHelper.RunGitCommand("checkout " + currentBranch, this.workingDirectory, wait: true);

               // Merge without commit from tfs-merge to current branch. 
               SourceControlHelper.RunGitCommand("merge " + BlinkboxSccOptions.Current.TfsRemoteBranch + " --no-commit", this.workingDirectory, wait:true);

               this.CommitIfRequired();
           }
       }

       /// <summary>
       /// Get the latest from TFS
       /// </summary>
       public void Review()
       {
           if (!this.InitialChecks("Review"))
           {
               return;
           }

           // store the name of the current branch
           var currentBranch = this.GetCurrentBranch();

           var diffText = SourceControlHelper.RunGitCommand("diff --name-status " + BlinkboxSccOptions.Current.TfsMergeBranch + ".." + currentBranch, this.workingDirectory);
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
       public void CompleteReview()
       {
           if (!this.InitialChecks("Complete Review"))
           {
               return;
           }

           // store the name of the current branch
           var currentBranch = this.GetCurrentBranch();

           // Switch to the tfs-merge branch 
           SourceControlHelper.RunGitCommand("checkout " + BlinkboxSccOptions.Current.TfsMergeBranch, this.workingDirectory, wait: true);

           // Merge without commit from tfs-merge to current branch. 
           SourceControlHelper.RunGitCommand("merge " + currentBranch, this.workingDirectory, wait: true);

           this.CommitIfRequired();

           // Switch back to the current branch 
           SourceControlHelper.RunGitCommand("checkout " + currentBranch, this.workingDirectory, wait: true);
       }

       /// <summary>
       /// Get the latest from TFS
       /// </summary>
       public void Checkin()
       {
           if (!this.InitialChecks("Checkin"))
           {
               return;
           }

           // store the name of the current branch
           var currentBranch = this.GetCurrentBranch();

           // Switch to the tfs-merge branch 
           SourceControlHelper.RunGitCommand("checkout " + BlinkboxSccOptions.Current.TfsMergeBranch, this.workingDirectory, wait:true);

           // Checkin from tfs-merge branch
           SourceControlHelper.RunGitTfs("checkintool", this.workingDirectory);

           // Switch back to the current Branch 
           SourceControlHelper.RunGitCommand("checkout " + currentBranch, this.workingDirectory, wait: true);
       }

        /// <summary>
        /// Runs initial checks
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns>true if successful</returns>
        private bool InitialChecks(string operation)
        {
            try
            {
                if (!this.WorkingDirectoryClean())
                {
                    MessageBox.Show("Cannot " + operation + " - there are uncommitted changes in your working directory", "Cannot " + operation, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Create the tfs_merge branch (fails silently if it already exists)
                SourceControlHelper.RunGitCommand("branch refs/heads/" + BlinkboxSccOptions.Current.TfsMergeBranch, this.workingDirectory, wait: true);

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
        /// Get the name of the current branch.
        /// </summary>
        /// <returns>the name of the current branch</returns>
        private string GetCurrentBranch()
        {
            var branchName = SourceControlHelper.RunGitCommand("symbolic-ref -q HEAD", this.workingDirectory);
            return branchName.Replace("refs/heads/", string.Empty);
        }

        /// <summary> 
        /// Checks whether the working directory is clean.
        /// </summary>
        /// <returns>
        /// true if the working directory is clean.
        /// </returns>
        private bool WorkingDirectoryClean()
        {
            return string.IsNullOrEmpty(SourceControlHelper.RunGitCommand("status --porcelain", this.workingDirectory));
        }

        /// <summary>
        /// Merges if required.
        /// </summary>
        private void CommitIfRequired()
        {
            if (!this.WorkingDirectoryClean())
            {
                SourceControlHelper.RunTortoise("commit", this.workingDirectory);
            }
        }

        /// <summary>
        /// Gets the latest revision.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <returns>the hash of the latest revision.</returns>
        public string GetLatestRevision(string branchName = Blinkbox.Options.BlinkboxSccOptions.HeadRevision)
        {
            var revision = SourceControlHelper.RunGitCommand("rev-parse " + branchName, this.workingDirectory, wait:true);
            return revision.Replace("\n", string.Empty); // Git adds a return to the revision
        }
    }
}
