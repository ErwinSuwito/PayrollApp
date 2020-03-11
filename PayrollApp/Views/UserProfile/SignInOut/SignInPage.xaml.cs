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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();
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

                Shift firstItem = (shiftSelectionView.Items[firstIndex] as Shift);

                Shift lastItem = (shiftSelectionView.Items[lastIndex] as Shift);
                int checkingItem = 0;

                foreach (Shift item in shiftSelectionView.Items)
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
                                ContentDialog warningDialog = new ContentDialog
                                {
                                    Title = "Shift selection not valid!",
                                    Content = "You can only sign in for continuous shifts. For example, if you have S1 to S2 and S4, you can only sign in for S1 and S2.",
                                    PrimaryButtonText = "Ok"
                                };

                                await warningDialog.ShowAsync();

                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Checking skipped.");
                    }

                    checkingItem++;
                }
            }
        }

        private void shiftSelectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void shiftSelectionView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
