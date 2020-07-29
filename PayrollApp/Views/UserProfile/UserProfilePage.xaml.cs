using Microsoft.Graph;
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            timeUpdater.Stop();
            base.OnNavigatedFrom(e);
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 10);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            // Make changes to the UI according to settings and user state
            ModifyUiV2();
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            //SettingsHelper.Instance.userState = null;
            //this.Frame.Navigate(typeof(LoginPagev2), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void signButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsHelper.Instance.appLocation.Shiftless == true)
            {
                if (SettingsHelper.Instance.userState.LatestActivity.NoActivity == true)
                {
                    this.Frame.Navigate(typeof(SpecialTask.SignInPage), false, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                else
                {
                    if (SettingsHelper.Instance.userState.LatestActivity.outTime == DateTime.MinValue)
                    {
                        this.Frame.Navigate(typeof(SpecialTask.SignOutPage), false, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    }
                    else
                    {
                        this.Frame.Navigate(typeof(SpecialTask.SignInPage), false, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    }
                }
            }
            else
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
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsHelper.Instance.userState = null;
            this.Frame.Navigate(typeof(LoginPagev2), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
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
                    if (userState.LatestActivity.StartShift.shiftName == "Special Task")
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
                signButton.Content = "Sign in";
                if (userState.LatestMeeting.outTime != DateTime.MinValue || userState.LatestMeeting.NoActivity != true)
                {
                    greeting = "Your attendance for the meeting has been recorded.";
                }
                else
                {
                    greeting = "You are not signed in.";
                }
            }

            if (userState.LatestActivity.StartShift != null)
            {
                if (userState.LatestActivity.StartShift.shiftName == "Normal sign in")
                {
                    greeting = "You are signed in.";
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

        private void ModifyUiV2()
        {
            fullNameTextBlock.Text = userState.user.fullName;
            string greeting = string.Empty;

            // Starts modifying UI
            if (SettingsHelper.Instance.appLocation.enableGM == false)
            {
                meetingButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                meetingButton.Visibility = Visibility.Visible;
            }

            if (userState.user.userGroup.ShowAdminSettings == true)
            {
                adminSettingsButton.Visibility = Visibility.Visible;
            }

            if (userState.LatestActivity != null)
            {
                if (userState.LatestActivity.outTime == DateTime.MinValue)
                {
                    if (!userState.LatestActivity.NoActivity)
                    {
                        if (userState.LatestActivity.StartShift.shiftName == "Special Task")
                        {
                            greeting = "You are signed in for special task";
                            signButton.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            signButton.Visibility = Visibility.Visible;
                            signButton.Content = "Sign out";
                            specialTaskButton.Visibility = Visibility.Collapsed;
                            if (userState.LatestActivity.StartShift.shiftName == "Normal sign in")
                            {
                                greeting = "You are signed in";
                            }
                            else
                            {
                                if (userState.LatestActivity.StartShift.shiftID == userState.LatestActivity.EndShift.shiftID)
                                {
                                    greeting = "You are signed in for " + userState.LatestActivity.StartShift.shiftName;
                                }
                                else
                                {
                                    greeting = "You are signed in from " + userState.LatestActivity.StartShift.shiftName + " to " + userState.LatestActivity.EndShift.shiftName;
                                }
                            }
                        }
                    }
                    else
                    {
                        greeting = "You are not signed in";
                        signButton.Visibility = Visibility.Visible;
                        signButton.Content = "Sign in";
                    }
                }
                else
                {
                    greeting = "You are not signed in";
                    signButton.Visibility = Visibility.Visible;
                    signButton.Content = "Sign in";
                }
            }
            else
            {
                greeting = "You are not signed in";
                signButton.Visibility = Visibility.Visible;
                signButton.Content = "Sign in";
            }

            if (userState.LatestMeeting != null)
            {
                if (userState.LatestMeeting.outTime == DateTime.MinValue)
                {
                    if (!userState.LatestMeeting.NoActivity)
                    {
                        if (string.IsNullOrEmpty(greeting))
                        {
                            greeting = "Your meeting attendance has been recorded";
                        }
                        else
                        {
                            if (greeting == "You are not signed in")
                            {
                                greeting = "Your meeting attendance has been recorded";
                            }
                            else
                            {
                                greeting += " and for a meeting";
                            }
                        }
                    }
                }
            }

            greeting += ".";

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
                this.Frame.Navigate(typeof(SpecialTask.SignInPage), true, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                if (userState.LatestActivity.outTime == DateTime.MinValue && userState.LatestActivity.IsSpecialTask)
                {
                    this.Frame.Navigate(typeof(SpecialTask.SignOutPage), true, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                else
                {
                    this.Frame.Navigate(typeof(SpecialTask.SignInPage), true, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
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
