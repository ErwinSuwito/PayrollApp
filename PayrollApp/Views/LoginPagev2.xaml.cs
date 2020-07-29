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
using Windows.System;
using PayrollCore.Entities;
using ServiceHelpers;
using System.Diagnostics;
using Microsoft.Graph;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPagev2 : Page
    {
        private Task processingLoopTask;
        private bool isProcessingLoopInProgress;
        private bool isProcessingPhoto;
        private bool isLogginIn = false;
        private string cardId = string.Empty;
        IProvider provider = ProviderManager.Instance.GlobalProvider;

        public LoginPagev2()
        {
            this.InitializeComponent();

            Window.Current.Activated += CurrentWindowActivationStateChanged;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            this.cameraControl.FilterOutSmallFaces = true;
            this.cameraControl.HideCameraControls();

            if (Debugger.IsAttached)
            {
                cameraControl.Visibility = Visibility.Visible;
            }
        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            
            if (args.VirtualKey == Windows.System.VirtualKey.Enter && !string.IsNullOrEmpty(cardId))
            {
                string _cardId = cardId;
                cardId = string.Empty;

                string upn = await SettingsHelper.Instance.op2.GetUserIdFromCard(_cardId);
                
                if (string.IsNullOrEmpty(upn))
                {
                    // Checks if username is null or empty and shows the correct error to the user
                    if (upn == null)
                    {
                        ContentDialog contentDialog = new ContentDialog
                        {
                            Title = "Unable to scan your card",
                            Content = "There's a problem when finding your card. Please try again later. Meanwhile, try if your card works in other systems. If it does, contact Supervisor for help.",
                            PrimaryButtonText = "More info",
                            CloseButtonText = "Ok"
                        };

                        ContentDialogResult result = await contentDialog.ShowAsync();
                        
                        // More info button is selected
                        if (result == ContentDialogResult.Primary)
                        {
                            Exception ex = SettingsHelper.Instance.op2.GetLastError();
                            contentDialog = new ContentDialog
                            {
                                Title = "More info",
                                Content = "upn: " + upn + "\n" +  ex.Message,
                                CloseButtonText = "Close"
                            };

                            await contentDialog.ShowAsync();
                        }
                    }
                }
                else
                {
                    this.Frame.Navigate(typeof(NewUserOnboarding.RegisterUserPage), upn, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
            else 
            {
                if (dict.TryGetValue(args.VirtualKey, out int newInt))
                {
                    cardId += newInt.ToString();
                }
            }
        }

        private void StartProcessingLoop()
        {
            this.isProcessingLoopInProgress = true;

            if (this.processingLoopTask == null || this.processingLoopTask.Status != System.Threading.Tasks.TaskStatus.Running)
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
                        brandingTextBlock.Visibility = Visibility.Collapsed;
                        greetingTextBlock.Text = "One person at a time.";
                        greetingTextBlock.Visibility = Visibility.Visible;
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
                        brandingTextBlock.Visibility = Visibility.Collapsed;
                        greetingTextBlock.Text = "One person at a time.";
                        greetingTextBlock.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        brandingTextBlock.Visibility = Visibility.Collapsed;
                        greetingTextBlock.Text = "Scan your card to get started.";
                        greetingTextBlock.Visibility = Visibility.Visible;
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
            brandingTextBlock.Visibility = Visibility.Visible;
            greetingTextBlock.Visibility = Visibility.Collapsed;
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
            currentTime.Text = DateTime.Now.ToString("H:mm");
            currentDate.Text = DateTime.Now.ToString("MMMM dd");
            currentDay.Text = DateTime.Now.ToString("dddd");
            timeUpdater.Interval = new TimeSpan(0, 0, 30);
            timeUpdater.Tick += TimeUpdater_Tick;
            timeUpdater.Start();
            CalendarUpdate();
        }

        private async void CalendarUpdate()
        {
            if (SettingsHelper.Instance.enableCalendar)
            {
                if (provider != null && provider.State == ProviderState.SignedIn)
                {
                    TimeSpan timeSpan = new TimeSpan(23, 59, 59);
                    DateTime endOfDay = DateTime.Today.AddDays(1) + timeSpan;

                    var queryOptions = new List<QueryOption>()
                    {
                        new QueryOption("startdatetime", DateTime.Now.ToUniversalTime().ToString()),
                        new QueryOption("enddatetime", endOfDay.ToUniversalTime().ToString())
                    };

                    var eventList = await provider.Graph.Me.Calendars[SettingsHelper.Instance.calendarId].CalendarView.Request(queryOptions).GetAsync();

                    if (eventList != null)
                    {
                        Event firstEvent = eventList.FirstOrDefault();
                        if (firstEvent != null)
                        {
                            eventTitle.Text = firstEvent.Subject;
                            calendarPanel.Visibility = Visibility.Visible;
                        }

                        return;
                    }
                }
            }

            calendarPanel.Visibility = Visibility.Collapsed;
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            CalendarUpdate();

            currentTime.Text = DateTime.Now.ToString("H:mm");
            currentDate.Text = DateTime.Now.ToString("MMMM dd");
            currentDay.Text = DateTime.Now.ToString("dddd");
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.isProcessingLoopInProgress = false;
            Window.Current.Activated -= CurrentWindowActivationStateChanged;
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;

            await this.cameraControl.StopStreamAsync();
            base.OnNavigatedFrom(e);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: true);
            this.StartProcessingLoop();
        }

        private async void footerContent_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ContentDialogResult result = await newAccountDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                this.Frame.Navigate(typeof(NewUserOnboarding.RegisterUserPage), emailBox.Text, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }

            newAccountDialog.Hide();
        }

        private Dictionary<VirtualKey, int> dict = new Dictionary<VirtualKey, int>
        {
            { VirtualKey.Number0, 0},
            {VirtualKey.NumberPad0, 0 },
            { VirtualKey.Number1, 1},
            {VirtualKey.NumberPad1, 1 },
            { VirtualKey.Number2, 2},
            {VirtualKey.NumberPad2, 2 },
            { VirtualKey.Number3, 3},
            {VirtualKey.NumberPad3, 3 },
            { VirtualKey.Number4, 4},
            {VirtualKey.NumberPad4, 4 },
            { VirtualKey.Number5, 5},
            {VirtualKey.NumberPad5, 5 },
            { VirtualKey.Number6, 6},
            {VirtualKey.NumberPad6, 6 },
            { VirtualKey.Number7, 7},
            {VirtualKey.NumberPad7, 7 },
            { VirtualKey.Number8, 8},
            {VirtualKey.NumberPad8, 8 },
            { VirtualKey.Number9, 9},
            {VirtualKey.NumberPad9, 9 }
        };
    }
}
