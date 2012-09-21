// -----------------------------------------------------------------------
// <copyright file="DeploySettings.cs" company="">
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

    using GitScc.Blinkbox.Data;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SolutionUserSettings : SettingsBase
    {
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static SolutionUserSettings sccOptions;

        public string TestSwarmUsername { get; set; }
        public string TestSwarmPassword { get; set; }
        public string TestSwarmTags { get; set; }
        public bool? SubmitTestsOnDeploy { get; set; }

        public Deployment LastDeployment { get; set; }
        

        /// <summary>
        /// Gets Current.
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
        /// Inits this instance.
        /// </summary>
        protected override void Init()
        {
            this.TestSwarmPassword = string.IsNullOrEmpty(this.TestSwarmPassword) ? "1234$abcd" : this.TestSwarmPassword;
            this.TestSwarmTags = string.IsNullOrEmpty(this.TestSwarmTags) ? SolutionSettings.Current.TestSwarmTags : this.TestSwarmTags;
            this.TestSwarmUsername = string.IsNullOrEmpty(this.TestSwarmUsername) ? Environment.UserName : this.TestSwarmUsername;
            this.SubmitTestsOnDeploy = this.SubmitTestsOnDeploy ?? true;
        }
    }
}
