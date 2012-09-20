// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestTab.xaml.cs" company="blinkbox">
//   TestTab control
// </copyright>
// <summary>
//   Interaction logic for TestTab.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GitScc.Blinkbox.UI
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Threading;

    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Interaction logic for TestTab.xaml
    /// </summary>
    public partial class TestTab : UserControl
    {
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


        public TestTab()
        {
            InitializeComponent();

            grid.DataContext = this;

            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
            var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
            if (sccProvider != null)
            {
                sccProvider.OnSolutionOpen += (s, a) =>
                    {
                        // Refresh data context to trigger update
                        grid.DataContext = null;
                        grid.DataContext = this;
                    };
            }
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SolutionSettings.Current.Save();
            SolutionUserSettings.Current.Save();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            BasicSccProvider.LaunchBrowser(SolutionSettings.Current.TestSwarmUrl + "/user/" + SolutionUserSettings.Current.TestSwarmUsername);
        }
    }
}
