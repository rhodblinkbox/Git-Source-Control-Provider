// -----------------------------------------------------------------------
// <copyright file="GitTfsMenu.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Provides the structure of the git tfs menu.
    /// </summary>
    public class GitTfsMenu
    {
        /// <summary>
        /// Gets the menu options.
        /// </summary>
        /// <value>The menu options.</value>
        public static readonly List<GitTfsCommand> MenuOptions = new List<GitTfsCommand>();

        /// <summary>
        /// Initializes static members of the <see cref="GitTfsMenu"/> class.
        /// </summary>
        static GitTfsMenu()
        {
            MenuOptions.Add(new GitTfsCommand()
            {
                Name = "Review",
                CommandId = Blinkbox.CommandIds.GitTfsReviewButtonId,
                Handler = () => RunTaskAsync(DevelopmentProcess.Review)
            });

            MenuOptions.Add(new GitTfsCommand()
            {
                Name = "Complete Review",
                CommandId = Blinkbox.CommandIds.GitTfsCompleteReviewButtonId,
                Handler = () => RunTaskAsync(DevelopmentProcess.CompleteReview)
            });

            MenuOptions.Add(new GitTfsCommand()
            {
                Name = "Check in", 
                CommandId = Blinkbox.CommandIds.GitTfsCheckinButtonId,
                Handler = () => RunTaskAsync(DevelopmentProcess.Checkin)
            });

            MenuOptions.Add(new GitTfsCommand()
            {
                Name = "Get Latest", 
                CommandId = Blinkbox.CommandIds.GitTfsGetLatestButtonId,
                Handler = () => RunTaskAsync(DevelopmentProcess.GetLatest)
            });

            MenuOptions.Add(new GitTfsCommand()
            {
                Name = "Clean Workspace",
                CommandId = Blinkbox.CommandIds.GitTfsCleanWorkspacesButtonId,
                Handler = () => SourceControlHelper.RunGitTfs("cleanup-workspaces")
            });
        }


        /// <summary>
        /// Runs the a command asyncronously.
        /// </summary>
        /// <param name="action">The action.</param>
        private static void RunTaskAsync(Action action)
        {
            var task = new System.Threading.Tasks.Task(action);
            task.Start();
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
            public Action Handler { get; set; }
        }
    }
}
