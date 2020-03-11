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

namespace PayrollApp.Views.AdminSettings.UserManagement
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserDetailsPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        User user;

        public UserDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                user = (e.Parameter as User);
                return;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
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

            LoadUserInfo();
        }

        void LoadUserInfo()
        {
            if (user != null)
            {
                pageTitle.Text = user.fullName;
                usernameTextBlock.Text = user.userID;
                statusTextBlock.Text = user.userGroup.groupName;

                //if (user.userGroup.EnableFaceRec == true)
                //{
                //    faceRecPanel.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    faceRecPanel.Visibility = Visibility.Collapsed;
                //}

                if (user.userGroup.ShowAdminSettings)
                {
                    roleText.Text = "Admin";
                }
                else
                {
                    roleText.Text = "User";
                }

                if (user.fromAD)
                {
                    sourceText.Text = "Active Directory";
                    disableAccBtn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    sourceText.Text = "Local";
                }

                if (user.isDisabled)
                {
                    disabledText.Text = "Disabled";
                }
                else
                {
                    disabledText.Text = "Enabled";
                }
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
            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private async void disableAccBtn_Click(object sender, RoutedEventArgs e)
        {
            loadGrid.Visibility = Visibility.Visible;
            progText.Text = "Making changes...";
            user.isDisabled = true;
            await SettingsHelper.Instance.da.UpdateUserInfo(user);

            progText.Text = "Just a moment...";
            var newUser = await SettingsHelper.Instance.da.GetUserFromDbById(user.userID);

            while (newUser == null)
            {

            }

            user = newUser;

            LoadUserInfo();
            loadGrid.Visibility = Visibility.Collapsed;
        }

        private void changeSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddUserPage), user, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
