using Microsoft.Toolkit.Graph.Providers;
using PayrollCore.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.AdminSettings.Database
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatabaseConnPage : Page
    {
        public DatabaseConnPage()
        {
            this.InitializeComponent();
            this.DataContext = SettingsHelper.Instance;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            payrollDbConn.Text = localSettings.Values["DbConnString"].ToString();
            cardsDbConn.Text = localSettings.Values["CardConnString"].ToString();
            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                this.Frame.Navigate(typeof(LoginPagev2), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private async void changePayrollConn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Change connection setting?",
                Content = "Configuring a wrong connection will cause the app to not work properly. You'll be logged out after changing this settings.",
                PrimaryButtonText = "Change",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.Frame.Navigate(typeof(FirstRunSetup.DbSetupPage), string.Empty, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private async void changeCardsConn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Change connection setting?",
                Content = "Configuring a wrong connection will cause the app to not work properly. You'll be logged out after changing this settings.",
                PrimaryButtonText = "Change",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.Frame.Navigate(typeof(FirstRunSetup.CardDbSetupPage), string.Empty, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}
