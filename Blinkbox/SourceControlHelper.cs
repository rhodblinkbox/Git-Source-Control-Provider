// -----------------------------------------------------------------------
// <copyright file="SourceControlHelper.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System.Diagnostics;

    using GitScc.Blinkbox.Commands;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SourceControlHelper
    {
        /// <summary>
        /// Runs a tortoiseGit command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="workingDirectory">The working directory.</param>
        public static void RunTortoise(string command, string workingDirectory)
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
                process.StartInfo.Arguments = "/command:" + command + " /path:\"" + workingDirectory + "\"";
                process.StartInfo.WorkingDirectory = workingDirectory;
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
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="wait">if set to <c>true</c> [wait].</param>
        /// <returns>the output of the git command.</returns>
        public static string RunGitCommand(string command, string workingDirectory, bool wait = false)
        {
            var process = new GitCommand(command, workingDirectory);
            process = wait ? (GitCommand)process.StartAndWait() : (GitCommand)process.Start();
            return process.Output;
        }

        /// <summary>
        /// Runs a command in the git tfs commandline environment.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="wait">waits for the process to exit before continuing execution.</param>
        public static void RunGitTfs(string command, string workingDirectory, bool wait = false)
        {
            new CommandProcess("cmd.exe", "/k git tfs " + command, workingDirectory).StartAndWait();
        }
    }
}
