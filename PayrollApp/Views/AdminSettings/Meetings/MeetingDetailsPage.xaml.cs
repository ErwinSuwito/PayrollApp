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
using Windows.UI.Xaml.Documents;
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
    public sealed partial class MeetingDetailsPage : Page
    {
        Meeting meeting;
        int groupIndex = 0;
        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        DispatcherTimer loadTimer2 = new DispatcherTimer();
        ObservableCollection<UserGroup> userGroups = new ObservableCollection<UserGroup>();
        List<MeetingUserGroup> selectedUserGroup = new List<MeetingUserGroup>();

        public MeetingDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                meeting = e.Parameter as Meeting;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            loadTimer.Interval = new TimeSpan(0, 0, 1);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();

            if (meeting.newMeeting == false)
            {
                pageTitle.Text = "Meeting details";
                meetingNameBox.Text = meeting.meetingName;
                daySelector.SelectedIndex = meeting.meetingDay;
                disableMeetingBtn.Visibility = Visibility.Visible;
                startTimePicker.Time = meeting.StartTime;
            }
            else
            {
                disableMeetingBtn.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            userGroups = await SettingsHelper.Instance.op2.GetUserGroups(false, false);
            userGroupSelector.ItemsSource = userGroups;

            ObservableCollection<Rate> rate = await SettingsHelper.Instance.op2.GetAllRates(false);
            defaultRateBox.ItemsSource = rate;

            if (!meeting.newMeeting)
            {
                selectedUserGroup = await SettingsHelper.Instance.op2.GetMeetingUserGroups(meeting.meetingID);
                for (int i = 0; i < rate.Count; i++)
                {
                    var item = rate.ElementAt(i) as Rate;
                    if (item.rateID == meeting.rate.rateID)
                    {
                        defaultRateBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            loadTimer2.Interval = new TimeSpan(0, 0, 0, 0, 60);
            loadTimer2.Tick += LoadTimer2_Tick;
            loadTimer2.Start();
        }

        private void LoadTimer2_Tick(object sender, object e)
        {
            loadTimer2.Stop();

            if (!meeting.newMeeting)
            {
                foreach (var item in selectedUserGroup)
                {
                    for (int i = 0; i < userGroups.Count; i++)
                    {
                        ListViewItem listViewItem = userGroupSelector.ContainerFromIndex(i) as ListViewItem;
                        Debug.WriteLine("item usrGroupID: " + item.usrGroupId);
                        Debug.WriteLine("userGroups groupID: " + userGroups[i].groupID);

                        if (item.usrGroupId == userGroups[i].groupID)
                        {
                            listViewItem.IsSelected = true;
                            break;
                        }
                    }
                }
            }

            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                this.Frame.Navigate(typeof(NewSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        private async void disableMeetingBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Disable meeting?",
                Content = "Disabling meeting will not allow any users to log their attendance for this meeting. If there no attendance record for this meeting, the meeting will be deleted.",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };

            ContentDialogResult result = await contentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                loadGrid.Visibility = Visibility.Visible;
                meeting.isDisabled = true;

                bool IsSuccess = await SaveChanges();

                if (!IsSuccess)
                {
                    ContentDialog contentDialog1 = new ContentDialog()
                    {
                        Title = "Unable to save changes",
                        Content = "There's a problem in saving your changes. Please try again later.",
                        PrimaryButtonText = "More info",
                        CloseButtonText = "Ok"
                    };

                    result = await contentDialog1.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        Exception ex = SettingsHelper.Instance.op2.GetLastError();
                        string message;
                        if (ex == null)
                        {
                            message = "No details";
                        }
                        else
                        {
                            message = ex.Message;
                        }
                        contentDialog1 = new ContentDialog()
                        {
                            Title = "More info",
                            Content = message,
                            CloseButtonText = "Ok"
                        };

                        await contentDialog1.ShowAsync();
                    }
                }

                loadGrid.Visibility = Visibility.Collapsed;
            }

            
        }

        private async void saveMeetingBtn_Click(object sender, RoutedEventArgs e)
        {
            bool IsSuccess = await SaveChanges();

            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save meeting",
                    Content = "There is an error in saving the meeting. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        async Task<bool> SaveChanges()
        {
            meeting.meetingDay = daySelector.SelectedIndex;
            meeting.meetingName = meetingNameBox.Text;
            meeting.StartTime = startTimePicker.Time;
            meeting.rate = defaultRateBox.SelectedItem as Rate;

            List<MeetingUserGroup> meetingUserGroups = new List<MeetingUserGroup>();

            foreach (UserGroup item in userGroupSelector.SelectedItems)
            {
                MeetingUserGroup meetingUserGroup = new MeetingUserGroup()
                {
                    meetingID = meeting.meetingID,
                    usrGroupId = item.groupID
                };

                meetingUserGroups.Add(meetingUserGroup);
            }

            bool IsSuccess;

            if (meeting.newMeeting)
            {
                IsSuccess = await SettingsHelper.Instance.op2.AddNewMeeting(meeting, meetingUserGroups);
            }
            else
            {
                IsSuccess = await SettingsHelper.Instance.op2.UpdateMeeting(meeting, meetingUserGroups);
            }

            return IsSuccess;
        }
    }
}
