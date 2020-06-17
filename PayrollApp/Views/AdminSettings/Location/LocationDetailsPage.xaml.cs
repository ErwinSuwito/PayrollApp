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
    public sealed partial class LocationDetailsPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        PayrollCore.Entities.Location location;
        Shift specialTask;

        public LocationDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                location = (e.Parameter as PayrollCore.Entities.Location);
                return;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            if (location.isNewLocation)
            {
                pageTitle.Text = "New location";
                deleteButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                location.isNewLocation = false;
                locationName.Text = location.locationName;
                enableMeetingSwitch.IsOn = location.enableGM;
            }

            HideUIParts();

            // Make loadGrid to visible when loading location data.
            // Starts timer that will get data on first tick.
            loadTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();

            loadGrid.Visibility = Visibility.Visible;

            Debug.WriteLine("PageLoaded location.locationID: " + location.locationID);
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();

            ObservableCollection<Rate> getRates = await SettingsHelper.Instance.da.GetAllRates(false);
            defaultRateBox.ItemsSource = getRates;

            specialTask = await SettingsHelper.Instance.da.GetSpecialTaskShift(location.locationID);

            ObservableCollection<Meeting> getItem = await SettingsHelper.Instance.da.GetMeetings(location);
            dataGrid.ItemsSource = getItem;

            if (getItem.Count <1)
            {
                noItemsPanel.Visibility = Visibility.Visible;
            }

            if (location.isNewLocation == false)
            {
                try
                {
                    for (int i = 0; i < getRates.Count; i++)
                    {
                        var item = getRates.ElementAt(i) as Rate;
                        if (item.rateID == specialTask.DefaultRate.rateID)
                        {
                            defaultRateBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    defaultRateBox.SelectedIndex = 0;
                }
            }

            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LocationListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void dataGrid_AutoGeneratingColumn(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "meetingName")
            {
                e.Column.Header = "Meeting Name";
            }
            else if (e.Column.Header.ToString() == "meetingDay")
            {
                e.Column.Header = "Meeting Day";
            }
            else if (e.Column.Header.ToString() == "bmOnly")
            {
                e.Column.Header = "BM only";
            }
            else if (e.Column.Header.ToString() == "isDisabled")
            {
                e.Column.Header = "Meeting Disabled";
            }
            else
            {
                e.Cancel = true;
            }
        }

        private async void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                Meeting meeting = (dataGrid.SelectedItem as Meeting);
                this.Frame.Navigate(typeof(MeetingDetailsPage), meeting, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private async void addBtn_Click(object sender, RoutedEventArgs e)
        {
            Meeting meeting = new Meeting();
            meeting.locationID = location.locationID;
            meeting.meetingName = "New meeting";
            meeting.newMeeting = true;
            meeting.rate = new Rate();
            meeting.rate.rateID = 1;
            meeting.meetingID = await SettingsHelper.Instance.da.SaveMeetingAndReturnId(meeting);

            this.Frame.Navigate(typeof(MeetingDetailsPage), meeting, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            location.isDisabled = false;

            bool IsSuccess = await SaveLocationInfo();
            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents location to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            location.isDisabled = true;

            bool IsSuccess = await SaveLocationInfo();
            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents location to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private void locationName_TextChanged(object sender, TextChangedEventArgs e)
        {
            location.locationName = locationName.Text;
        }

        private void enableMeetingSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            location.enableGM = enableMeetingSwitch.IsOn;
        }

        async Task<bool> SaveLocationInfo()
        {
            bool IsSuccess = await SettingsHelper.Instance.da.SaveLocationAsync(location);

            if (IsSuccess == true)
            {
                if (location.isNewLocation)
                {
                    specialTask = new Shift();
                    specialTask.shiftName = "Special Task";
                    specialTask.startTime = TimeSpan.MinValue;
                    specialTask.endTime = TimeSpan.MaxValue;
                    specialTask.locationID = location.locationID;
                    specialTask.isDisabled = true;
                    specialTask.WeekendOnly = false;
                    specialTask.DefaultRate = defaultRateBox.SelectedItem as Rate;

                    IsSuccess = await SettingsHelper.Instance.da.AddNewShift(specialTask);
                }
                else
                {
                    specialTask.DefaultRate = defaultRateBox.SelectedItem as Rate;
                    IsSuccess = await SettingsHelper.Instance.da.UpdateShiftInfo(specialTask);
                }
            }

            return IsSuccess;
        }

        private async void enableButton_Click(object sender, RoutedEventArgs e)
        {
            location.isDisabled = false;

            bool IsSuccess = await SaveLocationInfo();
            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents location to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private void HideUIParts()
        {
            if (location != null)
            {
                if (location.isDisabled)
                {
                    enableButton.Visibility = Visibility.Visible;
                    deleteButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    enableButton.Visibility = Visibility.Collapsed;
                    deleteButton.Visibility = Visibility.Visible;
                }
            }
            else
            {
                enableButton.Visibility = Visibility.Collapsed;
                deleteButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
