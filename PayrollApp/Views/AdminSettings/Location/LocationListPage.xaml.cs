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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.AdminSettings.Location
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LocationListPage : Page
    {
        public LocationListPage()
        {
            this.InitializeComponent();
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            // Make loadGrid to visible when loading location data.
            // Starts timer that will get data on first tick.
            loadTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();

            loadGrid.Visibility = Visibility.Visible;
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            ObservableCollection<PayrollCore.Entities.Location> getItem = await SettingsHelper.Instance.da.GetLocations(true);
            dataGrid.ItemsSource = getItem;
            loadTimer.Stop();
            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void dataGrid_AutoGeneratingColumn(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "locationName")
            {
                e.Column.Header = "Location Name";
            }
            else if (e.Column.Header.ToString() == "lv_enableGM")
            {
                e.Column.Header = "Allow GM Attendance";
            }
            else if (e.Column.Header.ToString() == "lv_isDisabled")
            {
                e.Column.Header = "Disabled";
            }
            else
            {
                e.Cancel = true;
            }
        }

        private async void addBtn_Click(object sender, RoutedEventArgs e)
        {
            // Shows loadGrid and creates a new location in the database with temporary name.
            loadGrid.Visibility = Visibility.Visible;
            PayrollCore.Entities.Location location = new PayrollCore.Entities.Location();
            location.isDisabled = false;
            location.locationName = Guid.NewGuid().ToString();
            location.enableGM = false;
            bool IsSucess = await SettingsHelper.Instance.da.AddLocationAsync(location);
            location = await SettingsHelper.Instance.da.GetLocationByName(location.locationName);
            location.isNewLocation = true;

            if (IsSucess)
            {
                this.Frame.Navigate(typeof(LocationDetailsPage), location, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Can't create a new location",
                    Content = "There is a problem in connecting to the database. Please try again in a while.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                PayrollCore.Entities.Location selectedLocation = (dataGrid.SelectedItem as PayrollCore.Entities.Location);
                this.Frame.Navigate(typeof(LocationDetailsPage), selectedLocation, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }
    }
}
