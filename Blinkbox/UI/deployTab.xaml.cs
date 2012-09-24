// --------------------------------------------------------------------------------------------------------------------
// <copyright file="deployTab.xaml.cs" company="blinkbox">
//   TODO: add comment
// </copyright>
// <summary>
//   Interaction logic for deployTab.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.UI
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;

    using GitScc.Blinkbox.Data;
    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Interaction logic for deployTab.xaml
    /// </summary>
    public partial class deployTab
    {
        /// <summary>
        /// instance of the <see cref="DevelopmentService"/>
        /// </summary>
        private DevelopmentService developmentServiceInstance = null;

        /// <summary>
        /// instance of the <see cref="SccProviderService"/>
        /// </summary>
        private SccProviderService sccProviderInstance = null;

        public SolutionUserSettings solutionUserSettings
        {
            get
            {
                return SolutionUserSettings.Current;
            }
        }

        public SolutionSettings solutionSettings
        {
            get
            {
                return SolutionSettings.Current;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="deployTab"/> class.
        /// </summary>
        public deployTab()
        {
            this.InitializeComponent();

            // Allow binding to local properties
            grid.DataContext = this;

            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
            var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
            if (sccProvider != null)
            {
                sccProvider.OnSolutionOpen += (s, a) => this.RefreshBindings(); 
            }
        }

        /// <summary>
        /// Refreshes the data context bindings to update the UI.
        /// </summary>
        public void RefreshBindings()
        {
            // TODO: proper way to do this?
            grid.DataContext = null;
            grid.DataContext = this;
        }

        /// <summary>
        /// Gets the development service.
        /// </summary>
        /// <value>The development service.</value>
        private DevelopmentService DevelopmentService
        {
            get
            {
                this.developmentServiceInstance = this.developmentServiceInstance ?? BasicSccProvider.GetServiceEx<DevelopmentService>();
                return this.developmentServiceInstance;
            }
        }

        /// <summary>
        /// Gets the development service.
        /// </summary>
        /// <value>The development service.</value>
        private SccProviderService SccProvider
        {
            get
            {
                this.sccProviderInstance = this.sccProviderInstance ?? BasicSccProvider.GetServiceEx<SccProviderService>();
                return this.sccProviderInstance;
            }
        }



        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SolutionSettings.Current.Save();
            SolutionUserSettings.Current.Save();
            UserSettings.Current.Save();
        }

        private void Deploylink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SolutionUserSettings.Current.LastDeployment == null)
            {
                return;
            }

            if (sender == AppLink)
            {
                BasicSccProvider.LaunchBrowser(SolutionUserSettings.Current.LastDeployment.AppUrl);
            }
            else
            {
                BasicSccProvider.LaunchBrowser(SolutionUserSettings.Current.LastDeployment.TestRunUrl);
            }
        }

    }
}
