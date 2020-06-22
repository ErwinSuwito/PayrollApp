using Microsoft.AppCenter.Analytics;
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
                user = await SettingsHelper.Instance.op2.da.GetUserByIdAsync(upn);
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
                        await SettingsHelper.Instance.op2.UpdateUser(user);
                    }

                    progText.Text = "Logging you in...";
                    if (user.isDisabled == false)
                    {
                        bool IsSuccess = await SettingsHelper.Instance.UpdateUserState(user);

                        if (IsSuccess)
                        {
                            if (user.IsNewUser == true && SettingsHelper.Instance.userState.user.userGroup.EnableFaceRec == true)
                            {
                                this.Frame.Navigate(typeof(FaceRecIntroPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                            }
                            else
                            {
                                this.Frame.Navigate(typeof(UserProfile.UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                            }

                            return;
                        }
                        else
                        {
                            ContentDialog contentDialog = new ContentDialog
                            {
                                Title = "Unable to login",
                                Content = "There's a problem that prevents us to log you in. Please try again later. If the problem persists, contact Chiefs or HR Functional Unit to help you sign in.",
                                PrimaryButtonText = "Ok"
                            };

                            await contentDialog.ShowAsync();
                        }
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
                    }
                }
                else
                {
                    // User not registered in system yet, proceed to set up user account.
                    progText.Text = "Setting up your account...";
                    User newUser = await GetUserFromAD(upn);
                    if (newUser != null)
                    {
                        newUser.IsNewUser = true;
                        bool IsSuccess = await SettingsHelper.Instance.op2.User(newUser);

                        if (IsSuccess)
                        {
                            loadTimer.Start();
                            return;
                        }
                        else
                        {
                            ContentDialog contentDialog = new ContentDialog
                            {
                                Title = "Unable to register your account.",
                                Content = "There's a problem in creating your account. Please try again later. If the problem persists, please contact Chiefs or HR Functional Unit to help you login.",
                                PrimaryButtonText = "Ok"
                            };

                            await contentDialog.ShowAsync();
                        }
                    }
                    else
                    {
                        ContentDialog contentDialog = new ContentDialog
                        {
                            Title = "Unable to create your account",
                            Content = "There is a problem in creating your account. Please make sure that your AD account is enabled. Please contact Chiefs or HR Functional Unit to get help.",
                            PrimaryButtonText = "Ok"
                        };

                        await contentDialog.ShowAsync();
                    }
                }
            }

            this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
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

        private async Task<User> GetUserFromAD(string upn)
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
                        user.fromAD = true;
                        user.isDisabled = false;
                        if (user.userID.Contains("mail.apu.edu.my"))
                        {
                            user.userGroup = SettingsHelper.Instance.defaultStudentGroup;
                        }
                        else
                        {
                            user.userGroup = SettingsHelper.Instance.defaultOtherGroup;
                        }

                        return user;
                    }
                    else
                    {
                        AccNotEnabledOrNotFound = true;
                        return null;
                    }
                }
                catch (Microsoft.Graph.ServiceException graphEx)
                {
                    if (graphEx.Message.Contains("Request_ResourceNotFound"))
                    {
                        AccNotEnabledOrNotFound = true;
                        return null;
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

            return null;
        }
    }
}
