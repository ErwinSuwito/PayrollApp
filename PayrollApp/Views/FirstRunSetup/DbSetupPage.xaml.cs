
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.FirstRunSetup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DbSetupPage : Page
    {
        public DbSetupPage()
        {
            this.InitializeComponent();
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            string connString;

            if (connStringToggle.IsOn)
            {
                connString = connStringTextBox.Text;
            }
            else
            {
                if (securityTypeToggle.IsOn)
                {
                    connString = SettingsHelper.Instance.GenerateConnectionString(dataSourceTextBox.Text, dbNameTextBox.Text);
                }
                else
                {
                    connString = SettingsHelper.Instance.GenerateConnectionString(dataSourceTextBox.Text, dbNameTextBox.Text, userNameTextBox.Text, passwordBox.Password);
                }
            }

            if (SettingsHelper.Instance.da.TestConnString(connString))
            {
                SettingsHelper.Instance.SaveConnectionString(true, connString);
                this.Frame.Navigate(typeof(CardDbSetupPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to connect to database!",
                    Content = "We can't connect to the database based on the specified info. Make sure that this device has network connection and that the database is reachable.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }
    }
}
