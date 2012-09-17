// -----------------------------------------------------------------------
// <copyright file="DeploySettings.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;


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
                        string configFileName = Path.Combine(solutionDirectory, Path.GetFileNameWithoutExtension(sccProvider.GetSolutionFileName()) + ".settings");

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
            this.TestSwarmUrl = string.IsNullOrEmpty(this.TestSwarmUrl) ? Environment.UserName : this.TestSwarmUrl;
            this.FeaturePath = string.IsNullOrEmpty(this.FeaturePath) ? ".\\test\\AngularClient.Test.Artefacts\\Features" : this.FeaturePath;
            this.TestBrowserSets = string.IsNullOrEmpty(this.TestBrowserSets) ? "default,currentDesktop" : this.TestBrowserSets;
            this.CurrentBranch = string.IsNullOrEmpty(this.CurrentBranch) ? "v0" : this.CurrentBranch;
            this.TestRunnerMode = string.IsNullOrEmpty(this.TestRunnerMode) ? "appfirst" : this.TestRunnerMode;
        }
    }
}
