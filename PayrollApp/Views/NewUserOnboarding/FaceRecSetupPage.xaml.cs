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
    public sealed partial class FaceRecSetupPage : Page
    {

        DispatcherTimer timeUpdater = new DispatcherTimer();
        DispatcherTimer loadTimer = new DispatcherTimer();
        string username = SettingsHelper.Instance.userState.user.userID;

        public FaceRecSetupPage()
        {
            this.InitializeComponent();
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

            this.cameraControl.FilterOutSmallFaces = true;
        }

        private async void LoadTimer_Tick(object sender, object e)
        {
            loadTimer.Stop();

            bool LoadPersonResult = await SettingsHelper.Instance.LoadRegisteredPeople();
            if (LoadPersonResult == true)
            {
                bool IsUserFound = SettingsHelper.Instance.SelectPeople(username);
                if (IsUserFound != true)
                {
                    bool CreatePersonSuccess = await SettingsHelper.Instance.CreatePersonAsync(username);

                    if (CreatePersonSuccess == true)
                    {
                        loadGrid.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
            }
            
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Unable to register your face at this time.",
                Content = "A problem occurred that prevents us to setup your facial recognition settings. You can register your face by selecting the 'Improve recognition' button after you login to Payroll.",
                CloseButtonText = "Ok"
            };
            
            await contentDialog.ShowAsync();
            this.Frame.Navigate(typeof(UserProfile.UserProfilePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void TimeUpdater_Tick(object sender, object e)
        {
            currentTime.Text = DateTime.Now.ToString("hh:mm tt");
            currentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cameraControl_ImageCaptured(object sender, ServiceHelpers.ImageAnalyzer e)
        {

        }
    }
}
