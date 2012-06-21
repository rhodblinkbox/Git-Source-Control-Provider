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
        /// The name of the config file
        /// </summary>
        private static string configFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "blinkboxScc.config");

        /// <summary>
        /// Gets or sets GitTfsPath.
        /// </summary>
        public string GitTfsPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether deployed urls should be launched in VS or in the default browser.
        /// </summary>
        /// <value>
        /// <c>true</c> if [launch deployed urls in VS]; otherwise, <c>false</c>.
        /// </value>
        public bool LaunchDeployedUrlsInVS { get; set; }

        /// <summary>
        /// BlinkboxSccOptions.
        /// </summary>
        private static BlinkboxSccOptions sccOptions;

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
        /// Prevents a default instance of the <see cref="BlinkboxSccOptions"/> class from being created. 
        /// Initializes a new instance of the <see cref="BlinkboxSccOptions"/> class.
        /// </summary>
        private BlinkboxSccOptions()
        {
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
