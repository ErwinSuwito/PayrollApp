using PayrollCore.Entities;
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
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.UserProfile.Meeting
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignInPage : Page
    {
        public SignInPage()
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

            loadTimer.Interval = new TimeSpan(0, 0, 1);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            ObservableCollection<PayrollCore.Entities.Meeting> shifts = await SettingsHelper.Instance.op2.GetMeetings(SettingsHelper.Instance.appLocation.locationID, 
                SettingsHelper.Instance.userState.user.userGroup.groupID, (int)DateTime.Today.DayOfWeek, false);
            shiftSelectionView.ItemsSource = shifts;

            if (shifts.Count < 1)
            {
                noItemsPanel.Visibility = Visibility.Visible;
            }

            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }


        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private async void signInButton_Click(object sender, RoutedEventArgs e)
        {
            loadGrid.Visibility = Visibility.Visible;

            var selectedItem = shiftSelectionView.SelectedItem as PayrollCore.Entities.Meeting;

            if (selectedItem == null)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Please select a meeting",
                    Content = "You haven't selected any meeting. Please select one and try again.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
                return;
            }

            var activity = SettingsHelper.Instance.op2.GenerateMeetingActivity(SettingsHelper.Instance.userState.user.userID, selectedItem);

            bool IsSuccess = await SettingsHelper.Instance.op2.AddNewActivity(activity);

            if (IsSuccess == true)
            {
                if (activity.RequireNotification && SettingsHelper.Instance.userState.user.fromAD)
                {
                    string emailContent;
                    emailContent = "Dear all, \n " + SettingsHelper.Instance.userState.user.fullName + " has signed in late for a meeting. Below are the details of the meeting.";
                    emailContent += "\n Shift: " + activity.meeting.meetingName + "\n Location: " + SettingsHelper.Instance.appLocation.locationName + "\n Meeting start: ";
                    emailContent += activity.inTime.ToShortDateString() + " " + activity.meeting.StartTime.ToString() + "\n Actual sign in: " + activity.inTime;
                    emailContent += "\n Thank You. \n This is an auto-generated email. Please do not reply to this email.";

                    var message = new Message
                    {
                        Subject = "[Payroll] Meeting Late Attendance " + activity.meeting.meetingName + DateTime.Today.ToShortDateString(),
                        Body = new ItemBody
                        {
                            ContentType = BodyType.Text,
                            Content = emailContent
                        },
                        ToRecipients = new List<Recipient>()
                            {
                                new Recipient
                                {
                                    EmailAddress = new EmailAddress
                                    {
                                        Address = "erwin.suwito@cloudmails.apu.edu.my"
                                    }
                                }
                            },
                        CcRecipients = new List<Recipient>()
                            {
                                new Recipient
                                {
                                    EmailAddress = new EmailAddress
                                    {
                                        Address = SettingsHelper.Instance.userState.user.userID
                                    }
                                }
                            }
                    };

                    var saveToItems = false;

                    try
                    {
                        var provider = ProviderManager.Instance.GlobalProvider;

                        if (provider != null && provider.State == ProviderState.SignedIn)
                        {
                            await provider.Graph.Me.SendMail(message, saveToItems).Request().PostAsync();
                        }
                    }
                    catch (Microsoft.Graph.ServiceException graphEx)
                    {
                        ContentDialog contentDialog = new ContentDialog
                        {
                            Title = "Unable to send late sign in notification",
                            Content = "Please notify HR Functional Unit members that you have joined the meeting late. Tap on the More info button to see what failed.",
                            PrimaryButtonText = "Ok",
                            SecondaryButtonText = "More info"
                        };

                        ContentDialogResult result = await contentDialog.ShowAsync();

                        if (result == ContentDialogResult.Secondary)
                        {
                            ContentDialog contentDialog2 = new ContentDialog
                            {
                                Title = "Graph API error",
                                Content = graphEx.Error,
                                PrimaryButtonText = "Close"
                            };

                            await contentDialog2.ShowAsync();
                        }
                    }
                }

                PayrollCore.Entities.User user = SettingsHelper.Instance.userState.user;
                await SettingsHelper.Instance.UpdateUserState(user);
                this.Frame.Navigate(typeof(AttendanceRecodedPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else
            {
                ContentDialog warningDialog = new ContentDialog
                {
                    Title = "Unable to record your attendance.",
                    Content = "There is a problem recording your attendance for the meeting. Please try again later. If the problem persits, please contact Chiefs or HR Functional Unit to help you sign in.",
                    PrimaryButtonText = "Ok"
                };

                await warningDialog.ShowAsync();

                this.Frame.Navigate(typeof(UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }
    }
}
