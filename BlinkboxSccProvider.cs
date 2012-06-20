
namespace GitScc
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Windows;

    using EnvDTE;

    using Microsoft.Build.Execution;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    using MsVsShell = Microsoft.VisualStudio.Shell;
    using Process = System.Diagnostics.Process;

    /// <summary>
    /// Blinkbox implementation inheriting from GitSourceControlProvider. 
    /// BasicSccProvider has been modified as little as possible, so thats its easier to merge in 3rd party changes. 
    /// </summary>
    // See BasicSccProvider class for help with these attributes. 
    [MsVsShell.ProvideLoadKey("Standard", "0.1", "Git Source Control Provider", "Yiyisun@hotmail.com", 15261)]
    [MsVsShell.DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\10.0Exp")]
    [MsVsShell.InstalledProductRegistration(false, "#100", "#101", "1.0.0.0", IconResourceID = CommandId.iiconProductIcon)]
    [MsVsShell.PackageRegistration(UseManagedResourcesOnly = true)]
    [MsVsShell.ProvideMenuResource(1000, 1)]
    [MsVsShell.ProvideOptionPageAttribute(typeof(SccProviderOptions), "Source Control", "Git Source Control Provider Options", 106, 107, false)]
    [ProvideToolsOptionsPageVisibility("Source Control", "Git Source Control Provider Options", "C4128D99-0000-41D1-A6C3-704E6C1A3DE2")]
    [MsVsShell.ProvideToolWindow(typeof(PendingChangesToolWindow), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom)]
    [MsVsShell.ProvideToolWindowVisibility(typeof(PendingChangesToolWindow), "C4128D99-0000-41D1-A6C3-704E6C1A3DE2")]
    [MsVsShell.ProvideService(typeof(SccProviderService), ServiceName = "Git Source Control Service")]
    [ProvideSourceControlProvider("Git Source Control Provider", "#100")]
    [MsVsShell.ProvideAutoLoad("C4128D99-0000-41D1-A6C3-704E6C1A3DE2")]
    [Guid("C4128D99-2000-41D1-A6C3-704E6C1A3DE2")]
    public class BlinkboxSccProvider : BasicSccProvider, IOleCommandTarget
    {
        /// <summary>
        /// Gets the blinkbox SCC provider service.
        /// </summary>
        /// <value>The blinkbox SCC provider service.</value>
        protected BlinkboxSccProviderService BlinkboxSccProviderService
        {
            get
            {
                return (BlinkboxSccProviderService)this.sccService;
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            RegisterComponents();
        }

        /// <summary>
        /// Registers commands and services used by the extension.
        /// </summary>
        protected override void RegisterComponents()
        {
            projects = new List<GitFileStatusTracker>();
            sccService = new BlinkboxSccProviderService(this, projects);

            ((IServiceContainer)this).AddService(typeof(BlinkboxSccProviderService), sccService, true);

            base.RegisterComponents();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as MsVsShell.OleMenuCommandService;
            if (mcs != null)
            {
                CommandID cmd;

                // Register Git Tfs commands with menu service
                for (int i = 0; i < GitToolCommands.GitTfsCommands.Count; i++)
                {
                    RegisterCommandWithMenuService(mcs, CommandId.icmdGitTfsCommandBase + i, this.OnGitTfsCommandExec);
                }

                // Commit and test button
                RegisterCommandWithMenuService(mcs, CommandId.icmdBlinkboxCommitAndTest, this.OnBlinkboxCommitAndTest);

                // Checkin Button.
                RegisterCommandWithMenuService(mcs, CommandId.cmdidGitTfsCheckinButton, this.Checkin);

                // Get Latest Button.
                RegisterCommandWithMenuService(mcs, CommandId.cmdidGitTfsGetLatestButton, this.GetLatest);
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
        /// <param name="guidCmdGroup">The Command group Guid.</param>
        /// <param name="cCmds">The c CMDS.</param>
        /// <param name="prgCmds">A list of commands currently being queried.</param>
        /// <param name="pCmdText">the command text.</param>
        /// <returns>integer indicating whether the command is supported.</returns>
        int IOleCommandTarget.QueryStatus(ref Guid guidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            Debug.Assert(cCmds == 1, "Multiple commands");
            Debug.Assert(prgCmds != null, "NULL argument");

            if (prgCmds == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            // Filter out commands that are not defined by this package
            if (guidCmdGroup != GuidList.guidSccProviderCmdSet)
            {
                return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            }

            OLECMDF cmdf = OLECMDF.OLECMDF_SUPPORTED;

            // All source control commands needs to be hidden and disabled when the provider is not active
            if (!sccService.Active)
            {
                cmdf = cmdf | OLECMDF.OLECMDF_INVISIBLE;
                cmdf = cmdf & ~(OLECMDF.OLECMDF_ENABLED);

                prgCmds[0].cmdf = (uint)cmdf;
                return VSConstants.S_OK;
            }

            // Process Blinkbox Commands
            switch (prgCmds[0].cmdID)
            {
                case CommandId.icmdBlinkboxCommitAndTest:
                    if (GitBash.Exists && this.sccService.IsSolutionGitControlled && this.BlinkboxSccProviderService.SolutionIsDevDeployable())
                    {
                        cmdf |= OLECMDF.OLECMDF_ENABLED;
                    }
                break;

                case CommandId.cmdidGitTfsCheckinButton:
                case CommandId.cmdidGitTfsGetLatestButton:
                case CommandId.GitTfsMenu:
                    // Disable controls if git-tfs is not found. 
                    if (!string.IsNullOrEmpty(GitSccOptions.Current.GitTfsPath) && sccService.IsSolutionGitControlled)
                    {
                        cmdf |= OLECMDF.OLECMDF_ENABLED;
                    }
                    else
                    {
                        cmdf &= ~OLECMDF.OLECMDF_ENABLED;
                    }
                    break;

                default:

                    // Git Tfs commands in pending window
                    if (prgCmds[0].cmdID >= CommandId.icmdGitTfsCommandBase && prgCmds[0].cmdID < CommandId.icmdGitTfsCommandBase + GitToolCommands.GitTfsCommands.Count
                        && !string.IsNullOrEmpty(GitSccOptions.Current.GitTfsPath))
                    {
                        int idx = (int)prgCmds[0].cmdID - CommandId.icmdGitTfsCommandBase;
                        SetOleCmdText(pCmdText, GitToolCommands.GitTfsCommands[idx].Name);
                        cmdf |= OLECMDF.OLECMDF_ENABLED;
                        break;
                    }

                    // Command ID is not a blinkbox command - process Git Scc commands in the base class. 
                    return QueryStatus(prgCmds, pCmdText, cmdf);
            }

            prgCmds[0].cmdf = (uint)cmdf;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called when a button on the Git Tfs menu is clicked.
        /// </summary>
        /// <param name="sender">The menu command.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnGitTfsCommandExec(object sender, EventArgs e)
        {
            var menuCommand = sender as MenuCommand;
            if (null != menuCommand)
            {
                int idx = menuCommand.CommandID.ID - CommandId.icmdGitTfsCommandBase;
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Run GitTfs Command {0}", GitToolCommands.GitTfsCommands[idx].Command));
                var cmd = GitToolCommands.GitTfsCommands[idx];
                this.GitTfsCommand(cmd.Command);
            }
        }

        /// <summary>
        /// Handles the "Commit and Test button". Builds and deploys the projects listed in the solution file under DevBuildProjectNames. 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnBlinkboxCommitAndTest(object sender, EventArgs e)
        {
            // Commit to git repository
            var commit = GetToolWindowPane<PendingChangesToolWindow>().OnCommitCommand();

            if (!string.IsNullOrEmpty(commit.Id))
            {
                try
                {
                    // Look for a deploy project
                    var dte = (EnvDTE.DTE)this.GetService(typeof(SDTE));

                    if (dte.Solution.Globals["DevBuildProjectNames"] == null)
                    {
                        MessageBox.Show("No deploy projects listed in solution file", "No deploy projects found", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var devBuildProjectNames = dte.Solution.Globals["DevBuildProjectNames"].ToString().Split(",".ToCharArray()).ToList();
                    var msbuildProjects = new List<Tuple<int, ProjectInstance>>();
                    var projectCollection = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection;
                    var launchUrls = new List<string>();

                    // Global properties need to be set before the projects are instantiated. 
                    var globalProperties = new Dictionary<string, string>()
                        {
                            { "BuildGuid", "-" + commit.Id }, 
                            { "DevDeployment", "true" }, 
                            { "BuildComment", HttpUtility.UrlEncode(commit.Message.Substring(0, commit.Message.Length > 80 ? 80 : commit.Message.Length)) }
                        };

                    // Get the projects listed in the solution file DevBuildProjectNames keeping the order
                    foreach (EnvDTE.Project pr in dte.Solution.Projects)
                    {
                        if (devBuildProjectNames.Contains(pr.Name))
                        {
                            msbuildProjects.Add(new Tuple<int, ProjectInstance>(devBuildProjectNames.IndexOf(pr.Name), new ProjectInstance(pr.FileName, globalProperties, "4.0")));
                            if (msbuildProjects.Count == devBuildProjectNames.Count)
                            {
                                break;
                            }
                        }
                    }

                    if (msbuildProjects.Count == 0)
                    {
                        MessageBox.Show("Deploy project not found", "Deploy abandoned", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // build the projects in order
                    foreach (var msbuildProject in msbuildProjects.OrderBy(x => x.Item1).Select(x => x.Item2))
                    {
                        // Build it
                        WriteToStatusBar("Build " + Path.GetFileNameWithoutExtension(msbuildProject.FullPath));
                        var buildRequest = new BuildRequestData(msbuildProject, new string[] { "Build" });
                        var result = BuildManager.DefaultBuildManager.Build(new BuildParameters(projectCollection), buildRequest);

                        if (result.OverallResult == BuildResultCode.Failure)
                        {
                            string message = result.Exception == null ? "Unknown error" : result.Exception.Message;
                            MessageBox.Show(message, "Build failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Get variables required for IIS admin from the first project.
                        launchUrls.Add(msbuildProject.GetPropertyValue("LaunchUrlAfterBuild"));
                    }

                    // open website
                    this.LaunchBrowser(launchUrls.FirstOrDefault());
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Build failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Launches the provided url in the Visual Studio browser.
        /// </summary>
        /// <param name="url">The URL.</param>
        private void LaunchBrowser(string url = null)
        {
            string errorMessage = null;
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var browserService = this.GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;
                    if (browserService != null)
                    {
                        IVsWindowFrame frame;

                        // passing 0 to the NavigateFlags allows the browser service to reuse open instances of the internal browser.
                        browserService.Navigate(url, 0, out frame);
                        return;
                    }
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                }

                MessageBox.Show("Cannot launch " + url + ": " + errorMessage, "Browser failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Call git tfs checkin.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Checkin(object sender, EventArgs e)
        {
            this.GitTfsCommand("checkintool");
        }

        /// <summary>
        /// Get latest code from tfs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void GetLatest(object sender, EventArgs e)
        {
            this.GitTfsCommand("pull");
        }

        /// <summary>
        /// Runs a command in the git tfs commandline environment.
        /// </summary>
        /// <param name="command">The command.</param>
        private void GitTfsCommand(string command)
        {
            var process = new Process();
            process.StartInfo.FileName = GitSccOptions.Current.GitTfsPath;
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.WorkingDirectory = this.GetSolutionDirectory();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

            // Write output to the output pane. 
            var outputPane = this.GetOutputWindowPane(OutputPanes.GitTfs);
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += (sender, args) => outputPane.OutputString(args.Data + Environment.NewLine);
            process.Start();
            process.BeginOutputReadLine();
        }

        /// <summary>
        /// Gets the specified output window pane, craeeting a new one if it doesnt already exist.
        /// </summary>
        /// <param name="outputPane">The output pane.</param>
        /// <returns>the specified output pane.</returns>
        public IVsOutputWindowPane GetOutputWindowPane(OutputPane outputPane)
        {
            var dte = (DTE)GetService(typeof(DTE));
            var serviceProvider = new ServiceProvider(dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            var outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var generalPaneGuid = outputPane.Id;
            IVsOutputWindowPane pane;
            outputWindow.GetPane(ref generalPaneGuid, out pane);

            if (pane == null)
            {
                outputWindow.CreatePane(ref generalPaneGuid, outputPane.Name, 1, 0);
                outputWindow.GetPane(ref generalPaneGuid, out pane);
                outputPane.Id = generalPaneGuid;
            }
            pane.Activate();
            return pane;
        }

        /// <summary>
        /// Defines a custom output pane.
        /// </summary>
        public struct OutputPane
        {
            /// <summary>
            /// Gets or sets the id.
            /// </summary>
            /// <value>The id.</value>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
        }

        /// <summary>
        /// Defines custom output panes.
        /// </summary>
        public struct OutputPanes
        {
            public static OutputPane IVsTextEditGeneral = new OutputPane() { Id = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid, Name = "General" };
            public static OutputPane GitTfs = new OutputPane() { Id = new Guid("B81AFB34-8E7C-4BAF-A567-9B3216F3B25E"), Name = "Git tfs" };
        }

        /// <summary>
        /// Gets the solution directory.
        /// </summary>
        /// <returns>the path to the solution.</returns>
        private string GetSolutionDirectory()
        {
            var sol = (IVsSolution)this.GetService(typeof(SVsSolution));
            string solutionDirectory, solutionFile, solutionUserOptions;

            if (sol.GetSolutionInfo(out solutionDirectory, out solutionFile, out solutionUserOptions) == VSConstants.S_OK)
            {
                return Path.GetDirectoryName(solutionFile);
            }

            return null;
        }

        /// <summary>
        /// Shows a message in the status bar.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteToStatusBar(string message)
        {
            var dte = GetServiceEx<EnvDTE.DTE>();
            dte.StatusBar.Text = message;
        }
    }
}