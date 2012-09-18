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
    using System.Windows.Controls;

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

            // Allow binding to local properties
            grid.DataContext = this;

            // Register this component as a service so that we can use it externally. 
            BasicSccProvider.RegisterService(this);
            var sccProvider = BasicSccProvider.GetServiceEx<SccProviderService>();
            if (sccProvider != null)
            {
                sccProvider.OnSolutionOpen += (s, a) =>
                    { 
                        this.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    };
            }
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SolutionSettings.Current.Save();
            SolutionUserSettings.Current.Save();
        }
    }
}
