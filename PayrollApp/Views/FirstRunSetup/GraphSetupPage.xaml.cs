
using Microsoft.Toolkit.Graph.Providers;
using System;
using System.Collections.Generic;
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

namespace PayrollApp.Views.FirstRunSetup
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GraphSetupPage : Page
    {
        public GraphSetupPage()
        {
            this.InitializeComponent();
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();
        

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();

            var provider = ProviderManager.Instance.GlobalProvider;
            provider.StateChanged += Provider_StateChanged;

            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                var user = await provider.Graph.Me.Request().GetAsync();
                if (ClientSecret.AcceptableEmails.Contains(user.UserPrincipalName))
                {
                    nextBtn.IsEnabled = true;
                }
                else
                {
                    await provider.LogoutAsync();
                    nextBtn.IsEnabled = false;
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Access denied",
                        Content = "Your account is not approved to be used on this app. Please login with another account.",
                        PrimaryButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                }
            }
            else
            {
                nextBtn.IsEnabled = false;
            }

        }

        private async void Provider_StateChanged(object sender, StateChangedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                var user = await provider.Graph.Me.Request().GetAsync();
                if (ClientSecret.AcceptableEmails.Contains(user.UserPrincipalName))
                {
                    nextBtn.IsEnabled = true;
                }
                else
                {
                    await provider.LogoutAsync();
                    nextBtn.IsEnabled = false;
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Access denied",
                        Content = "Your account is not approved to be used on this app. Please login with another account.",
                        PrimaryButtonText = "Ok"
                    };

                    await contentDialog.ShowAsync();
                }
            }
            else
            {
                nextBtn.IsEnabled = false;
            }
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private async void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DbSetupPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

    }
}
