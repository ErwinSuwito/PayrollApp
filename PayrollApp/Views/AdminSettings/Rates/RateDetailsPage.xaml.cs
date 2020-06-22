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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.AdminSettings.Rates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RateDetailsPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        Rate rate;
        bool IsNewRate = false;

        public RateDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                rate = e.Parameter as Rate;
            }
            else
            {
                IsNewRate = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            if (IsNewRate == false)
            {
                rateDescBox.Text = rate.rateDesc;
                rateAmountBox.Text = rate.rate.ToString();

                if (rate.isDisabled == true)
                {
                    enableButton.Visibility = Visibility.Visible;
                    deleteButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    enableButton.Visibility = Visibility.Collapsed;
                    deleteButton.Visibility = Visibility.Visible;
                }
            }
            else
            {
                pageTitle.Text = "New rate";
            }
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsNewRate == true)
            {
                rate = new Rate();
            }

            rate.isDisabled = false;

            bool IsSuccess = await SaveLocationInfo();
            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents the rate to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            rate.isDisabled = true;

            bool IsSuccess = await SaveLocationInfo();
            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents the rate to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        async Task<bool> SaveLocationInfo()
        {
            // TO-DO: Add code to validate input (i.e. End time must not be earlier than start time, shift name and rate shouldn't be empty
            bool IsSuccess = false;

            rate.rateDesc = rateDescBox.Text;
            float.TryParse(rateAmountBox.Text, out float newRate);
            rate.rate = newRate;

            if (IsNewRate == false)
            {
                IsSuccess = await SettingsHelper.Instance.op2.UpdateRateAsync(rate);
            }
            else
            {
                IsSuccess = await SettingsHelper.Instance.op2.AddNewRate(rate);
            }
            
            return IsSuccess;
        }

        private async void enableButton_Click(object sender, RoutedEventArgs e)
        {
            rate.isDisabled = false;

            bool IsSuccess = await SaveLocationInfo();
            if (IsSuccess)
            {
                this.Frame.GoBack();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to save settings!",
                    Content = "Something happened that prevents the rate to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }
    }
}
