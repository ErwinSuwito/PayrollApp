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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InitFailed : Page
    {
        public InitFailed()
        {
            this.InitializeComponent();
        }

        unhandledExParam unhandledEx = new unhandledExParam();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var param = (unhandledExParam)e.Parameter;
            if (param.hasCustomMessage == true)
            {
                titleText.Text = param.customTitle;
                subtitleText.Text = param.customSubtitle;
            } 
            else
            {
                subtitleText.Text = param.ErrorMessage;
            }

            unhandledEx = param;
        }

        private async void moreInfo_Click(object sender, RoutedEventArgs e)
        {
            //ContentDialog errorDialog = new ContentDialog
            //{
            //    Title = "Error Details",
            //    Content = unhandledEx.ErrorMessage + "\n" + unhandledEx.StackTrace + "\n" + unhandledEx.Source,
            //    PrimaryButtonText = "Ok"
            //};

            //await errorDialog.ShowAsync();

            message.Text = unhandledEx.ErrorMessage;
            stackTrace.Text = unhandledEx.StackTrace;
            source.Text = "Source: " + unhandledEx.Source;

            await errorDialog.ShowAsync();
        }
    }
}
