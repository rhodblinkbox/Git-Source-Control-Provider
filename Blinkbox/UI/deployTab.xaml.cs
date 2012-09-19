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
    using System.Windows.Controls;
    using System.Windows.Threading;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="deployTab"/> class.
        /// </summary>
        public deployTab()
        {
            this.InitializeComponent();

            // Allow binding to local properties
            this.DataContext = this;

            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
            var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
            if (sccProvider != null)
            {
                sccProvider.OnSolutionOpen += (s, a) =>
                    {
                        try
                        {
                            Action action = () =>
                                {
                                    this.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                                };

                            this.Dispatcher.BeginInvoke(action, DispatcherPriority.ApplicationIdle);
                        }
                        catch 
                        {
                            
                        }
                    }; 
            }
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

        /// <summary>
        /// Gets the solution settings.
        /// </summary>
        /// <value>The solution settings.</value>
        public SolutionSettings solutionSettings
        {
            get
            {
                return this.SccProvider.SolutionOpen ? SolutionSettings.Current : null;
            }
        }
    }
}
