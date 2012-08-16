// -----------------------------------------------------------------------
// <copyright file="SourceControlHelper.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc
{
    using System.Diagnostics;

    using GitScc.Blinkbox.Commands;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SourceControlHelper
    {

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <value>The working directory.</value>
        private static string WorkingDirectory
        {
            get
            {
                return BasicSccProvider.GetWorkingDirectory();
            }
        }


        /// <summary>
        /// Checks whether the working directory is clean.
        /// </summary>
        /// <returns>true if the working directory is clean.</returns>
        public static bool WorkingDirectoryClean()
        {
            return string.IsNullOrEmpty(RunGitCommand("status --porcelain"));
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
                process.StartInfo.Arguments = "/command:" + command + " /path:\"" + WorkingDirectory + "\"";
                process.StartInfo.WorkingDirectory = WorkingDirectory;
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
