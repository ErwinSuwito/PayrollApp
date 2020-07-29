using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.Experiments
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PurgePayrollDbPage : Page
    {
        public PurgePayrollDbPage()
        {
            this.InitializeComponent();
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            progPanel.Opacity = 1;
            progText.Text = "Preparing to purge...";
            
            try
            {
                string purgeScriptPath = @"Assets\dropscript.sql";
                StorageFolder installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile file = await installFolder.GetFileAsync(purgeScriptPath);

                if (File.Exists(file.Path))
                {
                    string script = File.ReadAllText(file.Path);
                    using (SqlConnection conn = new SqlConnection(SettingsHelper.Instance.DbConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            progText.Text = "Purging...";
                            cmd.CommandText = script;
                            int? result = await cmd.ExecuteNonQueryAsync();

                            if (result != null)
                            {
                                ContentDialog contentDialog = new ContentDialog()
                                {
                                    Title = "Database purged!",
                                    Content = "All tables have been dropped.",
                                    CloseButtonText = "Ok"
                                };

                                await contentDialog.ShowAsync();
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Unable to purge",
                    Content = "An application resource cannot be found or is corrupted. Please re-install the application and try again. If the problem persists, contact the developer.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Unable to purge",
                    Content = ex.Message,
                    CloseButtonText = "Ok"
                };
                
                await contentDialog.ShowAsync();
            }
            finally
            {
                progPanel.Opacity = 0;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            progPanel.Opacity = 1;
            progText.Text = "Preparing to initialize...";

            try
            {
                string initScriptPath = @"Assets\InitDb.sql";
                StorageFolder installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile file = await installFolder.GetFileAsync(initScriptPath);

                if (File.Exists(file.Path))
                {
                    string script = File.ReadAllText(file.Path);
                    using (SqlConnection conn = new SqlConnection(SettingsHelper.Instance.DbConnectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            progText.Text = "Initializing...";
                            cmd.CommandText = script;
                            int? result = await cmd.ExecuteNonQueryAsync();

                            if (result != null)
                            {
                                ContentDialog contentDialog = new ContentDialog()
                                {
                                    Title = "Database initialized!",
                                    Content = "Tables have been added.",
                                    CloseButtonText = "Ok"
                                };

                                await contentDialog.ShowAsync();
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Unable to initialize",
                    Content = "An application resource cannot be found or is corrupted. Please re-install the application and try again. If the problem persists, contact the developer.",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Unable to initialize",
                    Content = ex.Message,
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
            finally
            {
                progPanel.Opacity = 0;
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            bool IsSuccess = await SettingsHelper.Instance.op2.TestPayrollDb();
            if (IsSuccess)
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Payroll Db useable",
                    Content = "Important tables and data in Payroll Db is found",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "Initialize Payroll Db",
                    Content = "Database needs to be initialized before being used for Payroll",
                    CloseButtonText = "Ok"
                };

                await contentDialog.ShowAsync();

            }
        }
    }
}
