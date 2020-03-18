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
            ObservableCollection<PayrollCore.Entities.Shift> shifts = await SettingsHelper.Instance.da.GetShiftsFromLocation(SettingsHelper.Instance.appLocation.locationID.ToString(), false);
            shiftSelectionView.ItemsSource = shifts;

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

            bool IsSelectionValid = CheckUserSelection();

            if (IsSelectionValid == true)
            {
                int firstIndex;
                int lastIndex;

                if (shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.First()) < shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.Last()))
                {
                    firstIndex = shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.First());
                    lastIndex = shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.Last());
                }
                else
                {
                    firstIndex = shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.Last());
                    lastIndex = shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.First());
                }

                PayrollCore.Entities.Shift firstItem = (shiftSelectionView.Items[firstIndex] as PayrollCore.Entities.Shift);

                PayrollCore.Entities.Shift lastItem = (shiftSelectionView.Items[lastIndex] as PayrollCore.Entities.Shift);

                var newActivity = await SettingsHelper.Instance.op.GenerateSignInInfo(SettingsHelper.Instance.userState.user, firstItem, lastItem, SettingsHelper.Instance.appLocation);

                bool IsSuccess = await SettingsHelper.Instance.da.AddNewActivity(newActivity);

                if (IsSuccess)
                {
                    if (newActivity.RequireNotification && SettingsHelper.Instance.userState.user.fromAD)
                    {
                        string emailContent;
                        emailContent = "Dear all, \n " + SettingsHelper.Instance.userState.user.fullName + " has signed in late. Below are the details of the shift.";
                        emailContent += "\n Shift: " + newActivity.StartShift.shiftName + "\n Location: " + SettingsHelper.Instance.appLocation.locationName + "\n Shift start: ";
                        emailContent += newActivity.StartShift.startTime.ToString() + "\n Actual sign in: " + newActivity.inTime;
                        emailContent += "\n Thank You. \n This is an auto-generated email. Please do not reply to this email.";

                        var message = new Message
                        {
                            Subject = "[Payroll] Sign In Late " + newActivity.StartShift.shiftName + DateTime.Today.ToShortDateString(),
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
                        finally
                        {
                            this.Frame.Navigate(typeof(UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                        }

                        //case 2:
                        //    emailContent = "Dear all, \n " + SettingsHelper.Instance.userState.user.fullName + " has signed out late. Below are the details of the shift.";
                        //    emailContent += "\n Shift: " + newActivity.EndShift.shiftName + "\n Location: " + newActivity.location.locationName + "\n Shift ends: ";
                        //    emailContent += newActivity.EndShift.endTime.ToString() + "\n Actual sign out: " + newActivity.outTime;
                        //    emailContent += "\n Thank You. \n This is an auto-generated email. Please do not reply to this email.";

                        //    break;
                    }
                }
                else
                {
                    ContentDialog warningDialog = new ContentDialog
                    {
                        Title = "Unable to sign in!",
                        Content = "There's a problem preventing us to sign you in. Please try again later.",
                        PrimaryButtonText = "Ok"
                    };

                    await warningDialog.ShowAsync();
                }
            }
            else
            {
                ContentDialog warningDialog = new ContentDialog
                {
                    Title = "Shift selection not valid!",
                    Content = "You can only sign in for continuous shifts. For example, if you have S1 to S2 and S4, you can only sign in for S1 and S2.",
                    PrimaryButtonText = "Ok"
                };

                await warningDialog.ShowAsync();
            }

            loadGrid.Visibility = Visibility.Collapsed;

        }

        private bool CheckUserSelection()
        {
            bool IsSelectionValid = false;
            if (shiftSelectionView.SelectedItems.Count > 1)
            {
                int firstIndex;
                int lastIndex;

                if (shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.First()) < shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.Last()))
                {
                    firstIndex = shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.First());
                    lastIndex = shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.Last());
                }
                else
                {
                    firstIndex = shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.Last());
                    lastIndex = shiftSelectionView.Items.IndexOf(shiftSelectionView.SelectedItems.First());
                }

                PayrollCore.Entities.Shift firstItem = (shiftSelectionView.Items[firstIndex] as PayrollCore.Entities.Shift);

                PayrollCore.Entities.Shift lastItem = (shiftSelectionView.Items[lastIndex] as PayrollCore.Entities.Shift);
                int checkingItem = 0;

                foreach (PayrollCore.Entities.Shift item in shiftSelectionView.Items)
                {
                    Debug.WriteLine("checkingItem: " + checkingItem);
                    Debug.WriteLine("Start checking shift: " + item.shiftName);

                    if (checkingItem <= lastIndex && checkingItem >= firstIndex)
                    {
                        Debug.WriteLine("Item is within firstIndex and lastIndex");

                        if (checkingItem == firstIndex || checkingItem == lastIndex)
                        {
                            Debug.WriteLine("Item being checked matches firstIndex or lastIndex");
                            if (item.shiftID == firstItem.shiftID || item.shiftID == lastItem.shiftID)
                            {
                                Debug.WriteLine("Item shiftID matches firstItem or lastItem shiftID");
                            }
                            else
                            {
                                Debug.WriteLine("shiftID does not match");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Item being checked does not match firstIndex or lastIndex");

                            if (item.selected == false)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Checking skipped.");
                    }

                    checkingItem++;
                }

                IsSelectionValid = true;
            }

            return IsSelectionValid;
        }

        private void shiftSelectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
