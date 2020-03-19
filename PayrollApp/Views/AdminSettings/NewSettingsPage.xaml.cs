using Microsoft.Toolkit.Graph.Providers;
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
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.AdminSettings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewSettingsPage : Page
    {
        public NewSettingsPage()
        {
            this.InitializeComponent();
            this.DataContext = SettingsHelper.Instance;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        int locationIndex = 0;
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            // Make loadGrid to visible when loading location data.
            // Starts timer that will get data on first tick.
            loadTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();

            loadGrid.Visibility = Visibility.Visible;

            SettingsHelper.Instance.Initializev2();

            if (SettingsHelper.Instance.appLocation == null || SettingsHelper.Instance.appLocation.isDisabled == true)
            {
                loadTimer.Stop();
                this.Frame.Navigate(typeof(FirstRunSetup.WelcomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            provider.StateChanged += Provider_StateChanged;
        }

        private async void Provider_StateChanged(object sender, StateChangedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                var user = await provider.Graph.Me.Request().GetAsync();
                if (user.UserPrincipalName == "ta@cloudmails.apu.edu.my" || user.UserPrincipalName == "TADev@cloudmails.apu.edu.my")
                {
                    logoutButton.IsEnabled = true;
                }
                else
                {
                    logoutButton.IsEnabled = true;
                    //ContentDialog contentDialog = new ContentDialog
                    //{
                    //    Title = "Access denied",
                    //    Content = "You are not signed in as E-Docs. Please sign in again as E-Docs to proceed.",
                    //    PrimaryButtonText = "Ok"
                    //};

                    //await contentDialog.ShowAsync();
                }
            }
            else
            {
                logoutButton.IsEnabled = false;
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "You're logged out!",
                    Content = "Please sign in again as E-Docs to proceed.",
                    PrimaryButtonText = "Ok"
                };
                logoutButton.IsEnabled = false;
            }

        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();

            // Loads the app location settings and get the appropriate index for locationSelector
            ObservableCollection<PayrollCore.Entities.Location> getLocation = await SettingsHelper.Instance.da.GetLocations(false);
            locationSelector.ItemsSource = getLocation;

            refreshLocationIndex();
            locationSelector.SelectedIndex = locationIndex;

            // Gets the latest setting for minimum hours.
            minHoursBox.Text = SettingsHelper.Instance.MinHours;

            // Gets all user groups and the default user group for student and all other accounts
            ObservableCollection<UserGroup> userGroups = await SettingsHelper.Instance.da.GetAllUserGroups();
            defaultTraineeGroup.ItemsSource = userGroups;
            defaultOtherGroup.ItemsSource = userGroups;

            int index = refreshStudentIndex();
            //Debug.WriteLine("Student index: " + index.ToString());
            defaultTraineeGroup.SelectedIndex = index;

            index = refreshOthersIndex();
            //Debug.WriteLine("Others index: " + index.ToString());
            defaultOtherGroup.SelectedIndex = index;

            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            var lastPage = Frame.BackStack.Last();

            if (lastPage != null && lastPage.SourcePageType.Equals(typeof(UserProfile.UserProfilePage)))
            {
                this.Frame.Navigate(typeof(UserProfile.UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else
            {
                this.Frame.Navigate(typeof(DebugModePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        private void manageLocationText_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Location.LocationListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void useDefaultFaceApi_Click(object sender, RoutedEventArgs e)
        {
            faceApiEndpointTextBox.Text = ClientSecret.FaceApiEndpoint;
            faceApiKeyTextBox.Password = ClientSecret.FaceApiKey;
        }

        private void saveLocationBtn_Click(object sender, RoutedEventArgs e)
        {
            localSettings.Values["selectedLocation"] = (locationSelector.SelectedItem as PayrollCore.Entities.Location).locationID;
            SettingsHelper.Instance.Initializev2();

            if (SettingsHelper.Instance.appLocation != null || SettingsHelper.Instance.appLocation.isDisabled == false)
            {
                changeText.Visibility = Visibility.Collapsed;
                refreshLocationIndex();
            }
            else
            {
                this.Frame.Navigate(typeof(FirstRunSetup.WelcomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
        }

        private void openUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserManagement.UserListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void openShiftsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Shifts.ShiftsListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private async void resetBtn_Click(object sender, RoutedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            await provider.LogoutAsync();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["selectedLocation"] = null;
            localSettings.Values["DbConnString"] = null;
            localSettings.Values["CardConnString"] = null;
            SettingsHelper.Instance.FaceApiKey = "";
            SettingsHelper.Instance.CustomFaceApiEndpoint = "";

            App.Current.Exit();
        }

        private int refreshStudentIndex()
        {
            int i = 0;

            foreach (UserGroup group in defaultTraineeGroup.Items)
            {
                Debug.WriteLine(group.groupID.ToString());
                if (SettingsHelper.Instance.defaultStudentGroup.groupID != group.groupID)
                {
                    i++;
                }
                else
                {
                    break;
                }
            }

            return i;
        }

        private int refreshOthersIndex()
        {
            int i = 0;

            foreach (UserGroup group in defaultOtherGroup.Items)
            {
                if (SettingsHelper.Instance.defaultOtherGroup.groupID != group.groupID)
                {
                    i++;
                }
                else
                {
                    break;
                }
            }

            return i;
        }


        private void refreshLocationIndex()
        {
            // resets locationIndex
            locationIndex = 0;

            foreach (PayrollCore.Entities.Location location in locationSelector.Items)
            {
                if (int.Parse(localSettings.Values["selectedLocation"].ToString()) != location.locationID)
                {
                    locationIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        private void locationSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (locationSelector.SelectedIndex != locationIndex)
            {
                changeText.Visibility = Visibility.Visible;
            }
            else
            {
                changeText.Visibility = Visibility.Collapsed;
            }
        }

        private void openRatesBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Rates.RateListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void editUserGroupsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserGroups.UserGroupListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void dbConnSettingsBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void saveMinHoursBtn_Click(object sender, RoutedEventArgs e)
        {
            bool IsSuccess = await SettingsHelper.Instance.da.UpdateGlobalSetting("MinHours", minHoursBox.Text);

            if (IsSuccess == false)
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Can't save",
                    Content = "There is a problem that prevents this setting to be saved. Please try again later.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Minimum Hours updated",
                    Content = "Minimum hours has been updated. Other locations will also use this settings.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
        }

        private void saveDefaultGroup_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
