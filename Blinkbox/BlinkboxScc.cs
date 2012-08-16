// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlinkboxScc.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   Blinkbox implementation inheriting from GitSourceControlProvider.
//   BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc
{
    using System;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Linq;

    using GitScc.Blinkbox;
    using GitScc.Blinkbox.Options;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell.Interop;

    using MsVsShell = Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Blinkbox implementation inheriting from GitSourceControlProvider. 
    /// BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes. 
    /// </summary>
    public partial class BasicSccProvider
    {
        /// <summary>
        /// Shows a message in the status bar.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteToStatusBar(string message)
        {
            var dte = GetService<EnvDTE.DTE>();
            dte.StatusBar.Text = message;
        }

        /// <summary>
        /// Handles a refresh button click.
        /// </summary>
        public void HandleRefreshButton()
        {
            PendingChangesView.CancelReview();
        }

        /// <summary>
        /// Gets a service of type T.
        /// </summary>
        /// <typeparam name="T">the type of service.</typeparam>
        /// <returns>the required service.</returns>
        private static T GetService<T>()
        {
            return GetServiceEx<T>();
        }

        /// <summary>
        /// Registers commands and services used by the extension.
        /// </summary>
        /// <param name="menuService">The menu service.</param>
        private void RegisterComponents(MsVsShell.OleMenuCommandService menuService)
        {
            if (menuService != null)
            {
                // Register Git Tfs commands with menu service
                foreach (var menuOption in GitTfsMenu.MenuOptions)
                {
                    var currentMenuOption = menuOption;
                    Action handler = () => currentMenuOption.Handler();
                    this.RegisterCommandWithMenuService(menuService, menuOption.CommandId, (sender, args) => handler());
                }

                // Commit and test button
                this.RegisterCommandWithMenuService(menuService, Blinkbox.CommandIds.BlinkboxCommitAndDeployId, this.OnCommitAndDeploy);
                this.RegisterCommandWithMenuService(menuService, Blinkbox.CommandIds.BlinkboxDeployId, this.OnDeploy);
            }
        }

        /// <summary>
        /// Registers the command with menu service.
        /// </summary>
        /// <param name="menuService">The menu service.</param>
        /// <param name="commandId">The command id.</param>
        /// <param name="handler">The handler for the command.</param>
        /// <param name="commandSetGuid">The command set GUID.</param>
        private void RegisterCommandWithMenuService(MsVsShell.OleMenuCommandService menuService, int commandId, EventHandler handler, Guid? commandSetGuid = null)
        {
            commandSetGuid = commandSetGuid ?? GuidList.guidSccProviderCmdSet;
            var command = new CommandID(commandSetGuid.Value, commandId);
            var menuCommand = new MenuCommand(handler, command);
            menuService.AddCommand(menuCommand);
        }

        /// <summary>
        /// QueryStatus called for each command when the extension initialises.
        /// </summary>
        /// <param name="commandGroupGuid">The Command group Guid.</param>
        /// <param name="commands">The commands array. Passing the array is necessary, because otherwise command flags do not apply to the original instance. </param>
        /// <param name="commandFlags">The command flags.</param>
        /// <param name="commandText">the command text.</param>
        /// <returns>
        /// integer indicating whether the command is supported.
        /// </returns>
        private int QueryBlinkboxCommandStatus(Guid commandGroupGuid, OLECMD[] commands, OLECMDF commandFlags, IntPtr commandText)
        {
            // Process Blinkbox Commands
            switch (commands[0].cmdID)
            {
                case Blinkbox.CommandIds.BlinkboxDeployId:
                    if (GitBash.Exists && this.DeployProjectAvailable())
                    {
                        commandFlags |= OLECMDF.OLECMDF_ENABLED;
                    }
                    break;

                case Blinkbox.CommandIds.BlinkboxCommitAndDeployId:
                    if (GitBash.Exists && this.sccService.IsSolutionGitControlled && this.DeployProjectAvailable())
                    {
                        commandFlags |= OLECMDF.OLECMDF_ENABLED;
                    }
                break;

                case Blinkbox.CommandIds.GitTfsCheckinButtonId:
                case Blinkbox.CommandIds.GitTfsGetLatestButtonId:
                case Blinkbox.CommandIds.GitTfsCleanWorkspacesButtonId:
                case Blinkbox.CommandIds.GitTfsReviewButtonId:
                case Blinkbox.CommandIds.GitTfsCompleteReviewButtonId:
                case Blinkbox.CommandIds.GitTfsMenu:
                case Blinkbox.CommandIds.GitTfsMenuGroup:
                    // Disable controls if git-tfs is not found. 
                    if (this.IsSolutionGitTfsControlled() && this.sccService.IsSolutionGitControlled)
                    {
                        commandFlags |= OLECMDF.OLECMDF_ENABLED;
                    }
                    else
                    {
                        commandFlags &= ~OLECMDF.OLECMDF_ENABLED;
                    }

                    var menuOption = GitTfsMenu.MenuOptions.FirstOrDefault(x => x.CommandId == commands[0].cmdID);
                    if (menuOption != null)
                    {
                        // If its a menu option set the text. 
                        this.SetOleCmdText(commandText, menuOption.Name);
                    }

                    break;

                default:
                    // Not one of our commands - return
                    return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            // command handled here
            commands[0].cmdf = (uint)commandFlags;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Handles a click of the commit button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void HandleCommit(object sender, EventArgs e)
        {
            // Call existing implementation
            OnCommitCommand(sender, e);
        }

        /// <summary>
        /// Checks whether a deploy project is available.
        /// </summary>
        /// <returns>true if the solution has a deploy project.</returns>
        private bool DeployProjectAvailable()
        {
            var solutionDir = GetSolutionDirectory();
            return File.Exists(solutionDir + "\\" + BlinkboxSccOptions.Current.PostCommitDeployProjectName);
        }

        /// <summary>
        /// Handles the Deploy button. 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnDeploy(object sender, EventArgs e)
        {
            using (var deploy = new Deploy())
            {
                var commit = new CommitData()
                    {
                        Hash = SourceControlHelper.GetHeadRevisionHash(sccService.CurrentGitWorkingDirectory),
                        Message = "Re-deploy"
                    };

                NotificationWriter.Clear();
                deploy.RunDeploy(commit);
            }

        }

        /// <summary>
        /// Handles the "Commit and Deploy button". Builds and deploys the projects listed in the solution file under DevBuildProjectNames. 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnCommitAndDeploy(object sender, EventArgs e)
        {
            // Commit to git repository
             var commit = this.GetToolWindowPane<PendingChangesToolWindow>().BlinkboxCommit();

            if (commit.Success)
            {
                commit.Hash = SourceControlHelper.GetHeadRevisionHash(sccService.CurrentGitWorkingDirectory);
                NotificationWriter.Clear();
                NotificationWriter.Write("Commit " + commit.Hash + " successful");
                new Deploy().RunDeploy(commit);
            }
        }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns>The working directory</returns>
        public static string GetWorkingDirectory()
        {
            if (_SccProvider == null)
            {
                throw new Exception("Unable to get working directory- _SccProvider is null");
            }
            return _SccProvider.sccService.CurrentGitWorkingDirectory;
        }

        /// <summary>
        /// Gets the current branch.
        /// </summary>
        /// <returns>The branch</returns>
        public static string GetCurrentBranch()
        {
            if (_SccProvider == null)
            {
                throw new Exception("Unable to get current branch - _SccProvider is null");
            }
            return _SccProvider.sccService.CurrentBranchName;
        }


        /// <summary>
        /// Gets the solution directory.
        /// </summary>
        /// <returns>the path to the solution.</returns>
        public static string GetSolutionDirectory()
        {
            var sol = (IVsSolution)GetService<SVsSolution>();
            string solutionDirectory, solutionFile, solutionUserOptions;

            if (sol.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
            {
                return Path.GetDirectoryName(solutionFile);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the solution is git TFS controlled.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the solution is git TFS controlled.
        /// </returns>
        public bool IsSolutionGitTfsControlled()
        {
            var repositoryDirectory = GitFileStatusTracker.GetRepositoryDirectory(GetSolutionDirectory());
            if (!string.IsNullOrEmpty(repositoryDirectory))
            {
                var expectedGitTfsDirectory = repositoryDirectory + "\\.git\\tfs";
                return Directory.Exists(expectedGitTfsDirectory);
            }

            return false;
        }
    }
}