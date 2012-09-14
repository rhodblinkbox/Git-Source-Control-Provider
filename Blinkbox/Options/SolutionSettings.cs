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


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SolutionSettings : SettingsBase
    {
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static SolutionSettings sccOptions;

        public string TestSwarmUsername { get; set; }
        public string TestSwarmPassword { get; set; }
        public string TestSwarmTags { get; set; }

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
                    var solutionFileName = sccProvider.GetSolutionFileName();
                    if (!string.IsNullOrEmpty(solutionFileName))
                    {
                        // Solution is open. 
                        string configFileName = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.GetFileNameWithoutExtension(sccProvider.GetSolutionFileName()) + ".user.settings");

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
            // this.TfsRemoteBranch = string.IsNullOrEmpty(this.TfsRemoteBranch) ? "remotes/tfs/default" : this.TfsRemoteBranch;
            this.TestSwarmPassword = string.IsNullOrEmpty(this.TestSwarmPassword) ? "1234$abcd" : this.TestSwarmPassword;
            this.TestSwarmTags = string.IsNullOrEmpty(this.TestSwarmTags) ? "devcomplete" : this.TestSwarmTags;
            this.TestSwarmUsername = string.IsNullOrEmpty(this.TestSwarmUsername) ? Environment.UserName : this.TestSwarmUsername;
        }
    }
}
