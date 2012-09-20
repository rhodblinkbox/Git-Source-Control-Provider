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
    public class UserSettings : SettingsBase
    {
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static UserSettings sccOptions;

        public bool? PreviewGetLatest { get; set; }
        public bool? OpenUrlsInVS { get; set; }
        public bool? OpenUrlsAfterDeploy { get; set; }
        
        /// <summary>
        /// Gets Current.
        /// </summary>
        public static UserSettings Current
        {
            get
            {
                if (sccOptions == null)
                {
                    string configFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "blinkboxScc.settings");
                    sccOptions = SettingsBase.LoadFromConfig<UserSettings>(configFileName);
                }

                return sccOptions;
            }
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        protected override void Init()
        {
            this.PreviewGetLatest = this.PreviewGetLatest ?? false;
            this.OpenUrlsInVS = this.OpenUrlsInVS ?? false;
            this.OpenUrlsAfterDeploy = this.OpenUrlsAfterDeploy ?? true;
        }
    }
}
