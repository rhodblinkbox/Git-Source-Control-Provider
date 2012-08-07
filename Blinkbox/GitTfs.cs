// -----------------------------------------------------------------------
// <copyright file="GitTfs.cs" company="blinkbox">
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

    /// <summary>
    /// Git Tfs class.
    /// </summary>
    public class GitTfs
    {
        /// <summary>
        /// the name of the tfs merge branch
        /// </summary>
        private static readonly string tfsMergeBranch = "tfs_merge_test";

        /// <summary>
        /// The name of the tfs remote branch
        /// </summary>
        private static readonly string tfsRemoteBranch = "remotes/tfs/default";

        /// <summary>
        /// Commands which appear in the git tfs menu
        /// </summary>
        private static readonly List<GitTfsCommand> menuOptions = new List<GitTfsCommand>();

        /// <summary>
        /// Initializes static members of the <see cref="GitTfs"/> class.
        /// </summary>
        static GitTfs()
        {

            menuOptions.Add(new GitTfsCommand()
            {
                Name = "Review (compare to tfs_merge branch)",
                CommandId = Blinkbox.CommandIds.GitTfsReviewButtonId,
                Handler = (workingDir) => Review(workingDir)
            });

            menuOptions.Add(new GitTfsCommand()
            {
                Name = "Complete Review (merge to tfs_merge branch)",
                CommandId = Blinkbox.CommandIds.GitTfsCompleteReviewButtonId,
                Handler = (workingDir) => CompleteReview(workingDir)
            });

            menuOptions.Add(new GitTfsCommand()
            {
                Name = "Check in from tfs_merge", 
                CommandId = Blinkbox.CommandIds.GitTfsCheckinButtonId,
                Handler = (workingDir) => Checkin(workingDir)
            });

            menuOptions.Add(new GitTfsCommand()
            {
                Name = "Get Latest (via tfs_merge)", 
                CommandId = Blinkbox.CommandIds.GitTfsGetLatestButtonId,
                Handler = (workingDir) => GetLatest(workingDir)
            });

            menuOptions.Add(new GitTfsCommand()
            {
                Name = "Clean Workspace",
                CommandId = Blinkbox.CommandIds.GitTfsCleanWorkspacesButtonId,
                Handler = (workingDir) => RunGitTfs("cleanup-workspaces", workingDir)
            });
        }

        /// <summary>
        /// Gets the menu options.
        /// </summary>
        /// <value>The menu options.</value>
        public static List<GitTfsCommand> MenuOptions 
        { 
            get
            {
                return menuOptions;
            } 
        }

        /// <summary>
        /// Runs a command in the git tfs commandline environment.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        /// <param name="wait">
        /// waits for the process to exit before continuing execution.
        /// </param>
        public static void RunGitTfs(string command, string workingDirectory, bool wait = false)
        {
            var process = new Command("cmd.exe", "/k git tfs " + command, workingDirectory) { WaitUntilFinished = wait };
            process.Start();
        }

        /// <summary>
        /// Runs initial checks
        /// </summary>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        /// <returns>
        /// true if successful
        /// </returns>
        private static bool InitialChecks(string workingDirectory)
        {
            try
            {
                var cleanWorkingDirectory = string.IsNullOrEmpty(GitBash.Run("status --porcelain", workingDirectory));
                if (!cleanWorkingDirectory)
                {
                    MessageBox.Show("Cannot Get Latest - there are uncommitted changes in your working directory", "Cannot Get Latest", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Create the tfs_merge branch (fails silently if it already exists)
                GitBash.Run("branch refs/heads/" + tfsMergeBranch, workingDirectory);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        /// <param name="workingDirectory">
        /// The working Directory.
        /// </param>
        public static void GetLatest(string workingDirectory)
        {
            if (!InitialChecks(workingDirectory))
            {
                return;
            }

            // store the name of the current branch
            var currentBranch = GetCurrentBranch(workingDirectory);

            // Switch to the tfs-merge branch 
            var checkoutTfsMerge = new GitCommand("checkout " + tfsMergeBranch, workingDirectory).StartAndWait();

            // Pull down changes into tfs remote branch, and tfs_merge branch
            RunGitTfs("pull", workingDirectory);

            CommitIfRequired(workingDirectory);

            if (!string.IsNullOrEmpty(currentBranch))
            {
                // Switch back to current branch
                var checkoutCurrentBranch = new GitCommand("checkout " + currentBranch, workingDirectory).StartAndWait();

                // Merge without commit from tfs-merge to current branch. 
                var mergeLatestToCurrent = new GitCommand("merge " + tfsRemoteBranch + " --no-commit", workingDirectory).StartAndWait();

                CommitIfRequired(workingDirectory);
            }
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        /// <param name="workingDirectory">
        /// The working Directory.
        /// </param>
        public static void Review(string workingDirectory)
        {
            var cleanWorkingDirectory = string.IsNullOrEmpty(GitBash.Run("status --porcelain", workingDirectory));
            if (!cleanWorkingDirectory)
            {
                MessageBox.Show("Cannot Checkin - there are uncommitted changes in your working directory", "Cannot Checkin", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // store the name of the current branch
            var currentBranch = GetCurrentBranch(workingDirectory);

            var compareCommand = new GitCommand("diff --name-status " + currentBranch + ".." + tfsMergeBranch, workingDirectory).Start();
            var diffList = compareCommand.Output.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var files = diffList.Select(x => x.Split("\t".ToCharArray())).Select(x => new { Status = x[0], File = x[1] });
            var gitFiles = files.Select(f => new GitFile() { FileName = f.File, Status = GitFileStatus.Modified });
            PendingChangesView.Review(gitFiles.ToList());
        }

        /// <summary>
        /// Merges the current working branch into tfs_merge branch
        /// </summary>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        public static void CompleteReview(string workingDirectory)
        {
            var cleanWorkingDirectory = string.IsNullOrEmpty(GitBash.Run("status --porcelain", workingDirectory));
            if (!cleanWorkingDirectory)
            {
                MessageBox.Show("Cannot Get Latest - there are uncommitted changes in your working directory", "Cannot Get Latest", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // store the name of the current branch
            var currentBranch = GitBash.Run("symbolic-ref -q HEAD", workingDirectory);

            // Create the tfs_merge branch (fails silently if it already exists)
            GitBash.Run("branch refs/heads/" + tfsMergeBranch, workingDirectory);

            // Switch to the tfs-merge branch 
            GitBash.Run("checkout " + tfsMergeBranch, workingDirectory);

            // Merge without commit from tfs-merge to current branch. 
            GitBash.Run("git merge " + currentBranch + " --no-commit", workingDirectory);

            // show the tortoise commit tool
            RunTortoise("commit", workingDirectory);
        }

        /// <summary>
        /// Get the latest from TFS
        /// </summary>
        /// <param name="workingDirectory">
        /// The working Directory.
        /// </param>
        public static void Checkin(string workingDirectory)
        {
            var cleanWorkingDirectory = string.IsNullOrEmpty(GitBash.Run("status --porcelain", workingDirectory));
            if (!cleanWorkingDirectory)
            {
                MessageBox.Show("Cannot Checkin - there are uncommitted changes in your working directory", "Cannot Checkin", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // store the name of the current branch
            var currentBranch = GetCurrentBranch(workingDirectory);

            // Create the tfs_merge branch (fails silently if it already exists)
            new GitCommand("branch refs/heads/" + tfsMergeBranch, workingDirectory);

            // Switch to the tfs-merge branch 
            new GitCommand("checkout " + tfsMergeBranch, workingDirectory);

            // Run a tortoise merge . 
            new GitCommand("merge " + currentBranch + " --no-commit", workingDirectory);

            // Checkin from tfs-merge branch
            RunGitTfs("checkintool", workingDirectory);

            // Switch back to the current Branch 
            new GitCommand("checkout " + currentBranch, workingDirectory);
        }

        /// <summary>
        /// Get the name of the current branch. 
        /// </summary>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        /// <returns>
        /// the name of the current branch
        /// </returns>
        private static string GetCurrentBranch(string workingDirectory)
        {
            var versionCommand = new GitCommand("symbolic-ref -q HEAD", workingDirectory).StartAndWait();
            return versionCommand.Output.Replace("refs/heads/", string.Empty);
        }

        /// <summary> 
        /// Checks whether the working directory is clean.
        /// </summary>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        /// <returns>
        /// true if the working directory is clean.
        /// </returns>
        private static bool WorkingDirectoryClean(string workingDirectory)
        {
            return string.IsNullOrEmpty(GitBash.Run("status --porcelain", workingDirectory));
        }

        /// <summary>
        /// Merges if required.
        /// </summary>
        /// <param name="workingDirectory">
        /// The working Directory.
        /// </param>
        private static void CommitIfRequired(string workingDirectory)
        {
            if (!WorkingDirectoryClean(workingDirectory))
            {
                RunTortoise("commit", workingDirectory);
            }
        }

        /// <summary>
        /// Runs a tortoiseGit command.
        /// </summary>
        /// <param name="command">
        /// The tortoiseGit command.
        /// </param>
        /// <param name="workingDirectory">
        /// The working Directory.
        /// </param>
        public static void RunTortoise(string command, string workingDirectory)
        {
            var tortoiseGitPath = GitSccOptions.Current.TortoiseGitPath;
            RunDetatched(tortoiseGitPath, "/command:" + command + " /path:\"" + workingDirectory + "\"", workingDirectory);
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;

                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = tortoiseGitPath;
                process.StartInfo.Arguments = "/command:" + command + " /path:\"" + workingDirectory + "\"";
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Runs a command.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory.
        /// </param>
        public static void RunDetatched(string command, string arguments, string workingDirectory)
        {
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;

                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
                process.WaitForExit();
            }
        } 


        /// <summary>
        /// Defines a git tfs command
        /// </summary>
        public class GitTfsCommand
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets CommandId.
            /// </summary>
            public int CommandId { get; set; }

            /// <summary>
            /// Gets or sets the command handler.
            /// </summary>
            public Action<string> Handler { get; set; }
        }


    }
}
