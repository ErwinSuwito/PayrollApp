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

namespace PayrollApp.Views.AdminSettings.Meetings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MeetingListPage : Page
    {
        public MeetingListPage()
        {
            this.InitializeComponent();
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        Location location;
        int locationID;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                location = e.Parameter as Location;
            }
            base.OnNavigatedTo(e);
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            if (location != null)
            {
                locationID = location.locationID;
            }
            else
            {
                locationID = SettingsHelper.Instance.appLocation.locationID;
            }

            ObservableCollection<Meeting> meetings = await SettingsHelper.Instance.op2.GetMeetings(true, locationID, true);
            meetingListView.ItemsSource = meetings;
            loadGrid.Visibility = Visibility.Collapsed;

            if (meetings.Count == 0)
            {
                noItemsPanel.Visibility = Visibility.Visible;
            }
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Instance.InitState == SettingsHelper.InitStates.Setup)
            {
                this.Frame.Navigate(typeof(FirstRunSetup.LocationSetupPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else
            {
                this.Frame.Navigate(typeof(NewSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        private async void addBtn_Click(object sender, RoutedEventArgs e)
        {
            Meeting meeting = new Meeting();
            meeting.locationID = locationID;
            meeting.newMeeting = true;
            this.Frame.Navigate(typeof(MeetingDetailsPage), meeting, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void meetingListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Meeting meeting = e.ClickedItem as Meeting;
            this.Frame.Navigate(typeof(MeetingDetailsPage), meeting, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
