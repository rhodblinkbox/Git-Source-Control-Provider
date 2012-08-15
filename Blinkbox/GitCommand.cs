// -----------------------------------------------------------------------
// <copyright file="GitCommand.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Executes a command in git.
    /// </summary>
    public class GitCommand : CommandProcess
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitCommand"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="workingDirectory">The working directory.</param>
        public GitCommand(string command, string workingDirectory)
            : base(GitBash.GitExePath, command, workingDirectory)
        {
            // Required for various git commands such as diff to prevent a warning.
            StartInfo.EnvironmentVariables.Add("TERM", "msys");
        }

        /// <summary>
        /// Gets GitBash output - which is often sent to Error.
        /// </summary>
        public new string Output
        {
            get
            {
                return base.Output + this.Error;
            }
        }
    }
}
