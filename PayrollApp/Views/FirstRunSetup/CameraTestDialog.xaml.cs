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
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.FirstRunSetup
{
    public sealed partial class CameraTestDialog : ContentDialog
    {
        DispatcherTimer timer = new DispatcherTimer();
        public CameraTestDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = new TimeSpan(0, 0, 6);
            timer.Tick += Timer_Tick;
            timer.Start();
            await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: false);
        }

        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            helpPanel.Visibility = Visibility.Visible;
        }

        private async void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            await this.cameraControl.StopStreamAsync();
        }
    }
}
