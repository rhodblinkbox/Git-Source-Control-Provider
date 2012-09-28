// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeploymentService.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Text.RegularExpressions;
    using System.Web;

    using GitScc.Blinkbox.Data;
    using GitScc.Blinkbox.Options;
    using GitScc.Blinkbox.Powershell;

    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Framework;

    /// <summary>
    /// Performs deployments 
    /// </summary>
    public class DeploymentService : IDisposable
    {
        /// <summary>
        /// The current instance of the <see cref="SccProviderService"/>
        /// </summary>
        private readonly SccProviderService sccProviderService;

        /// <summary>
        /// Instance of the  <see cref="NotificationService"/>
        /// </summary>
        private readonly NotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentService"/> class.
        /// </summary>
        /// <param name="basicSccProvider">The basic SCC provider.</param>
        public DeploymentService(BasicSccProvider basicSccProvider)
        {
            this.sccProviderService = basicSccProvider.GetService<SccProviderService>();
            this.notificationService = basicSccProvider.GetService<NotificationService>();
        }

        /// <summary>
        /// Deploys using the deploy project specified in settings.
        /// </summary>
        /// <param name="deployment">The deployment.</param>
        /// <returns>true if the deploy was successful.</returns>
        public bool RunDeploy(Deployment deployment)
        {
            bool success;

            if (Path.GetExtension(SolutionSettings.Current.DeployProjectLocation) == ".ps1")
            {
                // Call a powershell script to run the deployment.
                var scriptName = SccHelperService.GetAbsolutePath(SolutionSettings.Current.DeployProjectLocation);

                var powershellArgs = new Dictionary<string, object>
                    {
                        { "buildProjectPath", this.sccProviderService.GetSolutionFileName() },
                        { "buildLabel", deployment.BuildLabel },
                        { "branchName", SolutionSettings.Current.CurrentBranch },
                        { "release", SolutionSettings.Current.CurrentRelease },
                    };

                var result = this.RunPowershell(scriptName, powershellArgs);
                success = true;
            }
            else
            {
                success = this.DeployMsBuild(deployment);
            }

            if (success)
            {
                // Save the deployment in userSettings. 
                var replacements = new Dictionary<string, string> 
                {
                    { "MachineName", Environment.MachineName },
                    { "BuildLabel", deployment.BuildLabel },
                    { "Tags", SolutionUserSettings.Current.TestSwarmTags },
                    { "RunnerMode", SolutionSettings.Current.TestRunnerMode },
                };
                deployment.AppUrl = SolutionUserSettings.Current.LocalAppUrlTemplate;
                deployment.TestRunUrl = SolutionUserSettings.Current.LocalTestUrlTemplate;

                foreach (var replacement in replacements)
                {
                    deployment.AppUrl = deployment.AppUrl.Replace("{" + replacement.Key + "}", replacement.Value);
                    deployment.TestRunUrl = deployment.TestRunUrl.Replace("{" + replacement.Key + "}", replacement.Value);
                }

                SolutionUserSettings.Current.LastDeployment = deployment;
                SolutionUserSettings.Current.Save();

                if (SolutionUserSettings.Current.SubmitTestsOnDeploy.GetValueOrDefault())
                {
                    // Submit tests to testswarm
                    this.SubmitTests();
                }
            }

            return success;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Submits a test run to testswarm.
        /// </summary>
        public void SubmitTests()
        {
            this.notificationService.ClearMessages();

            var scriptName = SccHelperService.GetAbsolutePath(SolutionSettings.Current.TestSubmissionScript);
            var featureDirectory = SccHelperService.GetAbsolutePath(SolutionSettings.Current.FeaturePath);

            var lastDeployment = SolutionUserSettings.Current.LastDeployment;
            if (lastDeployment == null || string.IsNullOrEmpty(lastDeployment.BuildLabel))
            {
                NotificationService.DisplayError("cannot find previous deployment", "cannot find the previous deployment - please re-deploy first");
                return;
            }

            var args = new Dictionary<string, object>
                {
                    { "version", lastDeployment.BuildLabel },
                    { "featuresDirectory", featureDirectory },
                    { "branch", SolutionSettings.Current.CurrentBranch },
                    { "userName", SolutionUserSettings.Current.TestSwarmUsername },
                    { "password", SolutionUserSettings.Current.TestSwarmPassword },
                    { "appUrl", lastDeployment.AppUrl },
                    { "tag", SolutionUserSettings.Current.TestSwarmTags },
                    { "jobName", lastDeployment.Message + " (" + lastDeployment.BuildLabel + ")" }
                };

            var results = this.RunPowershell(scriptName, args);
            var jobId = results["JobId"].ToString();
        }

        /// <summary>
        /// Deploys using the deploy project specified in settings.
        /// </summary>
        /// <param name="deployment">The deployment.</param>
        /// <returns>true if the deploy was successful.</returns>
        private bool DeployMsBuild(Deployment deployment)
        {
            this.notificationService.AddMessage("Begin build and deploy to " + deployment.BuildLabel);

            // Look for a deploy project
            var buildProjectFileName = SccHelperService.GetAbsolutePath(SolutionSettings.Current.DeployProjectLocation);

            if (!File.Exists(buildProjectFileName))
            {
                NotificationService.DisplayError("Deploy abandoned", "build project not found");
                return false;
            }

            this.notificationService.AddMessage("Deploy project found at " + buildProjectFileName);

            // Initisalise our own project collection which can be cleaned up after the build. This is to prevent caching of the project. 
            using (var projectCollection = new ProjectCollection(Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.ToolsetLocations))
            {
                var commitComment = Regex.Replace(deployment.Message, @"\r|\n|\t", string.Empty);
                commitComment = HttpUtility.UrlEncode(commitComment.Substring(0, commitComment.Length > 80 ? 80 : commitComment.Length));

                // Global properties need to be set before the projects are instantiated. 
                var globalProperties = new Dictionary<string, string>
                    {
                        { BlinkboxSccOptions.Current.CommitGuidPropertyName, deployment.BuildLabel }, 
                        { BlinkboxSccOptions.Current.CommitCommentPropertyName, commitComment }
                    };
                var msbuildProject = new ProjectInstance(buildProjectFileName, globalProperties, "4.0", projectCollection);

                // Build it
                this.notificationService.AddMessage("Building " + Path.GetFileNameWithoutExtension(msbuildProject.FullPath));
                var buildRequest = new BuildRequestData(msbuildProject, new string[] { });

                var buildParams = new BuildParameters(projectCollection);
                buildParams.Loggers = new List<ILogger> { new BuildNotificationLogger(this.notificationService) { Verbosity = LoggerVerbosity.Minimal } };

                var result = BuildManager.DefaultBuildManager.Build(buildParams, buildRequest);

                if (result.OverallResult == BuildResultCode.Failure)
                {
                    string message = result.Exception == null
                        ? "An error occurred during build; please see the pending changes window for details."
                        : result.Exception.Message;

                    NotificationService.DisplayError("Build failed", message);
                    return false;
                }

                var launchUrls = msbuildProject.Items.Where(pii => pii.ItemType == BlinkboxSccOptions.Current.UrlToLaunchPropertyName);

                if (UserSettings.Current.OpenUrlsAfterDeploy.GetValueOrDefault())
                {
                    // Launch urls in browser
                    this.notificationService.AddMessage("Launch urls...");
                    foreach (var launchItem in launchUrls)
                    {
                        BasicSccProvider.LaunchBrowser(launchItem.EvaluatedInclude);
                    }
                }

                // Clean up project to prevent caching.
                projectCollection.UnloadAllProjects();
                projectCollection.UnregisterAllLoggers();
            }

            return true;
        }

        private Hashtable RunPowershell(string script, IDictionary<string, object> parameters)
        {
            var host = new PowershellHost();
            try
            {
                using (var runSpace = RunspaceFactory.CreateRunspace(host))
                using (var powerShell = System.Management.Automation.PowerShell.Create())
                {
                    // Open the runspace.
                    runSpace.Open();
                    powerShell.Runspace = runSpace;

                    powerShell.AddCommand(script, false);
                    foreach (var parameter in parameters)
                    {
                        powerShell.AddParameter(parameter.Key, parameter.Value);
                    }

                    var outputs = powerShell.Invoke();
                    return outputs.Reverse()
                        .Where(x => x != null && x.ImmediateBaseObject is Hashtable)
                        .Select(x => x.ImmediateBaseObject as Hashtable)
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}