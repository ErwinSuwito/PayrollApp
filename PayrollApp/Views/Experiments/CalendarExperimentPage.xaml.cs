using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
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

namespace PayrollApp.Views.Experiments
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalendarExperimentPage : Page
    {
        public CalendarExperimentPage()
        {
            this.InitializeComponent();
        }

        IProvider provider = ProviderManager.Instance.GlobalProvider;

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void calendarSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCalendar = calendarSelector.SelectedItem as Calendar;
            TimeSpan timeSpan = new TimeSpan(23, 59, 59);
            DateTime endOfDay = DateTime.Today.AddDays(1) + timeSpan;

            var queryOptions = new List<QueryOption>()
            {
                new QueryOption("startdatetime", DateTime.Now.ToUniversalTime().ToString()),
                new QueryOption("enddatetime", endOfDay.ToUniversalTime().ToString())
            };


            var eventList = await provider.Graph.Me.Calendars[selectedCalendar.Id].CalendarView.Request(queryOptions).GetAsync();


            eventView.ItemsSource = eventList;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (provider != null && provider.State == ProviderState.SignedIn)
            {
                var calendarList = await provider.Graph.Me.Calendars.Request().GetAsync();
                calendarSelector.ItemsSource = calendarList;

                TimeSpan timeSpan = new TimeSpan(23, 59, 59);
                DateTime endOfDay = DateTime.Today.AddDays(1) + timeSpan;

                var queryOptions = new List<QueryOption>()
                {
                    new QueryOption("startdatetime", DateTime.Now.ToUniversalTime().ToString()),
                    new QueryOption("enddatetime", endOfDay.ToUniversalTime().ToString())
                };

                var eventList = await provider.Graph.Me.CalendarView.Request(queryOptions).GetAsync();

                eventView.ItemsSource = eventList;

            }
        }
    }
}
