using Microsoft.Toolkit.Graph.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DebugModePage : Page
    {
        public DebugModePage()
        {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(540, 740);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        DispatcherTimer loadTimer = new DispatcherTimer();

        private async void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            await provider.LogoutAsync();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["selectedLocation"] = null;
            localSettings.Values["DbConnString"] = null;
            localSettings.Values["CardConnString"] = null;
            SettingsHelper.Instance.FaceApiKey = "";
            SettingsHelper.Instance.CustomFaceApiEndpoint = "";
            SettingsHelper.Instance.Initializev2();

            (Window.Current.Content as Frame).Navigate(typeof(MainPage), null);
        }

        private async void checkPersonBtn_Click(object sender, RoutedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                try
                {
                    var user = await provider.Graph.Users[personEmailBox.Text].Request().GetAsync();
                    if (user != null)
                    {
                        if (user.AccountEnabled == true)
                        {
                            personResultText.Text = "User account is enabled";
                        }
                        else
                        {
                            personResultText.Text = "User account is disabled";
                        }
                    }
                }
                catch (Microsoft.Graph.ServiceException graphEx)
                {
                    if (graphEx.Message.Contains("Request_ResourceNotFound"))
                    {
                        personResultText.Text = "Not found";
                    }
                    else
                    {
                        personResultText.Text = graphEx.Message;
                    }
                }
                catch (Exception ex)
                {
                    personResultText.Text = ex.Message;
                }

            }
        }

        private void faceIdSetupBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminSettings.FaceSetup.FaceIdentificationSetup), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void adminSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminSettings.NewSettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void shiftsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Experiments.ViewShiftsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            loadTimer.Interval = new TimeSpan(0, 0, 5);
            loadTimer.Tick += LoadTimer_Tick;
            loadTimer.Start();
        }

        private void LoadTimer_Tick(object sender, object e)
        {
            throw new NotImplementedException();
        }
    }
}
