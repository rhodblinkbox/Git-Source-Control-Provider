// -----------------------------------------------------------------------
// <copyright file="SourceControlHelper.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// TODO: Update summary.
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
        /// Gets the current working directory.
        /// </summary>
        /// <returns>The working directory</returns>
        public static string GetWorkingDirectory()
        {
            return Tracker.GitWorkingDirectory;
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
        /// Gets the current branch.
        /// </summary>
        /// <returns>The branch</returns>
        public static string GetCurrentBranch()
        {
            return Tracker.CurrentBranch;
            ////var branchName = RunGitCommand("symbolic-ref -q HEAD");
            ////return branchName.Replace("refs/heads/", string.Empty);
        }

        /// <summary>
        /// Gets the latest revision.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <returns>the hash of the latest revision.</returns>
        public static string GetHeadRevisionHash(string branchName = Blinkbox.Options.BlinkboxSccOptions.HeadRevision)
        {
            var revision = RunGitCommand("rev-parse " + branchName, wait: true);
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
        /// <returns>the output of the git command.</returns>
        public static string RunGitCommand(string command, bool wait = false)
        {
            var process = new GitCommand(command);
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
            new CommandProcess("cmd.exe", "/k git tfs " + command).StartAndWait();
        }
    }
}
