// -----------------------------------------------------------------------
// <copyright file="DeploySettings.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.IO;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SolutionSettings : SettingsBase
    {
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static SolutionSettings sccOptions;

        public string TestSwarmUrl { get; set; }
        public string FeaturePath { get; set; }
        public string TestBrowserSets { get; set; }
        public string CurrentBranch { get; set; }
        public string TestRunnerMode { get; set; }
        public string TestSwarmTags { get; set; }
        public string DeployProjectLocation { get; set; }
        public string CurrentRelease { get; set; }
        public string TestSubmissionScript { get; set; }

        /// <summary>
        /// Gets Current.
        /// </summary>
        public static SolutionSettings Current
        {
            get
            {
                if (sccOptions == null)
                {
                    var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
                    var solutionDirectory = sccProvider.GetSolutionDirectory();
                    var solutionFileName = sccProvider.GetSolutionFileName();
                    if (!string.IsNullOrEmpty(solutionFileName))
                    {
                        // Solution is open. 
                        string configFileName = Path.Combine(solutionDirectory, Path.GetFileNameWithoutExtension(sccProvider.GetSolutionFileName()) + "." + Extension);

                        sccOptions = SettingsBase.LoadFromConfig<SolutionSettings>(configFileName);
                    }
                }

                return sccOptions;
            }
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        protected override void Init()
        {
            this.TestSwarmUrl = string.IsNullOrEmpty(this.TestSwarmUrl) ? "http://battleship/testswarm" : this.TestSwarmUrl;
            this.FeaturePath = string.IsNullOrEmpty(this.FeaturePath) ? "test\\AngularClient.Test.Artefacts\\Features" : this.FeaturePath;
            this.TestBrowserSets = string.IsNullOrEmpty(this.TestBrowserSets) ? "default,currentDesktop" : this.TestBrowserSets;
            this.CurrentBranch = string.IsNullOrEmpty(this.CurrentBranch) ? "v0" : this.CurrentBranch;
            this.TestRunnerMode = string.IsNullOrEmpty(this.TestRunnerMode) ? "appfirst" : this.TestRunnerMode;
            this.TestSwarmTags = string.IsNullOrEmpty(this.TestSwarmTags) ? "devcomplete" : this.TestSwarmTags;
            this.DeployProjectLocation = string.IsNullOrEmpty(this.DeployProjectLocation) ? "deploy\\deployLocal.ps1" : this.DeployProjectLocation;
            this.CurrentRelease = string.IsNullOrEmpty(this.CurrentRelease) ? "v0.0.0" : this.CurrentRelease;
            this.TestSubmissionScript = string.IsNullOrEmpty(this.TestSubmissionScript) ? "deploy\\testSwarm.ps1" : this.TestSubmissionScript;
        }
    }
}
