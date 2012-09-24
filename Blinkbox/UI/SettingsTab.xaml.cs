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
    using System.IO;

    using GitScc.Blinkbox.Options;

    /// <summary>
    /// Interaction logic for SettingsTab.xaml
    /// </summary>
    public partial class SettingsTab : UserControl
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

        public UserSettings userSettings
        {
            get
            {
                return UserSettings.Current;
            }
        }


        public SettingsTab()
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
            UserSettings.Current.Save();
        }

        private void PSButton_Click(object sender, RoutedEventArgs e)
        {
            var sccProviderService = BasicSccProvider.GetServiceEx<SccProviderService>();

            var scriptName = Path.Combine(sccProviderService.GetSolutionDirectory(), "deploy\\setExecutionPolicy.ps1");

            if (!File.Exists(scriptName))
            {
                NotificationService.DisplayError("Task Failed", "Cannot find the required powershell script at " + scriptName);
                return;
            }

            var powershellCall = string.Format(
                "& '{0}'",
                scriptName);

            var command = new SccCommand("powershell.exe", powershellCall);
            command.Start();
        }
    }
}
