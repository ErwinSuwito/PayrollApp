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
            UserState userState = SettingsHelper.Instance.userState;
            string emailContent;

            if (userState != null)
            {
                UserState newUserState = await SettingsHelper.Instance.op.GenerateSignOutInfo(userState);
                Activity newActivity = newUserState.LatestActivity;

                if (newActivity.RequireNotification == true)
                {
                    emailContent = "Dear all, \n " + SettingsHelper.Instance.userState.user.fullName + " has signed out late. Below are the details of the shift.";
                    emailContent += "\n Shift: " + newActivity.EndShift.shiftName + "\n Location: " + SettingsHelper.Instance.appLocation.locationName + "\n Shift ends: ";
                    emailContent += newActivity.EndShift.endTime.ToString() + "\n Actual sign out: " + newActivity.outTime;
                    emailContent += "\n Thank You. \n This is an auto-generated email. Please do not reply to this email.";

                    var message = new Message
                    {
                        Subject = "[Payroll] Sign Out Late " + newActivity.EndShift.shiftName + DateTime.Today.ToShortDateString(),
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
                this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
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
    }
}
