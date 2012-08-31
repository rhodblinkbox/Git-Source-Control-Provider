// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicSccProvider.bb.cs" company="blinkbox">
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
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Linq;

    using GitScc.Blinkbox;
    using GitScc.Blinkbox.Data;
    using GitScc.Blinkbox.Options;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;

    using MsVsShell = Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Blinkbox implementation inheriting from GitSourceControlProvider. 
    /// BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes. 
    /// </summary>
    public partial class BasicSccProvider
    {
        /// <summary>
        /// Instance of the  <see cref="NotificationService"/>
        /// </summary>
        private NotificationService notificationService;

        /// <summary>
        /// List of git tfs commands available
        /// </summary>
        private List<GitTfsCommand> gitTfsCommands;

        /// <summary>
        /// Instance of the  <see cref="SccHelperService"/>
        /// </summary>
        private SccHelperService sccHelperService;

        /// <summary>
        /// Registers a service.
        /// </summary>
        /// <typeparam name="T">The thye of the service to register.</typeparam>
        /// <param name="instance">The instance of the service.</param>
        public static void RegisterService<T>(T instance)
        {
            if (_SccProvider == null)
            {
                throw new Exception("no instance of BasicSccProvider found");
            }

            ((IServiceContainer)_SccProvider).AddService(typeof(T), instance, false);
        }

        /// <summary>
        /// Determines whether the solution is git TFS controlled.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the solution is git TFS controlled.
        /// </returns>
        public bool IsSolutionGitTfsControlled()
        {
            if (this.sccService.Active && this.sccService.SolutionOpen)
            {
                var repositoryDirectory = GitFileStatusTracker.GetRepositoryDirectory(this.sccService.GetSolutionDirectory());
                if (!string.IsNullOrEmpty(repositoryDirectory))
                {
                    var expectedGitTfsDirectory = repositoryDirectory + "\\.git\\tfs";
                    return Directory.Exists(expectedGitTfsDirectory);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the service of type T.
        /// </summary>
        /// <typeparam name="T">The type of a service</typeparam>
        /// <returns>service of type T.</returns>
        public T GetService<T>()
        {
            return (T)base.GetService(typeof(T));
        }

        /// <summary>
        /// Handles a refresh button click.
        /// </summary>
        private void HandleRefreshButton()
        {
            var pendingChangesWindow = this.GetService<PendingChangesView>();
            if (pendingChangesWindow != null)
            {
                pendingChangesWindow.CancelReview();
            }
        }

        /// <summary>
        /// Initialises the blinkbox extensions to BasicSccProvider.
        /// </summary>
        private void InitialiseBlinkboxExtensions()
        {
            this.notificationService = new NotificationService();
            this.sccHelperService = new SccHelperService(this.sccService);

            // register services required elsewhere.
            RegisterService(this.notificationService);
            RegisterService(this.sccHelperService);
        }

        /// <summary>
        /// Registers commands and services used by the extension.
        /// </summary>
        /// <param name="menuService">The menu service.</param>
        private void RegisterComponents(MsVsShell.OleMenuCommandService menuService)
        {

            this.gitTfsCommands = this.DefineGitTfsCommands();

            if (menuService != null)
            {
                // Register Git Tfs commands with menu service
                foreach (var menuOption in this.gitTfsCommands)
                {
                    var currentMenuOption = menuOption;
                    Action handler = () => currentMenuOption.Handler();
                    this.RegisterCommandWithMenuService(menuService, menuOption.CommandId, (sender, args) => handler());
                }

                // Commit and test button
                this.RegisterCommandWithMenuService(menuService, CommandId.BlinkboxDeployId, (sender, args) => this.ReDeploy());
            }
        }

        /// <summary>
        /// Defines the git TFS commands availalble.
        /// </summary>
        /// <returns>A list of commands</returns>
        private List<GitTfsCommand> DefineGitTfsCommands()
        {
            var commands = new List<GitTfsCommand>();

            commands.Add(new GitTfsCommand
            {
                Name = "Review",
                CommandId = CommandId.GitTfsReviewButtonId,
                Handler = () => SccHelperService.RunAsync(() => new DevelopmentProcess().Review())
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Check in",
                CommandId = CommandId.GitTfsCheckinButtonId,
                Handler = () => SccHelperService.RunAsync(() => new DevelopmentProcess().Checkin())
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Get Latest",
                CommandId = CommandId.GitTfsGetLatestButtonId,
                Handler = () => SccHelperService.RunAsync(() => new DevelopmentProcess().GetLatest())
            });

            commands.Add(new GitTfsCommand
            {
                Name = "Clean Workspace",
                CommandId = CommandId.GitTfsCleanWorkspacesButtonId,
                Handler = () => SccHelperService.RunGitTfs("cleanup-workspaces")
            });

            return commands;
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
                case CommandId.BlinkboxDeployId:
                    if (GitBash.Exists && this.DeployProjectAvailable())
                    {
                        commandFlags |= OLECMDF.OLECMDF_ENABLED;
                    }
                    break;
                break;

                case CommandId.GitTfsCheckinButtonId:
                case CommandId.GitTfsGetLatestButtonId:
                case CommandId.GitTfsCleanWorkspacesButtonId:
                case CommandId.GitTfsReviewButtonId:
                case CommandId.ToolsMenu:
                case CommandId.ToolsMenuGroup:
                    // Disable controls if git-tfs is not found. 
                    if (this.IsSolutionGitTfsControlled() && this.sccService.IsSolutionGitControlled)
                    {
                        commandFlags |= OLECMDF.OLECMDF_ENABLED;
                    }
                    else
                    {
                        commandFlags &= ~OLECMDF.OLECMDF_ENABLED;
                    }

                    var menuOption = this.gitTfsCommands.FirstOrDefault(x => x.CommandId == commands[0].cmdID);
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
        /// Checks whether a deploy project is available.
        /// </summary>
        /// <returns>true if the solution has a deploy project.</returns>
        private bool DeployProjectAvailable()
        {
            if (this.sccService.Active && this.sccService.SolutionOpen)
            {
                var solutionDir = this.sccService.GetSolutionDirectory();
                return File.Exists(solutionDir + "\\" + BlinkboxSccOptions.Current.PostCommitDeployProjectName);
            }

            return false;
        }

        /// <summary>
        /// Handles the Deploy button. 
        /// </summary>
        private void ReDeploy()
        {
            try
            {
                // Run the following action asynchronously
                Action action = () =>
                    {
                        var commit = new CommitData 
                        {
                            Hash = sccHelperService.GetHeadRevisionHash(),
                            Message = sccHelperService.GetLastCommitMessage() + " Re-deploy"
                        };
                        new Deployment(this).RunDeploy(commit);
                    };

                this.notificationService.ClearMessages();
                new System.Threading.Tasks.Task(action).Start();
            }
            catch (Exception e)
            {
                NotificationService.DisplayException(e, "Deploy failed");
            }
        }
    }
}