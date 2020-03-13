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
        string upn;

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

            pageTitle.Text = upn;
        }

        // The code to do actual login is here. To be moved to PayrollCore when
        // The Graph controls are compatible with .NET Standard 2.0
        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();
            bool IsUserEnabled = false;
            if (upn != null)
            {
                User user = await SettingsHelper.Instance.da.GetUserFromDbById(upn);
                if (user != null)
                {
                    pageTitle.Text = user.fullName;

                    bool IsUserADEnabled = await IsUserEnabledAD(upn);
                    if (IsUserADEnabled == true)
                    {
                        // Do login
                    }
                    else
                    {
                        IsUserEnabled = false;
                    }
                }
                else
                {
                    progText.Text = "Please wait. We're setting up your account...";
                    // Do account setup
                }

                if (IsUserEnabled == false)
                {
                    await ShowAccountDisabledMessage();
                    this.Frame.Navigate(typeof(LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
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
            var provider = ProviderManager.Instance.GlobalProvider;

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
    }
}
