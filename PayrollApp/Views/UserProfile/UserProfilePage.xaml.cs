using PayrollCore;
using System;
using System.Collections.Generic;
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

            // Make changes to the UI according to settings and user state
            ModifyUI();
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void signButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Instance.userState.LatestActivity.NoActivity == true)
            {
                this.Frame.Navigate(typeof(SignInOut.SignInPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                if (SettingsHelper.Instance.userState.LatestActivity.outTime == DateTime.MinValue && SettingsHelper.Instance.userState.LatestActivity.IsSpecialTask == false)
                {
                    this.Frame.Navigate(typeof(SignInOut.SignOutPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                else
                {
                    this.Frame.Navigate(typeof(SignInOut.SignInPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Instance.userState = null;
            this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void adminSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminSettings.NewSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void improveRecButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewUserOnboarding.FaceRecSetupPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void ModifyUI()
        {
            fullNameTextBlock.Text = userState.user.fullName;
            string greeting = "";

            // Starts modifying UI
            // Modifies UI based on settings
            if (SettingsHelper.Instance.appLocation.enableGM == true)
            {
                meetingButton.Visibility = Visibility.Visible;
            }

            if (userState.user.userGroup.ShowAdminSettings == true)
            {
                adminSettingsButton.Visibility = Visibility.Visible;
            }

            if (userState.LatestActivity.NoActivity == false)
            {
                if (userState.LatestActivity.outTime == DateTime.MinValue)
                {
                    if (userState.LatestActivity.IsSpecialTask == true)
                    {
                        signButton.Visibility = Visibility.Collapsed;
                        if (userState.LatestMeeting != null && userState.LatestMeeting.outTime == DateTime.MinValue)
                        {
                            if (userState.LatestMeeting.outTime == DateTime.MinValue)
                            {
                                greeting = "You are signed in for special task.";
                            }
                            else
                            {
                                greeting = "You are signed in for both special task and meeting.";
                            }
                        }
                        else
                        {
                            greeting = "You are signed in for special task.";
                        }
                    }
                    else
                    {
                        specialTaskButton.Visibility = Visibility.Collapsed;
                        signButton.Content = "Sign out";
                        if (userState.LatestMeeting != null && userState.LatestMeeting.outTime == DateTime.MinValue)
                        {
                            if (userState.LatestMeeting.outTime == DateTime.MinValue)
                            {
                                if (userState.LatestActivity.StartShift.shiftID == userState.LatestActivity.EndShift.shiftID)
                                {
                                    greeting = "You are signed in for " + userState.LatestActivity.StartShift.shiftName + ".";
                                }
                                else
                                {
                                    greeting = "You are signed in for " + userState.LatestActivity.StartShift.shiftName + " to " + userState.LatestActivity.EndShift.shiftName + ".";
                                }
                            }
                            else
                            {
                                if (userState.LatestActivity.StartShift.shiftID == userState.LatestActivity.EndShift.shiftID)
                                {
                                    greeting = "You are signed in for " + userState.LatestActivity.StartShift.shiftName + " and meeting.";
                                }
                                else
                                {
                                    greeting = "You are signed in for " + userState.LatestActivity.StartShift.shiftName + " to " + userState.LatestActivity.EndShift.shiftName + " and meeting.";
                                }
                            }
                        }
                        else
                        {
                            if (userState.LatestActivity.StartShift.shiftID == userState.LatestActivity.EndShift.shiftID)
                            {
                                greeting = "You are signed in for " + userState.LatestActivity.StartShift.shiftName + ".";
                            }
                            else
                            {
                                greeting = "You are signed in for " + userState.LatestActivity.StartShift.shiftName + " to " + userState.LatestActivity.EndShift.shiftName + ".";
                            }
                        }
                    }
                }
                else
                {
                    signButton.Content = "Sign in";
                    greeting = "You are not signed in.";
                }
            }
            else
            {
                if (userState.LatestMeeting != null)
                {
                    greeting = "Your attendance for the meeting has been recorded.";
                }
                else
                {
                    greeting = "You are not signed in.";
                    signButton.Content = "Sign in";
                }
            }

            greetingTextBlock.Text = greeting;

            int.TryParse(SettingsHelper.Instance.MinHours, out int minHours);

            if (minHours == 0 || minHours < userState.ApprovedHours)
            {
                totalHoursTextBlock.Text = "You have completed " + userState.ApprovedHours.ToString("0.##") + " hours.";
            }
            else
            { 
                totalHoursTextBlock.Text = "You have completed " + userState.ApprovedHours.ToString("0.##") + " hours out " + minHours.ToString() + " hours minimum.";
            }
        }

        private void specialTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Instance.userState.LatestActivity.NoActivity == true)
            {
                this.Frame.Navigate(typeof(SpecialTask.SignInPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                if (userState.LatestActivity.outTime == DateTime.MinValue && userState.LatestActivity.IsSpecialTask)
                {
                    this.Frame.Navigate(typeof(SpecialTask.SignOutPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                else
                {
                    this.Frame.Navigate(typeof(SpecialTask.SignInPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }

        private void meetingButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Instance.userState.LatestMeeting == null)
            {
                this.Frame.Navigate(typeof(Meeting.SignInPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                if (SettingsHelper.Instance.userState.LatestMeeting.NoActivity == true || SettingsHelper.Instance.userState.LatestMeeting.outTime != DateTime.MinValue)
                {
                    this.Frame.Navigate(typeof(Meeting.SignInPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                else
                {
                    this.Frame.Navigate(typeof(Meeting.SignOutPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }
    }
}
