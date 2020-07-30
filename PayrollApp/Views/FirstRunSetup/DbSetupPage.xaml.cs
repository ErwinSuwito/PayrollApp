
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class DbSetupPage : Page
    {
        public DbSetupPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                logoutButton.Visibility = Visibility.Visible;
                appNameText.Visibility = Visibility.Collapsed;
            }

            base.OnNavigatedTo(e);
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

        private async void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            string connString;

            if (dbSettingsControl.haveConnString)
            {
                connString = dbSettingsControl.connString;
            }
            else
            {
                if (dbSettingsControl.useWinAuth)
                {
                    connString = SettingsHelper.Instance.GenerateConnectionString(dbSettingsControl.dataSource, dbSettingsControl.dbName);
                }
                else
                {
                    connString = SettingsHelper.Instance.GenerateConnectionString(dbSettingsControl.dataSource, dbSettingsControl.dbName, dbSettingsControl.sqlUser, dbSettingsControl.sqlPass);
                }
            }

            bool CanConnect = await SettingsHelper.Instance.op2.TestDbConnection(connString);

            if (CanConnect)
            {
                bool IsDbUseable = await SettingsHelper.Instance.op2.TestPayrollDb(connString);
                if (IsDbUseable)
                {
                    SettingsHelper.Instance.SaveConnectionString(true, connString);

                    if (appNameText.Visibility == Visibility.Collapsed)
                    {
                        SettingsHelper.Instance.userState = null;
                        this.Frame.Navigate(typeof(AppInitPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    }
                    else
                    {
                        this.Frame.Navigate(typeof(CardDbSetupPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    }
                }
                else
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "Initialize database?",
                        Content = "Important data is not found in the database. If this is a new database, select initialize database. This will delete drop all tables and initialize the database for usage with apSHA.",
                        PrimaryButtonText = "Initialize",
                        CloseButtonText = "Cancel"
                    };

                    ContentDialogResult result = await contentDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        loadGrid.Visibility = Visibility.Visible;
                        progText.Text = "Preparing to initialize database...";
                        try
                        {
                            string purgeScriptPath = @"Assets\dropscript.sql";
                            string initDbScriptPath = @"Assets\InitDb.sql";

                            StorageFolder installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                            StorageFile purgeScriptFile = await installFolder.GetFileAsync(purgeScriptPath);
                            StorageFile initDbScriptFile = await installFolder.GetFileAsync(initDbScriptPath);

                            string purgeScript = System.IO.File.ReadAllText(purgeScriptFile.Path);
                            string initDbScript = System.IO.File.ReadAllText(initDbScriptFile.Path);

                            progText.Text = "Dropping all tables...";

                            bool IsSuccess = await SettingsHelper.Instance.op2.ExecuteScript(connString, purgeScript);

                            if (IsSuccess)
                            {
                                progText.Text = "Initializing database...";
                                IsSuccess = await SettingsHelper.Instance.op2.ExecuteScript(connString, initDbScript);

                                if (IsSuccess)
                                {
                                    this.Frame.Navigate(typeof(InitializeDb.WelcomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                                }
                                else
                                {
                                    loadGrid.Visibility = Visibility.Collapsed;
                                    ContentDialog contentDialog1 = new ContentDialog()
                                    {
                                        Title = "Unable to create tables",
                                        Content = "An error occurred when creating tables in the database. Make sure that you have the permission to create tables in the database. Try again later.",
                                        CloseButtonText = "Ok"
                                    };

                                    await contentDialog1.ShowAsync();
                                }
                            }
                            else
                            {
                                loadGrid.Visibility = Visibility.Collapsed;
                                ContentDialog contentDialog1 = new ContentDialog()
                                {
                                    Title = "Unable to drop tables",
                                    Content = "An error occurred when creating tables in the database. Make sure that you have the permission to drop tables in the database. Try again later.",
                                    CloseButtonText = "Ok"
                                };

                                await contentDialog1.ShowAsync();

                            }
                        }
                        catch (Exception ex)
                        {
                            loadGrid.Visibility = Visibility.Collapsed;
                            ContentDialog contentDialog1 = new ContentDialog()
                            {
                                Title = "Unable to initialize database",
                                Content = "An error occurred when initializing the database. Please try again later. Select more info to see what's wrong.",
                                PrimaryButtonText = "More info",
                                CloseButtonText = "Ok"
                            };
                            var cdResult = await contentDialog1.ShowAsync();
                            if (cdResult == ContentDialogResult.Primary)
                            {
                                contentDialog1 = new ContentDialog()
                                {
                                    Title = "More info",
                                    Content = ex.Message,
                                    CloseButtonText = "Ok"
                                };

                                await contentDialog1.ShowAsync();
                            }
                        }
                    }
                }
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Unable to connect to database!",
                    Content = "We can't connect to the database based on the specified info. Make sure that this device has network connection and that the database is reachable. Click on More info to see what's wrong.",
                    PrimaryButtonText = "More info",
                    CloseButtonText = "Ok"
                };

                ContentDialogResult result = await contentDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    Exception ex = SettingsHelper.Instance.op2.GetLastError();
                    contentDialog = new ContentDialog
                    {
                        Title = "More info",
                        Content = ex.Message,
                        CloseButtonText = "Close"
                    };

                    await contentDialog.ShowAsync();
                }
            }
        }

        private void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
