using PayrollCore.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
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
    public sealed partial class LocationSetupPage : Page
    {
        public LocationSetupPage()
        {
            this.InitializeComponent();
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            loadTimer.Interval = new TimeSpan(0, 0, 1);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();

        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();

            if (SettingsHelper.Instance.InitState == SettingsHelper.InitStates.InProgress)
            {
                loadTimer.Start();
            }
            else
            {
                locationSelectionView.ItemsSource = await SettingsHelper.Instance.op2.GetLocations(false);
                loadGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (locationSelectionView.SelectedItem != null)
            {
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["selectedLocation"] = (locationSelectionView.SelectedItem as Location).locationID;
                this.Frame.Navigate(typeof(FaceAPISetupPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                ContentDialog testDialog = new ContentDialog
                {
                    Title = "Please select a location",
                    Content = "You haven't selected a location for this payroll machine. Make a selection by tapping one of the options.",
                    PrimaryButtonText = "Ok"
                };

                await testDialog.ShowAsync();
            }
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Instance.InitState = SettingsHelper.InitStates.Setup;
            this.Frame.Navigate(typeof(AdminSettings.Locations.LocationDetailsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
