using PayrollCore;
using PayrollCore.Entities;
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
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.UserProfile.SignInOut
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignOutPage : Page
    {
        public SignOutPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            timeUpdater.Stop();
            base.OnNavigatedFrom(e);
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

            loadGrid.Visibility = Visibility.Visible;
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            string emailContent;

            if (SettingsHelper.Instance.userState != null)
            {
                Activity activity = SettingsHelper.Instance.op2.CompleteWorkActivity(SettingsHelper.Instance.userState.LatestActivity,
                    SettingsHelper.Instance.userState.user, false);

                bool IsSuccess = await SettingsHelper.Instance.op2.UpdateActivity(activity);
                if (IsSuccess)
                {
                    if (activity.RequireNotification == true)
                    {
                        emailContent = "Dear all, \n " + SettingsHelper.Instance.userState.user.fullName + " has signed out late. Below are the details of the shift.";
                        emailContent += "\n Shift: " + activity.EndShift.shiftName + "\n Location: " + SettingsHelper.Instance.appLocation.locationName + "\n Shift ends: ";
                        emailContent += activity.EndShift.endTime.ToString() + "\n Actual sign out: " + activity.actualOutTime;
                        emailContent += "\n Thank You. \n This is an auto-generated email. Please do not reply to this email.";

                        var message = new Message
                        {
                            Subject = "[Payroll] Sign Out Late " + activity.EndShift.shiftName + DateTime.Today.ToShortDateString(),
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
                                Title = "Signed out with errors",
                                Content = "Please send the Sign Out Late email to HR. Tap on the More info button to see what failed.",
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
                        finally
                        {
                            PayrollCore.Entities.User user = SettingsHelper.Instance.userState.user;
                            await SettingsHelper.Instance.UpdateUserState(user);
                            this.Frame.Navigate(typeof(UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                        }
                    }
                }
                else
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Unable to sign out",
                        Content = "There's a problem signing you out. Please try again later. If the problem persists, please contact Chiefs or HR Functional Unit to sign you out.",
                        PrimaryButtonText = "Ok",
                        SecondaryButtonText = "More info"
                    };

                    await contentDialog.ShowAsync();
                    this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                }

            }
            else
            {
                this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }

            await SettingsHelper.Instance.UpdateUserState(SettingsHelper.Instance.userState.user);

            loadTimer.Stop();
            pageContent.Visibility = Visibility.Visible;
            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            SettingsHelper.Instance.userState = null;
            this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        public Activity GenerateSignOutInfo(Activity activity, PayrollCore.Entities.User user)
        {
            DateTime signInTime = activity.inTime;
            DateTime signOutTime = DateTime.Now;

            if (signInTime.DayOfYear < signOutTime.DayOfYear)
            {
                activity.RequireNotification = true;
                activity.NotificationReason = 2;
                string s = activity.inTime.ToShortDateString() + " " + activity.EndShift.startTime.ToString();
                DateTime.TryParse(s, out signOutTime);
            }
            else
            {
                activity.RequireNotification = false;
            }

            activity.outTime = signOutTime;

            TimeSpan activityOffset = signOutTime.Subtract(signInTime);

            if (user.userGroup.DefaultRate.rate > activity.StartShift.DefaultRate.rate)
            {
                // Use user's default rate
                activity.ApplicableRate = user.userGroup.DefaultRate;
            }
            else
            {
                // Use shift's default rate
                activity.ApplicableRate = activity.StartShift.DefaultRate;
            }

            activity.ClaimableAmount = CalcPay(activityOffset.TotalHours, activity.ApplicableRate.rate);
            activity.ApprovedHours = activityOffset.TotalHours;
            activity.ClaimDate = DateTime.Today;

            return activity;
        }

        public float CalcPay(double hours, float rate)
        {
            return (float)hours * rate;
        }
    }
}

