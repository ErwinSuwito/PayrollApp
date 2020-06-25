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
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.UserProfile.SignInOut
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
        ObservableCollection<PayrollCore.Entities.Shift> shifts;

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

            if (DateTime.Today.DayOfWeek == System.DayOfWeek.Saturday || DateTime.Today.DayOfWeek == System.DayOfWeek.Sunday)
            {
                shifts = await SettingsHelper.Instance.op2.GetUpcomingShifts(SettingsHelper.Instance.appLocation.locationID, true, false);
            }
            else
            {
                shifts = await SettingsHelper.Instance.op2.GetUpcomingShifts(SettingsHelper.Instance.appLocation.locationID, false, false);
            }

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
            bool AllowSignin = false;

            PayrollCore.Entities.Shift startShift;
            PayrollCore.Entities.Shift endShift;

            if (shiftSelectionView.SelectedItems.Count > 1)
            {
                List<PayrollCore.Entities.Shift> selectedShiftList = new List<PayrollCore.Entities.Shift>();

                foreach (PayrollCore.Entities.Shift shift in shiftSelectionView.SelectedItems)
                {
                    selectedShiftList.Add(shift);
                }

                selectedShiftList.Sort((s1, s2) => TimeSpan.Compare(s1.startTime, s2.startTime));
                startShift = selectedShiftList.First();
                endShift = selectedShiftList.Last();

                TimeSpan lastEndTime = new TimeSpan();
                foreach (PayrollCore.Entities.Shift shift in selectedShiftList)
                {
                    Debug.WriteLine("[SHIFT] Checking shift: " + shift.shiftName);
                    if (shift.Equals(startShift))
                    {
                        Debug.WriteLine("[SHIFT] First shift selected: " + shift.shiftName);
                        lastEndTime = shift.endTime;
                    }
                    else
                    {
                        if (lastEndTime != shift.startTime)
                        {
                            break;
                        }
                        else
                        {   
                            lastEndTime = shift.endTime;
                        }
                    }

                    if (shift.Equals(endShift))
                    {
                        Debug.WriteLine("[SHIFT] Last shift selected: " + shift.shiftName);
                        AllowSignin = true;
                    }
                }
            }
            else if (shiftSelectionView.SelectedItems.Count == 1)
            {
                AllowSignin = true;
                startShift = shiftSelectionView.SelectedItem as PayrollCore.Entities.Shift;
                endShift = startShift;
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Please select a shift",
                    Content = "You haven't selected any shift. Please select at least one and try again.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
                return;

            }

            if (AllowSignin)
            {
                var activity = SettingsHelper.Instance.op2.GenerateWorkActivity(SettingsHelper.Instance.userState.user.userID, startShift, endShift);

                bool IsSuccess = await SettingsHelper.Instance.op2.AddNewActivity(activity);

                if (IsSuccess)
                {
                    if (activity.RequireNotification && SettingsHelper.Instance.userState.user.fromAD)
                    {
                        string emailContent;
                        emailContent = "Dear all, \n " + SettingsHelper.Instance.userState.user.fullName + " has signed in late. Below are the details of the shift.";
                        emailContent += "\n Shift: " + activity.StartShift.shiftName + "\n Location: " + SettingsHelper.Instance.appLocation.locationName + "\n Shift start: ";
                        emailContent += activity.StartShift.startTime.ToString() + "\n Actual sign in: " + activity.inTime;
                        emailContent += "\n Thank You. \n This is an auto-generated email. Please do not reply to this email.";

                        var message = new Message
                        {
                            Subject = "[Payroll] Sign In Late " + activity.StartShift.shiftName + " " + DateTime.Today.ToShortDateString(),
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
                                Content = "Please send the Sign In Late email to HR. You are signed in. There is no need to re-sign in. Tap on the More info button to see what failed.",
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
                }
                else
                {
                    ContentDialog warningDialog = new ContentDialog
                    {
                        Title = "Unable to sign in!",
                        Content = "There's a problem preventing us to sign you in. Please try again later. If the problem persits, please contact Chiefs or HR Functional Unit to help you sign in.",
                        PrimaryButtonText = "Ok"
                    };

                    await warningDialog.ShowAsync();
                }

                PayrollCore.Entities.User user = SettingsHelper.Instance.userState.user;
                await SettingsHelper.Instance.UpdateUserState(user);
                this.Frame.Navigate(typeof(UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Invalid selection",
                    Content = "The shifts you selected are invalid. Please make sure that the shifts you selected are back to back.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
                loadGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
