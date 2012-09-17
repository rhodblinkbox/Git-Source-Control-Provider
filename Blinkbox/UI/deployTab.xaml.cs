using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitScc.Blinkbox.UI
{
    using System.Windows.Threading;

    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Interaction logic for deployTab.xaml
    /// </summary>
    public partial class deployTab : UserControl
    {


        /// <summary>
        /// instance of the <see cref="DevelopmentService"/>
        /// </summary>
        private DevelopmentService developmentServiceInstance = null;

        /// <summary>
        /// instance of the <see cref="SccProviderService"/>
        /// </summary>
        private SccProviderService sccProviderInstance = null;


        public deployTab()
        {
            InitializeComponent();

            // Allow binding to local properties
            DataContext = this;

            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
            var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
            if (sccProvider != null)
            {
                sccProvider.OnSolutionOpen += (s, a) => this.PopulateDeployTab();
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


        private void PopulateDeployTab()
        {
            if (SolutionSettings.Current != null)
            {
                Action action = () =>
                {
                    testSwarmPassword.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    testSwarmTags.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    testSwarmUsername.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                };
                this.Dispatcher.BeginInvoke(action, DispatcherPriority.ApplicationIdle);
            }
        }
    }
}
