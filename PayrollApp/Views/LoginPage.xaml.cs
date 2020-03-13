using Microsoft.Toolkit.Graph.Providers;
using System;
using System.Collections.Generic;
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
using PayrollCore.Entities;
using ServiceHelpers;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private Task processingLoopTask;
        private bool isProcessingLoopInProgress;
        private bool isProcessingPhoto;
        private bool isLogginIn = false;

        public LoginPage()
        {
            this.InitializeComponent();

            Window.Current.Activated += CurrentWindowActivationStateChanged;
            this.cameraControl.FilterOutSmallFaces = true;
            this.cameraControl.HideCameraControls();
        }

        private void StartProcessingLoop()
        {
            this.isProcessingLoopInProgress = true;

            if (this.processingLoopTask == null || this.processingLoopTask.Status != TaskStatus.Running)
            {
                this.processingLoopTask = Task.Run(() => this.ProcessingLoop());
            }
        }

        private async void ProcessingLoop()
        {
            while (this.isProcessingLoopInProgress)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    if (!this.isProcessingPhoto)
                    {
                        this.isProcessingPhoto = true;
                        if (this.cameraControl.NumFacesOnLastFrame == 0)
                        {
                            await this.ProcessCameraCapture(null);
                        }
                        else
                        {
                            await this.ProcessCameraCapture(await this.cameraControl.CaptureFrameAsync());
                        }
                    }
                });

                await Task.Delay(this.cameraControl.NumFacesOnLastFrame == 0 ? 100 : 1000);
            }
        }

        private async Task ProcessCameraCapture(ImageAnalyzer e)
        {
            if (e == null)
            {
                this.UpdateUIForNoFacesDetected();
                this.isProcessingPhoto = false;
                return;
            }

            DateTime start = DateTime.Now;

            await e.DetectFacesAsync();

            if (e.DetectedFaces.Any())
            {
                await e.IdentifyFacesAsync();
                //this.greetingTextBlock.Text = this.GetGreettingFromFaces(e);
                if (e.IdentifiedPersons.Any())
                {
                    string upn = e.IdentifiedPersons.First().Person.Name;

                    if (e.DetectedFaces.Count() > e.IdentifiedPersons.Count())
                    {
                        greetingTextBlock.Text = "Hi! One person at a time please...";
                    }
                    else
                    {
                        this.Frame.Navigate(typeof(NewUserOnboarding.RegisterUserPage), upn, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    }
                }
                else
                {
                    if (e.DetectedFaces.Count() > 1)
                    {
                        greetingTextBlock.Text = "Hi! One person at a time please...";
                    }
                    else
                    {
                        greetingTextBlock.Text = "Nice to meet you! Scan your card to get started.";
                    }
                }
            }
            else
            {
                this.UpdateUIForNoFacesDetected();
            }

            TimeSpan latency = DateTime.Now - start;
            string ApiLatency = string.Format("Face API latency: {0}ms", (int)latency.TotalMilliseconds);
            Debug.WriteLine(ApiLatency);

            this.isProcessingPhoto = false;
        }

        private void UpdateUIForNoFacesDetected()
        {
            this.greetingTextBlock.Text = "Step in front of the camera or tap your card to get started.";
            this.greetingTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
        }


        private async void CurrentWindowActivationStateChanged(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if ((e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.CodeActivated ||
                e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.PointerActivated) &&
                this.cameraControl.CameraStreamState == Windows.Media.Devices.CameraStreamState.Shutdown)
            {
                // When our Window loses focus due to user interaction Windows shuts it down, so we 
                // detect here when the window regains focus and trigger a restart of the camera.
                await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: true);
            }
        }

        DispatcherTimer timeUpdater = new DispatcherTimer();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void pageContent_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserProfile.UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private async void DoLogin(string upn)
        {
            PayrollCore.LoginInfoReturn loginInfo;

            if (isLogginIn != true)
            {
                isLogginIn = true;

                bool ADEnabled = await IsUserEnabledAD(upn);
                if (ADEnabled == true)
                {
                    //loginInfo = await SettingsHelper.Instance.op.StartLogin(upn, ADEnabled);
                }
                else
                {
                    
                }

                isLogginIn = false;
            }
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.isProcessingLoopInProgress = false;
            Window.Current.Activated -= CurrentWindowActivationStateChanged;

            await this.cameraControl.StopStreamAsync();
            base.OnNavigatedFrom(e);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: true);
            this.StartProcessingLoop();
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
