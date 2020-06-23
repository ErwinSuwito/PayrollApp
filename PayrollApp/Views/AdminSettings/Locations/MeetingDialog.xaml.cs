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
using PayrollCore.Entities;
using System.Threading.Tasks;
using System.Diagnostics;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PayrollApp.Views.AdminSettings.Location
{
    public sealed partial class NewMeetingDialog : ContentDialog
    {

        Meeting meeting;
        int locationID;

        public NewMeetingDialog(Meeting passedMeeting)
        {
            this.InitializeComponent();
            meeting = passedMeeting;
        }

        /// <summary>
        /// Saves the current meeting to database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            formPanel.Visibility = Visibility.Collapsed;
            loadPanel.Visibility = Visibility.Visible;
            this.PrimaryButtonText = "Close";
            this.IsPrimaryButtonEnabled = false;
            this.SecondaryButtonText = "";
            this.CloseButtonText = "";

            bool isSuccess = await SaveChanges();

            if (isSuccess)
            {
                loadPanel.Visibility = Visibility.Collapsed;
                savedPanel.Visibility = Visibility.Visible;
                this.PrimaryButtonText = "";
                this.CloseButtonText = "Close";
            }
            else
            {
                loadPanel.Visibility = Visibility.Collapsed;
                failedPanel.Visibility = Visibility.Visible;
                this.PrimaryButtonText = "";
                this.CloseButtonText = "Close";
            }
        }

        /// <summary>
        /// Update the isDisabled field in the database table.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            formPanel.Visibility = Visibility.Collapsed;
            loadPanel.Visibility = Visibility.Visible;
            this.PrimaryButtonText = "Close";
            this.IsPrimaryButtonEnabled = false;
            this.SecondaryButtonText = "";
            this.CloseButtonText = "";

            meeting.isDisabled = true;

            bool isSuccess = await SaveChanges();

            if (isSuccess)
            {
                loadPanel.Visibility = Visibility.Collapsed;
                savedPanel.Visibility = Visibility.Visible;
                this.PrimaryButtonText = "";
                this.CloseButtonText = "Close";
            }
            else
            {
                loadPanel.Visibility = Visibility.Collapsed;
                failedPanel.Visibility = Visibility.Visible;
                this.PrimaryButtonText = "";
                this.CloseButtonText = "Close";
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (meeting.newMeeting == true)
            {
                this.Title = "New meeting";
                this.SecondaryButtonText = "";
            }
            else
            {
                meeting.newMeeting = false;
                meetingNameTextBox.Text = meeting.meetingName;
                daySelector.SelectedIndex = meeting.meetingDay;
                locationID = meeting.locationID;
            }
        }

        private async Task<bool> SaveChanges()
        {
            Debug.WriteLine("location id: " + meeting.locationID);
            meeting.meetingName = meetingNameTextBox.Text;
            meeting.meetingDay = daySelector.SelectedIndex;
            bool IsSuccess = await SettingsHelper.Instance.da.SaveMeetingSettings(meeting);
            return IsSuccess;
        }
    }
}
