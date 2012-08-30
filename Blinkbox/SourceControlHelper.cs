// -----------------------------------------------------------------------
// <copyright file="SourceControlHelper.cs" company="blinkbox">
// Wrapper for Source control functionality.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Wrapper for Source control functionality.
    /// </summary>
    public class SourceControlHelper
    {
        /// <summary>
        /// Gets the tracker.
        /// </summary>
        /// <value>The tracker.</value>
        private static GitFileStatusTracker Tracker
        {
            get
            {
                return BasicSccProvider.GetCurrentTracker();
            }
        }

        /// <summary>
        /// Runs the a command asyncronously.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void RunAsync(System.Action action)
        {
            var task = new System.Threading.Tasks.Task(action);
            task.Start();
        }

        /// <summary>
        /// Checks out a branch.
        /// </summary>
        /// <param name="branch">The branch.</param>
        /// <param name="createNew">if set to <c>true</c> creates a new branch.</param>
        public static void CheckOutBranch(string branch, bool createNew = false)
        {
            Tracker.CheckOutBranch(branch, createNew);
        }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns>The working directory</returns>
        public static string GetWorkingDirectory()
        {
            return Tracker.GitWorkingDirectory;
        }

        /// <summary>
        /// Gets the last commit message.
        /// </summary>
        /// <returns>The last commit message</returns>
        public static string GetLastCommitMessage()
        {
            return Tracker.LastCommitMessage.Replace("\n", string.Empty);
        }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns>The working directory</returns>
        public static bool WorkingDirectoryClean()
        {
            return !Tracker.ChangedFiles.Any();
        }

        /// <summary>
        /// Parses git status into a list of files.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>A list of git files. </returns>
        public static IList<GitFile> ParseGitStatus(string status)
        {
            return Tracker.ParseGitStatus(status);
        }

        /// <summary>
        /// Gets the current branch.
        /// </summary>
        /// <returns>The branch</returns>
        public static string GetCurrentBranch()
        {
            return Tracker.CurrentBranch;
        }

        /// <summary>
        /// Gets the latest revision for the specified branch (defaults to current branch).
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <returns>the hash of the latest revision.</returns>
        public static string GetHeadRevisionHash(string branchName = null)
        {
            branchName = branchName ?? Tracker.CurrentBranch;
            var revision = RunGitCommand("rev-parse " + branchName, wait: true, silent: true);
            return revision.Replace("\n", string.Empty); // Git adds a return to the revision
        }

        /// <summary>
        /// Runs a tortoiseGit command.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void RunTortoise(string command)
        {
            var tortoiseGitPath = GitSccOptions.Current.TortoiseGitPath;
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;

                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = tortoiseGitPath;
                process.StartInfo.WorkingDirectory = GetWorkingDirectory();
                process.StartInfo.Arguments = "/command:" + command + " /path:\"" + process.StartInfo.WorkingDirectory + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
                process.WaitForExit();
            }
        }

        /// <summary>
        /// Runs a tortoiseGit command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="wait">if set to <c>true</c> [wait].</param>
        /// <param name="silent">if set to <c>true</c> output is not sent to the notification window.</param>
        /// <returns>the output of the git command.</returns>
        public static string RunGitCommand(string command, bool wait = false, bool silent = false)
        {
            var process = new GitCommand(command) { Silent = silent };
            process = wait ? (GitCommand)process.StartAndWait() : (GitCommand)process.Start();
            return process.Output;
        }

        /// <summary>
        /// Runs a command in the git tfs commandline environment.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="wait">waits for the process to exit before continuing execution.</param>
        public static void RunGitTfs(string command, bool wait = false)
        {
            new SccCommand("cmd.exe", "/k git tfs " + command).StartAndWait();
        }
    }
}
