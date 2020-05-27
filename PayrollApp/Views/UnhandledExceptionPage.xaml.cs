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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UnhandledExceptionPage : Page
    {
        Exception ex;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                ex = e.Parameter as Exception;
            }
            
            base.OnNavigatedTo(e);
        }

        public UnhandledExceptionPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string errorInfo = "Error occurred on: " + DateTime.Now.ToString("dd/MM/yy H:mm:ss zzz") + "\n";
            if (ex != null)
            {
                errorInfo += "Error message: " + ex.Message + "\n \n";
                errorInfo += "StackTrace: " + ex.StackTrace + "\n \n";
                errorInfo += "Source: " + ex.Source;

                if (ex.Message.Contains("SQL"))
                {
                    chgDbSettingsBtn.Visibility = Visibility.Visible;
                }
            }
            else
            {
                errorInfo += "No exception info available.";
            }

            errorInfoBox.Text = errorInfo;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chgDbSettingsBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
