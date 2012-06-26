// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlinkboxScc.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   Blinkbox implementation inheriting from GitSourceControlProvider.
//   BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Windows;

    using GitScc.Blinkbox.Events;
    using GitScc.Blinkbox.Options;

    using Microsoft.Build.Execution;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using MsVsShell = Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Blinkbox implementation inheriting from GitSourceControlProvider. 
    /// BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes. 
    /// </summary>
    public class BlinkboxScc
    {
        /// <summary>
        /// Keeps a referece to the sccService.
        /// </summary>
        private readonly SccProviderService sccService = null;

        /// <summary>
        /// reference to the BasicSccProvider package
        /// </summary>
        private readonly BasicSccProvider basicSccProvider = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlinkboxScc"/> class.
        /// </summary>
        /// <param name="sccProviderService">The SCC service.</param>
        /// <param name="basicSccProvider">The basic SCC provider.</param>
        public BlinkboxScc(SccProviderService sccProviderService, BasicSccProvider basicSccProvider)
        {
            // Setup Hooks
            BlinkboxSccHooks.QueryCommandStatus = this.QueryCommandStatus;
            BlinkboxSccHooks.OnRegisterCommands += (sender, args) => this.RegisterComponents(args.MenuService);
            this.sccService = sccProviderService;
            this.basicSccProvider = basicSccProvider;
        }

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
        /// Gets a service of type T.
        /// </summary>
        /// <typeparam name="T">the type of service.</typeparam>
        /// <returns>the required service.</returns>
        private static T GetService<T>()
        {
            return BasicSccProvider.GetServiceEx<T>();
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
                foreach (var menuOption in GitTfs.MenuOptions)
                {
                    var commandText = menuOption.CommandText;
                    Action handler = () => GitTfs.RunGitTfsCommand(commandText, this.GetSolutionDirectory());
                    RegisterCommandWithMenuService(menuService, menuOption.CommandId, (sender, args) => handler());
                }

                // Commit and test button
                RegisterCommandWithMenuService(menuService, Blinkbox.CommandIds.BlinkboxCommitAndTestId, this.OnCommitAndDeploy);
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
        private int QueryCommandStatus(Guid commandGroupGuid, OLECMD[] commands, OLECMDF commandFlags, IntPtr commandText)
        {
            // Process Blinkbox Commands
            switch (commands[0].cmdID)
            {
                case Blinkbox.CommandIds.BlinkboxCommitAndTestId:
                    if (GitBash.Exists && this.sccService.IsSolutionGitControlled && this.CommitAndDeployAvailable())
                    {
                        commandFlags |= OLECMDF.OLECMDF_ENABLED;
                    }
                break;

                case Blinkbox.CommandIds.GitTfsCheckinButtonId:
                case Blinkbox.CommandIds.GitTfsGetLatestButtonId:
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

                    var menuOption = GitTfs.MenuOptions.FirstOrDefault(x => x.CommandId == commands[0].cmdID);
                    if (menuOption != null)
                    {
                        // If its a menu option set the text. 
                        this.basicSccProvider.SetOleCmdText(commandText, menuOption.Name);
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
        /// Checks whether a postCommitDeploy project is available.
        /// </summary>
        /// <returns>true if the solution has devDeployable projects.</returns>
        private bool CommitAndDeployAvailable()
        {
            var solutionDir = this.GetSolutionDirectory();
            return File.Exists(solutionDir + "\\" + BlinkboxSccOptions.Current.PostCommitDeployProjectName);
        }

        /// <summary>
        /// Gets the tool window pane.
        /// </summary>
        /// <typeparam name="T">the type of pane.</typeparam>
        /// <returns>the tool window pane.</returns>
        private T GetToolWindowPane<T>() where T : ToolWindowPane
        {
            return (T)this.basicSccProvider.FindToolWindow(typeof(T), 0, true);
        }

        /// <summary>
        /// Handles the "Commit and Deploy button". Builds and deploys the projects listed in the solution file under DevBuildProjectNames. 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnCommitAndDeploy(object sender, EventArgs e)
        {
            // Subscribe to successful commit event
            BlinkboxSccHooks.OnCommit += this.DeploySuccessfulCommit;

            // Commit to git repository
            this.GetToolWindowPane<PendingChangesToolWindow>().OnCommitCommand();
        }

        /// <summary>
        /// Deploys dev websites on a successful commit
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="commit">
        /// The commit.
        /// </param>
        private void DeploySuccessfulCommit(object sender, OnCommitArgs commit)
        {
            if (commit.Success)
            {
                commit.Hash = this.GetLatestCommitHash();

                try
                {
                    // Look for a deploy project
                    var buildProjectFileName = this.GetSolutionDirectory() + "\\" + BlinkboxSccOptions.Current.PostCommitDeployProjectName;
                    if (!File.Exists(buildProjectFileName))
                    {
                        MessageBox.Show("build project not found", "Deploy abandoned", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var projectCollection = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection;
                    var commitComment = Regex.Replace(commit.Message, @"\r|\n|\t", string.Empty);
                    commitComment = HttpUtility.UrlEncode(commitComment.Substring(0, commitComment.Length > 80 ? 80 : commitComment.Length));

                    // Global properties need to be set before the projects are instantiated. 
                    var globalProperties = new Dictionary<string, string>
                        {
                            { BlinkboxSccOptions.Current.CommitGuidPropertyName, commit.Hash },
                            { BlinkboxSccOptions.Current.CommitCommentPropertyName, commitComment }
                        };
                    var msbuildProject = new ProjectInstance(buildProjectFileName, globalProperties, "4.0");

                    // Build it
                    WriteToStatusBar("Build " + Path.GetFileNameWithoutExtension(msbuildProject.FullPath));
                    var buildRequest = new BuildRequestData(msbuildProject, new string[] { });
                    var result = BuildManager.DefaultBuildManager.Build(new BuildParameters(projectCollection), buildRequest);

                    if (result.OverallResult == BuildResultCode.Failure)
                    {
                        string message = result.Exception == null ? "Unknown error" : result.Exception.Message;
                        MessageBox.Show(message, "Build failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Get variables required for IIS admin from the first project.
                    string launchUrl = msbuildProject.GetPropertyValue(BlinkboxSccOptions.Current.UrlToLaunchPropertyName);

                    // Launch url in browser
                    this.LaunchBrowser(launchUrl);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Build failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
         
            // Unsubscribe from successful commit event
            BlinkboxSccHooks.OnCommit -= this.DeploySuccessfulCommit;
        }

        /// <summary>
        /// Gets the hash of the latest commit
        /// </summary>
        /// <returns>
        /// The get latest commit hash.
        /// </returns>
        private string GetLatestCommitHash()
        {
            var commitHash = GitBash.Run("rev-parse HEAD", this.sccService.CurrentTracker.GitWorkingDirectory).Replace("\n", string.Empty); // Git bash adds a return on the end
            return commitHash;
        }

        /// <summary>
        /// Launches the provided url in the Visual Studio browser.
        /// </summary>
        /// <param name="url">The URL.</param>
        private void LaunchBrowser(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string errorMessage;
                try
                {
                    if (Blinkbox.Options.BlinkboxSccOptions.Current.LaunchDeployedUrlsInVS)
                    {
                        // launch in visual studio browser
                        var browserService = GetService<SVsWebBrowsingService>() as IVsWebBrowsingService;
                        if (browserService != null)
                        {
                            IVsWindowFrame frame;

                            // passing 0 to the NavigateFlags allows the browser service to reuse open instances of the internal browser.
                            browserService.Navigate(url, 0, out frame);
                        }
                    }
                    else
                    {
                        // Launch in default browser
                        System.Diagnostics.Process.Start(url);
                    }

                    return;
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }

                MessageBox.Show("Cannot launch " + url + ": " + errorMessage, "Browser failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the solution directory.
        /// </summary>
        /// <returns>the path to the solution.</returns>
        private string GetSolutionDirectory()
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
        private bool IsSolutionGitTfsControlled()
        {
            var solutionDirectory = this.GetSolutionDirectory();
            if (!string.IsNullOrEmpty(solutionDirectory))
            {
                var expectedGitTfsDirectory = solutionDirectory + "\\.git\\tfs";
                return Directory.Exists(expectedGitTfsDirectory);
            }

            return false;
        }
    }
}