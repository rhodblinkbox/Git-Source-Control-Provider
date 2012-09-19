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
    using System.Windows.Controls;
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
                                    this.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                                };

                            this.Dispatcher.BeginInvoke(action, DispatcherPriority.ApplicationIdle);
                        }
                        catch
                        {

                        }
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
