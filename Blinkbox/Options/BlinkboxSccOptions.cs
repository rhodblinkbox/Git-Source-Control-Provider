// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlinkboxSccOptions.cs" company="blinkbox">
// TODO: Update copyright text.
// </copyright>
// <summary>
//   SccOptions specific to the blinkbox implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.Options
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>
    /// SccOptions specific to the blinkbox implementation
    /// </summary>
    [Serializable]
    public class BlinkboxSccOptions
    {
        /// <summary>
        /// The name of the tfs remote branch
        /// </summary>
        public const string HeadRevision = "HEAD";

        /// <summary>
        /// The name of the config file
        /// </summary>
        private static string configFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "blinkboxScc.config");
        
        /// <summary>
        /// private instance of BlinkboxSccOptions.
        /// </summary>
        private static BlinkboxSccOptions sccOptions;

        /// <summary>
        /// Prevents a default instance of the <see cref="BlinkboxSccOptions"/> class from being created. 
        /// Initializes a new instance of the <see cref="BlinkboxSccOptions"/> class.
        /// </summary>
        private BlinkboxSccOptions()
        {
        }

        /// <summary>
        /// Gets Current.
        /// </summary>
        public static BlinkboxSccOptions Current
        {
            get
            {
                if (sccOptions == null)
                {
                    sccOptions = LoadFromConfig();
                }

                return sccOptions;
            }
        }

        /// <summary>
        /// Gets or sets GitTfsPath.
        /// </summary>
        public string GitTfsPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the TFS merge branch.
        /// </summary>
        public string TfsRemoteBranch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether deployed urls should be launched in VS or in the default browser.
        /// </summary>
        /// <value>
        /// <c>true</c> if [launch deployed urls in VS]; otherwise, <c>false</c>.
        /// </value>
        public bool LaunchDeployedUrlsInVS { get; set; }

        /// <summary>
        /// Gets or sets the name of the project which handles the deploy part of the commit and deploy operation.
        /// </summary>
        public string PostCommitDeployProjectName { get; set; }

        /// <summary>
        /// Gets or sets the name of the property which provides the url to be launched after a commit and deploy.
        /// </summary>
        public string UrlToLaunchPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the property which provides the commit guid to the PostCommitDeployProject.
        /// </summary>
        public string CommitGuidPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the property which provides the commit comment to the PostCommitDeployProject.
        /// </summary>
        public string CommitCommentPropertyName { get; set; }

        /// <summary>
        /// Gets a value indicating whether TortoiseGit is available.
        /// </summary>
        /// <value><c>true</c> if TortoiseGit is available otherwise, <c>false</c>.</value>
        public bool TortoiseAvailable
        {
            get
            {
                return !string.IsNullOrEmpty(GitSccOptions.Current.TortoiseGitPath) && File.Exists(GitSccOptions.Current.TortoiseGitPath);
            }
        }

        /// <summary>
        /// Loads settings from a config file.
        /// </summary>
        /// <returns>a BlinkboxSccOptions instance.</returns>
        internal static BlinkboxSccOptions LoadFromConfig()
        {
            BlinkboxSccOptions options = null;
            
            if (File.Exists(configFileName))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(BlinkboxSccOptions));
                    using (TextReader tr = new StreamReader(configFileName))
                    {
                        options = (BlinkboxSccOptions)serializer.Deserialize(tr);
                    }
                }
                catch (Exception)
                {
                }
            }

            if (options == null)
            {
                options = new BlinkboxSccOptions();
            }

            options.Init();

            return options;
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        private void Init()
        {
            if (string.IsNullOrEmpty(this.GitTfsPath))
            {
                var possibleLocations = new string[] { @"C:\Program Files\Git-tfs\git-tfs.exe", @"C:\Program Files (x86)\Git-tfs\git-tfs.exe", };
                this.GitTfsPath = this.TryFindFile(possibleLocations);
            }

            this.PostCommitDeployProjectName = string.IsNullOrEmpty(this.PostCommitDeployProjectName) ? "postCommitDeploy.proj" : this.PostCommitDeployProjectName;
            this.UrlToLaunchPropertyName = string.IsNullOrEmpty(this.UrlToLaunchPropertyName) ? "UrlToLaunch" : this.UrlToLaunchPropertyName;
            this.CommitGuidPropertyName = string.IsNullOrEmpty(this.CommitGuidPropertyName) ? "CommitGuid" : this.CommitGuidPropertyName;
            this.CommitCommentPropertyName = string.IsNullOrEmpty(this.CommitCommentPropertyName) ? "CommitComment" : this.CommitCommentPropertyName;
            this.TfsRemoteBranch = string.IsNullOrEmpty(this.TfsRemoteBranch) ? "remotes/tfs/default" : this.TfsRemoteBranch;
        }

        /// <summary>
        /// Saves the config file.
        /// </summary>
        internal void SaveConfig()
        {
            var serialiser = new XmlSerializer(typeof(BlinkboxSccOptions));
            using (TextWriter tw = new StreamWriter(configFileName))
            {
                serialiser.Serialize(tw, this);
            }
        }

        /// <summary>
        /// Tries the find file.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>the path if the file is found.</returns>
        private string TryFindFile(string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
