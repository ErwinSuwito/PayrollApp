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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PayrollApp.Controls
{
    public sealed partial class DatabaseSettingsControl : UserControl
    {
        public bool haveConnString
        {
            get { return (bool)GetValue(haveConnStringProperty); }
            set { SetValue(haveConnStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for haveConnString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty haveConnStringProperty =
            DependencyProperty.Register("haveConnString", typeof(bool), typeof(DatabaseSettingsControl), new PropertyMetadata(0));

        public bool useWinAuth
        {
            get { return (bool)GetValue(useWinAuthProperty); }
            set { SetValue(useWinAuthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for useWinAuth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty useWinAuthProperty =
            DependencyProperty.Register("useWinAuth", typeof(bool), typeof(DatabaseSettingsControl), new PropertyMetadata(0));

        public string connString
        {
            get { return (string)GetValue(connStringProperty); }
            set { SetValue(connStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty connStringProperty =
            DependencyProperty.Register("ConnString", typeof(string), typeof(DatabaseSettingsControl), new PropertyMetadata(0));

        public string dataSource
        {
            get { return (string)GetValue(dataSourceProperty); }
            set { SetValue(dataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for dataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty dataSourceProperty =
            DependencyProperty.Register("dataSource", typeof(string), typeof(DatabaseSettingsControl), new PropertyMetadata(0));

        public string dbName
        {
            get { return (string)GetValue(dbNameProperty); }
            set { SetValue(dbNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for dbName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty dbNameProperty =
            DependencyProperty.Register("dbName", typeof(string), typeof(DatabaseSettingsControl), new PropertyMetadata(0));

        public string sqlUser
        {
            get { return (string)GetValue(sqlUserProperty); }
            set { SetValue(sqlUserProperty, value); }
        }

        // Using a DependencyProperty as the backing store for sqlUser.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty sqlUserProperty =
            DependencyProperty.Register("sqlUser", typeof(string), typeof(DatabaseSettingsControl), new PropertyMetadata(0));

        public string sqlPass
        {
            get { return (string)GetValue(sqlPassProperty); }
            set { SetValue(sqlPassProperty, value); }
        }

        // Using a DependencyProperty as the backing store for sqlPass.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty sqlPassProperty =
            DependencyProperty.Register("sqlPass", typeof(string), typeof(DatabaseSettingsControl), new PropertyMetadata(0));

        public DatabaseSettingsControl()
        {
            this.InitializeComponent();
        }
    }
}
