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
using System.Diagnostics;
using Microsoft.Toolkit.Graph.Providers;
using Windows.Media.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PayrollApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = SettingsHelper.Instance;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Background.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/video.mp4"));
            //Background.MediaPlayer.IsLoopingEnabled = true;
            //Background.MediaPlayer.IsMuted = true;
            //Background.MediaPlayer.PlaybackRate = 0.7;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SettingsHelper.Instance.appLocation.isDisabled == false)
                {
                    rootFrame.Navigate(typeof(Views.LoginPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
                else
                {
                    rootFrame.Navigate(typeof(Views.FirstRunSetup.WelcomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
            catch (NullReferenceException)
            {
                rootFrame.Navigate(typeof(Views.FirstRunSetup.WelcomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
            }

            //Background.MediaPlayer.Play();

        }
    }
}
