// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SolutionUserSettings.cs" company="blinkbox">
//   TODO: Update copyright text.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.IO;

    using GitScc.Blinkbox.Data;

    /// <summary>
    /// Provides user settings for a solution.
    /// </summary>
    public class SolutionUserSettings : SettingsBase
    {
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static SolutionUserSettings sccOptions;

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        public static SolutionUserSettings Current
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
                        string configFileName = Path.Combine(solutionDirectory, Path.GetFileNameWithoutExtension(solutionFileName) + "." + Extension + ".user");
                        sccOptions = SettingsBase.LoadFromConfig<SolutionUserSettings>(configFileName);
                    }
                }

                return sccOptions;
            }
        }

        /// <summary>
        /// Gets or sets the test swarm username.
        /// </summary>
        /// <value>The test swarm username.</value>
        public string TestSwarmUsername { get; set; }

        /// <summary>
        /// Gets or sets the test swarm password.
        /// </summary>
        /// <value>The test swarm password.</value>
        public string TestSwarmPassword { get; set; }

        /// <summary>
        /// Gets or sets the test swarm tags.
        /// </summary>
        /// <value>The test swarm tags.</value>
        public string TestSwarmTags { get; set; }

        /// <summary>
        /// Gets or sets the submit tests on deploy.
        /// </summary>
        /// <value>The submit tests on deploy.</value>
        public bool? SubmitTestsOnDeploy { get; set; }

        /// <summary>
        /// Gets or sets the local app URL template.
        /// </summary>
        /// <value>The local app URL template.</value>
        public string LocalAppUrlTemplate { get; set; }

        /// <summary>
        /// Gets or sets the local test URL template.
        /// </summary>
        /// <value>The local test URL template.</value>
        public string LocalTestUrlTemplate { get; set; }

        /// <summary>
        /// Gets or sets the last deployment.
        /// </summary>
        /// <value>The last deployment.</value>
        public Deployment LastDeployment { get; set; }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        protected override void Init()
        {
            this.TestSwarmPassword = string.IsNullOrEmpty(this.TestSwarmPassword) ? "1234$abcd" : this.TestSwarmPassword;
            this.TestSwarmTags = string.IsNullOrEmpty(this.TestSwarmTags) ? SolutionSettings.Current.TestSwarmTags : this.TestSwarmTags;
            this.TestSwarmUsername = string.IsNullOrEmpty(this.TestSwarmUsername) ? Environment.UserName : this.TestSwarmUsername;
            this.SubmitTestsOnDeploy = this.SubmitTestsOnDeploy ?? true;
            this.LocalAppUrlTemplate = string.IsNullOrEmpty(this.LocalAppUrlTemplate)
                ? "http://tv-{MachineName}.bbdev1.com/Client/{BuildLabel}"
                : this.LocalAppUrlTemplate;
            this.LocalTestUrlTemplate = string.IsNullOrEmpty(this.LocalTestUrlTemplate) 
                ? (this.LocalAppUrlTemplate + "/Test/Index.html?runnerMode={RunnerMode}&tags={Tags}")
                : this.LocalTestUrlTemplate;
        }
    }
}
