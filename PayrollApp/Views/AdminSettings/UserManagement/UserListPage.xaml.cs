using Microsoft.Graph;
using PayrollApp.GroupList;
using PayrollCore.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
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
    public sealed partial class UserListPage : Page
    {
        public UserListPage()
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

            // Make loadGrid to visible when loading location data.
            // Starts timer that will get data on first tick.
            loadTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();

            loadGrid.Visibility = Visibility.Visible;
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            UsersCVS.Source = await GetUserGroupsAsync();
            Bindings.Update();

            loadTimer.Stop();
            loadGrid.Visibility = Visibility.Collapsed;
        }

        async Task<ObservableCollection<GroupedUsers>> GetUserGroupsAsync()
        {
            ObservableCollection<GroupedUsers> groups = new ObservableCollection<GroupedUsers>();
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList();
            var users = await SettingsHelper.Instance.op2.GetAllUsers(true, true);

            var groupByAlpha = from letter in letters
                               select new
                               { 
                                   Key = letter.ToString(),

                                   query = from item in users
                                           where item.fullName.StartsWith(letter.ToString(), StringComparison.CurrentCultureIgnoreCase)
                                           orderby item.fullName
                                           select item
                               };

            foreach (var g in groupByAlpha)
            {
                GroupedUsers groupedUsers = new GroupedUsers();
                groupedUsers.Key = g.Key;

                foreach (var item in g.query)
                {
                    groupedUsers.Add(item);
                }

                groups.Add(groupedUsers);
            }

            return groups;
        }

        async Task<ObservableCollection<GroupedUsers>> GetUsersGroupedAsync()
        {
            ObservableCollection<GroupedUsers> groups = new ObservableCollection<GroupedUsers>();

            var query = from item in await SettingsHelper.Instance.op2.GetAllUsers(true, true)
                        group item by item.fullName.Substring(0, 1).ToUpper() into g
                        orderby g.Key
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                GroupedUsers info = new GroupedUsers();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }

                groups.Add(info);
            }

            return groups;
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddUserPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void userListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PayrollCore.Entities.User user = e.ClickedItem as PayrollCore.Entities.User;
            this.Frame.Navigate(typeof(UserDetailsPage), user, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
