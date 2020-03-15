using Microsoft.Toolkit.Graph.Providers;
using PayrollCore;
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

namespace PayrollApp.Views.NewUserOnboarding
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterUserPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        IProvider provider = ProviderManager.Instance.GlobalProvider;
        string upn;
        bool AccNotEnabledOrNotFound = false;

        public RegisterUserPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                upn = e.Parameter.ToString();
            }

            base.OnNavigatedTo(e);
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

            pageTitle.Text = upn.ToLower();
        }

        // The code to do actual login is here. To be moved to PayrollCore when
        // The Graph controls are compatible with .NET Standard 2.0
        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            User user;

            if (upn != null)
            {
                user = await SettingsHelper.Instance.da.GetUserFromDbById(upn);
                if (user != null)
                {
                    pageTitle.Text = user.fullName;
                    progText.Text = "Syncing account data...";

                    // User is already registered. Proceed to check if it is from AD and sync their Allow Login status if does not contains TP.
                    if (user.fromAD == true && !user.userID.Contains("TP"))
                    {
                        bool IsEnabledInAd = await IsUserEnabledAD(upn);
                        user.isDisabled = !IsEnabledInAd;

                        Debug.WriteLine(user.isDisabled.ToString());
                        await SettingsHelper.Instance.da.UpdateUserInfo(user);
                    }

                    progText.Text = "Logging you in...";
                    if (user.isDisabled == false)
                    {
                        // Copies user to loggedInUser
                        SettingsHelper.Instance.userState = new UserState();
                        SettingsHelper.Instance.userState.user = user;
                        SettingsHelper.Instance.userState.LatestActivity = await SettingsHelper.Instance.da.GetLatestActivityByUserId(upn, SettingsHelper.Instance.appLocation.locationID);
                        SettingsHelper.Instance.userState.ApprovedHours = await SettingsHelper.Instance.da.GetApprovedHours(upn);
                        this.Frame.Navigate(typeof(UserProfile.UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    }
                    else
                    {
                        ContentDialog contentDialog = new ContentDialog
                        {
                            Title = "Your account is disabled.",
                            Content = "If you believe that your account has been disabled by mistake, contact TA Supervisor, Chiefs, or TA HR Functional Unit to enable your account.",
                            PrimaryButtonText = "Ok"
                        };

                        await contentDialog.ShowAsync();

                        this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    }
                }
                else
                {
                    // User not registered in system yet, proceed to set up user account.
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

        private async Task<bool> IsUserEnabledAD(string upn)
        {
            
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                try
                {
                    var user = await provider.Graph.Users[upn].Request().GetAsync();
                    if (user != null)
                    {
                        if (user.AccountEnabled == true)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Microsoft.Graph.ServiceException graphEx)
                {
                    if (graphEx.Message.Contains("Request_ResourceNotFound"))
                    {
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("Graph Service Exception: " + graphEx.Message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Check user AD Exception: " + ex.Message);
                }
            }

            return false;
        }

        private async Task<bool> ShowAccountDisabledMessage()
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Your account is disabled.",
                Content = "If you believe that your account has been disabled by mistake, contact TA Supervisor, Chiefs, or TA HR Functional Unit to enable your account.",
                PrimaryButtonText = "Ok"
            };

            await contentDialog.ShowAsync();

            return true;
        }

        private async Task<bool> GetUserFromAD(string upn)
        {
            User user = new User();
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                try
                {
                    var adUser = await provider.Graph.Users[upn].Request().GetAsync();
                    if (adUser.AccountEnabled == true)
                    {
                        user.userID = upn;
                        user.fullName = adUser.DisplayName;
                        user.userGroup = new UserGroup();
                        
                    }
                    else
                    {
                        AccNotEnabledOrNotFound = true;
                        return false;
                    }
                }
                catch (Microsoft.Graph.ServiceException graphEx)
                {
                    if (graphEx.Message.Contains("Request_ResourceNotFound"))
                    {
                        AccNotEnabledOrNotFound = true;
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("Graph Service Exception: " + graphEx.Message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Check user AD Exception: " + ex.Message);
                }

            }

            return false;
        }
    }
}
