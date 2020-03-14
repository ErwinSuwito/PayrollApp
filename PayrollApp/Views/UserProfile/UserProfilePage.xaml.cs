using PayrollCore;
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

namespace PayrollApp.Views.UserProfile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserProfilePage : Page
    {
        UserState userState = SettingsHelper.Instance.userState;

        public UserProfilePage()
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
            string greeting;

            if (SettingsHelper.Instance.appLocation.enableGM == false)
            {
                meetingButton.Visibility = Visibility.Visible;
            }

            if (userState.user.userGroup.ShowAdminSettings == true)
            {
                adminSettingsButton.Visibility = Visibility.Visible;
            }

            if (userState.LatestActivity != null)
            {
                if (userState.LatestActivity.outTime != null)
                {
                    greeting = "You are already signed in for";
                    // User is still logged in
                    if (userState.LatestActivity.StartShift != null && userState.LatestActivity.EndShift != null)
                    {
                        // User is on duty.
                        if (userState.LatestActivity.StartShift.shiftName == userState.LatestActivity.EndShift.shiftName)
                        {
                            greeting += userState.LatestActivity.StartShift.shiftName;
                        }
                        else
                        {
                            greeting += userState.LatestActivity.StartShift.shiftName + " and " + userState.LatestActivity.EndShift.shiftName;
                        }

                        if (userState.LatestActivity.inTime.Date == DateTime.Today)
                        {
                            greeting += " today.";
                        }
                        else
                        {
                            greeting += " on " + userState.LatestActivity.inTime.ToShortDateString();
                        }
                    }
                    else
                    {
                        // user is not on duty, but is logged in (for special task or meeting)

                    }
                }
                else
                {

                }

                if (userState.signInInfo.inTime.Date == DateTime.Today)
                {
                    greetingTextBlock.Text = "You are already signed in for " + userState.signInInfo.StartShift.shiftName + " today.";
                }
                else
                {
                    greetingTextBlock.Text = "You are still signed in for " + userState.signInInfo.StartShift.shiftName + " on " + userState.signInInfo.inTime.ToShortDateString();
                }
            }

            if (userState.meetingAttendance != null)
            {
                greetingTextBlock.Text = "Your attendance for the meeting has been recorded.";
            }
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void signButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void adminSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminSettings.NewSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void improveRecButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
