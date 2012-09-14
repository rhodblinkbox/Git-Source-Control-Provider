// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeploymentService.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using GitScc.Blinkbox.Data;
    using GitScc.Blinkbox.Options;

    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Framework;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Performs deployments 
    /// </summary>
    public class Deployment
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
        /// Initializes a new instance of the <see cref="Deployment"/> class.
        /// </summary>
        /// <param name="basicSccProvider">The basic SCC provider.</param>
        public Deployment(BasicSccProvider basicSccProvider)
        {
            this.sccProviderService = basicSccProvider.GetService<SccProviderService>();
            this.notificationService = basicSccProvider.GetService<NotificationService>();
        }

        /// <summary>
        /// Deploys using the deploy project specified in settings.
        /// </summary>
        /// <param name="commit">
        /// The commit. Supplied if called after a successful commit, otherwise a new instance is created. 
        /// </param>
        /// <returns>
        /// true if the deploy was successful.
        /// </returns>
        public bool RunDeploy(CommitData commit)
        {
            this.notificationService.AddMessage("Begin build and deploy to " + commit.Hash);

            // Look for a deploy project
            var buildProjectFileName = this.sccProviderService.GetSolutionDirectory() + "\\" + BlinkboxSccOptions.Current.PostCommitDeployProjectName;
            if (!File.Exists(buildProjectFileName))
            {
                NotificationService.DisplayError("Deploy abandoned", "build project not found");
                return false;
            }

            this.notificationService.AddMessage("Deploy project found at " + buildProjectFileName);

            // Initisalise our own project collection which can be cleaned up after the build. This is to prevent caching of the project. 
            using (var projectCollection = new ProjectCollection(Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.ToolsetLocations))
            {
                var commitComment = Regex.Replace(commit.Message, @"\r|\n|\t", string.Empty);
                commitComment = HttpUtility.UrlEncode(commitComment.Substring(0, commitComment.Length > 80 ? 80 : commitComment.Length));

                // Global properties need to be set before the projects are instantiated. 
                var globalProperties = new Dictionary<string, string>
                    {
                        { BlinkboxSccOptions.Current.CommitGuidPropertyName, commit.Hash }, 
                        { BlinkboxSccOptions.Current.CommitCommentPropertyName, commitComment },
                        { "TestSwarmUsername", SolutionSettings.Current.TestSwarmUsername },
                        { "TestSwarmPassword", SolutionSettings.Current.TestSwarmPassword },
                        { "TestSwarmTags", SolutionSettings.Current.TestSwarmTags }
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

                // Launch urls in browser
                this.notificationService.AddMessage("Launch urls...");
                var launchUrls = msbuildProject.Items.Where(pii => pii.ItemType == BlinkboxSccOptions.Current.UrlToLaunchPropertyName);
                foreach (var launchItem in launchUrls)
                {
                    this.LaunchBrowser(launchItem.EvaluatedInclude);
                }

                // Clean up project to prevent caching.
                projectCollection.UnloadAllProjects();
                projectCollection.UnregisterAllLoggers();
            }

            return true;
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
                        var browserService = BasicSccProvider.GetServiceEx<SVsWebBrowsingService>() as IVsWebBrowsingService;
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

                NotificationService.DisplayError("Browser failed", "Cannot launch " + url + ": " + errorMessage);
            }
        }
    }
}