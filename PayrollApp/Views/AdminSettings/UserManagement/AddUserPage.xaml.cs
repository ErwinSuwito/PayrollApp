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
    public sealed partial class AddUserPage : Page
    {
        User user;
        int groupIndex = 0;
        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();

        public AddUserPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                user = e.Parameter as User;
            }
        }

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

            if (user != null)
            {
                pageTitle.Text = "Change user settings";
                changeSettingsBtn.Visibility = Visibility.Visible;
                usernameBox.Text = user.userID;
                fullNameBox.Text = user.fullName;
                if (user.fromAD)
                {
                    adWarning.Visibility = Visibility.Visible;
                    fullNameBox.IsEnabled = false;
                }
            }
            else
            {
                usernameBox.IsEnabled = true;
                saveAccBtn.Visibility = Visibility.Visible;
            }
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            ObservableCollection<UserGroup> userGroups = await SettingsHelper.Instance.op2.GetUserGroups(GetDisabled: false, GetCompleteData: true);
            userGroupBox.ItemsSource = userGroups;

            if (user != null)
            {
                RefreshGroupIndex();
                userGroupBox.SelectedIndex = groupIndex;
            }

            loadGrid.Visibility = Visibility.Collapsed;
        }

        void RefreshGroupIndex()
        {
            groupIndex = 0;

            foreach (UserGroup userGroup in userGroupBox.Items)
            {
                if (user.userGroup.groupID != userGroup.groupID)
                {
                    groupIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (user == null)
            {
                this.Frame.Navigate(typeof(UserListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
            else
            {
                this.Frame.Navigate(typeof(UserDetailsPage), user, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            }
        }

        private async void changeSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (user.fromAD == true)
            {
                user.userGroup = userGroupBox.SelectedItem as UserGroup;
                bool IsSuccess = await SettingsHelper.Instance.op2.UpdateUser(user);
                if (IsSuccess)
                {
                    this.Frame.Navigate(typeof(UserListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                else
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Unable to save",
                        Content = "Unable to save user settings. Please try again later. You will be brought back to the users list.",
                        PrimaryButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();

                    this.Frame.Navigate(typeof(UserListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }

        private async void saveAccBtn_Click(object sender, RoutedEventArgs e)
        {
            user = new User();
            user.userID = usernameBox.Text;
            user.fullName = fullNameBox.Text;
            user.userGroup = userGroupBox.SelectedItem as UserGroup;
            user.fromAD = false;
            user.isDisabled = false;

            bool IsSuccess = await SettingsHelper.Instance.op2.AddUser(user);
            if (IsSuccess)
            {
                this.Frame.Navigate(typeof(UserListPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to add user",
                    Content = "Unable to add the user. Make sure that the user doesn't have the same username.",
                    PrimaryButtonText = "Ok"
                };

                await contentDialog.ShowAsync();

            }
        }

        private void userGroupBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (user != null && userGroupBox.SelectedIndex != groupIndex)
            {
                groupWarning.Visibility = Visibility.Visible;
            }
            else
            {
                groupWarning.Visibility = Visibility.Collapsed;
            }
        }
    }
}
