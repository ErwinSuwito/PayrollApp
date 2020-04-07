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

namespace PayrollApp.Views.AdminSettings
{
    public sealed partial class FaceApiSettingsDialog : ContentDialog
    {
        public FaceApiSettingsDialog()
        {
            this.InitializeComponent();
            this.DataContext = SettingsHelper.Instance;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SettingsHelper.Instance.CustomFaceApiEndpoint = endpointTextBox.Text;
            SettingsHelper.Instance.FaceApiKey = keyTextBox.Password;

            this.Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SettingsHelper.Instance.CustomFaceApiEndpoint = ClientSecret.FaceApiEndpoint;
            SettingsHelper.Instance.FaceApiKey = ClientSecret.FaceApiKey;

            this.Hide();
        }

        private void padlockBtn_Click(object sender, RoutedEventArgs e)
        {
            if (padlockBtn.IsChecked == true)
            {
                lockStateText.Text = "Unlocked";
                padlockBtn.Content = "\uE1F7";
            }
            else
            {
                lockStateText.Text = "Locked";
                padlockBtn.Content = "\uE1F6";
            }
        }
    }
}
