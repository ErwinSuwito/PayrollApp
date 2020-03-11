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

namespace PayrollApp.Views.AdminSettings.Shifts
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShiftDetailsPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        Shift shift;
        bool IsNewShift = false;

        public ShiftDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                shift = e.Parameter as Shift;
            }
            else
            {
                IsNewShift = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            loadGrid.Visibility = Visibility.Visible;

            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            loadTimer.Interval = new TimeSpan(0, 0, 1);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();

            if (shift != null)
            {
                shiftName.Text = shift.shiftName;
                startTime.SelectedTime = shift.startTime;
                endTime.SelectedTime = shift.endTime;

                if (shift.isDisabled == true)
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
                pageTitle.Text = "New shift";
            }
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();

            // Gets all available rates and assign it as ItemSource for defaultRateBox
            ObservableCollection<Rate> rate = await SettingsHelper.Instance.da.GetAllRates(false);
            defaultRateBox.ItemsSource = rate;

            if (shift != null)
            {
                for (int i = 0; i < rate.Count; i++)
                {
                    var item = rate.ElementAt(i) as Rate;
                    if (item.rateID == shift.DefaultRate.rateID)
                    {
                        defaultRateBox.SelectedIndex = i;
                        break;
                    }
                }
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
            this.Frame.GoBack();
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsNewShift == true)
            {
                shift = new Shift();
            }

            shift.isDisabled = false;

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
                    Content = "Something happened that prevents the shift to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            shift.isDisabled = true;

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
                    Content = "Something happened that prevents the shift to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        async Task<bool> SaveLocationInfo()
        {
            // TO-DO: Add code to validate input (i.e. End time must not be earlier than start time, shift name and rate shouldn't be empty
            bool IsSuccess = false;

            shift.locationID = SettingsHelper.Instance.appLocation.locationID;
            shift.shiftName = shiftName.Text;
            shift.startTime = startTime.SelectedTime.Value;
            shift.endTime = endTime.SelectedTime.Value;
            shift.DefaultRate = defaultRateBox.SelectedItem as Rate;

            if (IsNewShift == false)
            {
                IsSuccess = await SettingsHelper.Instance.da.UpdateShiftInfo(shift);
            }
            else
            {
                IsSuccess = await SettingsHelper.Instance.da.AddNewShift(shift);
            }
            
            return IsSuccess;
        }

        private async void enableButton_Click(object sender, RoutedEventArgs e)
        {
            shift.isDisabled = false;

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
                    Content = "Something happened that prevents the shift to be updated. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }
    }
}
