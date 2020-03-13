// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using PayrollCore;
using PayrollCore.Entities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace PayrollApp
{
    internal class SettingsHelper : INotifyPropertyChanged
    {

        string DbConnectionString;
        string CardConnString;

        public User loggedInUser;
        public Location appLocation;
        public ObservableCollection<Meeting> meetings;
        public DataAccess da;
        public Operations op;
        public string MinHours;

        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public static readonly string CustomEndpointName = "Custom";
        public static readonly string DefaultApiEndpoint = "Custom";

        public static readonly string[] AvailableApiRegions = new string[]
        {
            "westus",
            "westus2",
            "eastus",
            "eastus2",
            "westcentralus",
            "southcentralus",
            "westeurope",
            "northeurope",
            "southeastasia",
            "eastasia",
            "australiaeast",
            "brazilsouth",
            "canadacentral",
            "centralindia",
            "uksouth",
            "japaneast"
        };

        public event EventHandler SettingsChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private static SettingsHelper instance;

        static SettingsHelper()
        {
            instance = new SettingsHelper();
        }

        public async void Initializev2()
        {
            LoadRoamingSettings();
            Windows.Storage.ApplicationData.Current.DataChanged += RoamingDataChanged;
            da = new DataAccess();

            if (localSettings.Values["DbConnString"] != null && localSettings.Values["CardConnString"] != null)
            {
                DbConnectionString = localSettings.Values["DbConnString"].ToString();
                CardConnString = localSettings.Values["CardConnString"].ToString();
                if (da.TestConnString(DbConnectionString) == true && da.TestConnString(CardConnString) == true)
                {
                    da.StoreConnStrings(DbConnectionString, CardConnString);
                    op = new Operations(DbConnectionString, CardConnString);

                    var selLocation = localSettings.Values["selectedLocation"];

                    if (selLocation != null)
                    {
                        string selectedLocation = localSettings.Values["selectedLocation"].ToString();

                        appLocation = da.GetLocationById(selectedLocation);
                        if (appLocation != null && appLocation.isDisabled != true)
                        {
                            MinHours = await da.GetMinHours();

                            if (appLocation.enableGM == true)
                            {
                                meetings = await da.GetMeetings(appLocation);
                            }
                        }
                    }
                }
                else
                {
                    // Clears the saved connection string as it is invalid.
                    localSettings.Values["DbConnString"] = null;
                    localSettings.Values["CardConnString"] = null;
                }
            }
            else
            {
            }
        }

        private void RoamingDataChanged(ApplicationData sender, object args)
        {
            LoadRoamingSettings();
            instance.OnSettingsChanged();
        }

        private void OnSettingsChanged()
        {
            instance.SettingsChanged?.Invoke(instance, EventArgs.Empty);
        }

        private async void OnSettingChanged(string propertyName, object value)
        {
            ApplicationData.Current.RoamingSettings.Values[propertyName] = value;

            instance.OnSettingsChanged();
            instance.OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            instance.PropertyChanged?.Invoke(instance, new PropertyChangedEventArgs(propertyName));
        }

        public static SettingsHelper Instance
        {
            get
            {
                return instance;
            }
        }

        private async void LoadRoamingSettings()
        {
            object value = ApplicationData.Current.RoamingSettings.Values["FaceApiKey"];
            if (value != null)
            {
                this.FaceApiKey = value.ToString();
            }

            value = ApplicationData.Current.RoamingSettings.Values["FaceApiKeyEndpoint"];
            if (value == null && ApplicationData.Current.RoamingSettings.Values["FaceApiKeyRegion"] != null)
            {
                var faceApiRegion = ApplicationData.Current.RoamingSettings.Values["FaceApiKeyRegion"].ToString();
                value = GetRegionEndpoint(faceApiRegion);
            }
            if (value != null)
            {
                this.FaceApiKeyEndpoint = value.ToString();
            }

            value = ApplicationData.Current.RoamingSettings.Values["WorkspaceKey"];
            if (value != null)
            {
                this.WorkspaceKey = value.ToString();
            }

            value = ApplicationData.Current.RoamingSettings.Values["CameraName"];
            if (value != null)
            {
                this.CameraName = value.ToString();
            }

            value = ApplicationData.Current.RoamingSettings.Values["MinDetectableFaceCoveragePercentage"];
            if (value != null)
            {
                uint size;
                if (uint.TryParse(value.ToString(), out size))
                {
                    this.MinDetectableFaceCoveragePercentage = size;
                }
            }

            value = ApplicationData.Current.RoamingSettings.Values["ShowDebugInfo"];
            if (value != null)
            {
                bool booleanValue;
                if (bool.TryParse(value.ToString(), out booleanValue))
                {
                    this.ShowDebugInfo = booleanValue;
                }
            }

            value = ApplicationData.Current.RoamingSettings.Values["CustomFaceApiEndpoint"];
            if (value != null)
            {
                this.CustomFaceApiEndpoint = value.ToString();
            }
        }

        public string GetRegionEndpoint(string region)
        {
            if (!string.IsNullOrEmpty(region) && AvailableApiRegions.Any(x => string.Equals(x, region, StringComparison.OrdinalIgnoreCase)))
            {
                return $"https://{region.ToLower()}.api.cognitive.microsoft.com";
            }
            return DefaultApiEndpoint;
        }

        public void RestoreAllSettings()
        {
            ApplicationData.Current.RoamingSettings.Values.Clear();
        }

        private string faceApiKey = string.Empty;
        public string FaceApiKey
        {
            get { return this.faceApiKey; }
            set
            {
                this.faceApiKey = value;
                this.OnSettingChanged("FaceApiKey", value);
            }
        }

        private string faceApiKeyEndpoint = DefaultApiEndpoint;
        public string FaceApiKeyEndpoint
        {
            get
            {
                return string.Equals(this.faceApiKeyEndpoint, SettingsHelper.CustomEndpointName, StringComparison.OrdinalIgnoreCase)
                    ? this.customFaceApiEndpoint
                    : this.faceApiKeyEndpoint;
            }
            set
            {
                this.faceApiKeyEndpoint = value;
                this.OnSettingChanged("FaceApiKeyEndpoint", value);
            }
        }

        public string BindingFaceApiKeyEndpoint
        {
            get { return this.faceApiKeyEndpoint; }
            set
            {
                this.faceApiKeyEndpoint = value;
                this.OnSettingChanged("FaceApiKeyEndpoint", value);
            }
        }

        private string workspaceKey = string.Empty;
        public string WorkspaceKey
        {
            get { return workspaceKey; }
            set
            {
                this.workspaceKey = value;
                this.OnSettingChanged("WorkspaceKey", value);
            }
        }

        private string cameraName = string.Empty;
        public string CameraName
        {
            get { return cameraName; }
            set
            {
                this.cameraName = value;
                this.OnSettingChanged("CameraName", value);
            }
        }

        private uint minDetectableFaceCoveragePercentage = 7;
        public uint MinDetectableFaceCoveragePercentage
        {
            get { return this.minDetectableFaceCoveragePercentage; }
            set
            {
                this.minDetectableFaceCoveragePercentage = value;
                this.OnSettingChanged("MinDetectableFaceCoveragePercentage", value);
            }
        }

        private bool showDebugInfo = false;
        public bool ShowDebugInfo
        {
            get { return showDebugInfo; }
            set
            {
                this.showDebugInfo = value;
                this.OnSettingChanged("ShowDebugInfo", value);
            }
        }

        private string customFaceApiEndpoint = string.Empty;
        public string CustomFaceApiEndpoint
        {
            get { return this.customFaceApiEndpoint; }
            set
            {
                this.customFaceApiEndpoint = value;
                this.OnSettingChanged("CustomFaceApiEndpoint", value);
            }
        }

        /// <summary>
        /// Generate connection string using the Windows authentication mode.
        /// </summary>
        /// <param name="hostName">The name or IP address of the server</param>
        /// <param name="initialCatalog">The name of the database</param>
        public string GenerateConnectionString(string hostName, string initialCatalog)
        {
            string connString = @"Data Source=" + hostName + "; Initial Catalog=" + initialCatalog + "; Integrated Security=SSPI";

            return connString;
        }

        /// <summary>
        /// Generate connection string using the SQL Authentication mode.
        /// </summary>
        /// <param name="hostName">The name or IP address of the server</param>
        /// <param name="initialCatalog">The name of the database</param>
        /// <param name="userName">Username of the user that has access to the database</param>
        /// <param name="password">Password of the user that has access to the database</param>
        /// <returns></returns>
        public string GenerateConnectionString(string hostName, string initialCatalog, string userName, string password)
        {
            string connString = @"Server=" + hostName + "; Initial Catalog=" + initialCatalog + "; Persist Security Info=False; User ID=" + userName + "; Password=" + password + ";";

            return connString;
        }

        /// <summary>
        /// Saves the connection string to the settings container.
        /// </summary>
        /// <param name="isDbConn">Is the connection string for the main database</param>
        /// <param name="connectionString">The connection string</param>
        /// <returns></returns>
        public bool SaveConnectionString(bool isDbConn, string connectionString)
        {
            try
            {
                if (isDbConn == true)
                {
                    localSettings.Values["DbConnString"] = connectionString;
                }
                else
                {
                    localSettings.Values["CardConnString"] = connectionString;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string[] AvailableApiEndpoints
        {
            get
            {
                return new string[]
                {
                    CustomEndpointName,
                    "https://westus.api.cognitive.microsoft.com",
                    "https://westus2.api.cognitive.microsoft.com",
                    "https://eastus.api.cognitive.microsoft.com",
                    "https://eastus2.api.cognitive.microsoft.com",
                    "https://westcentralus.api.cognitive.microsoft.com",
                    "https://southcentralus.api.cognitive.microsoft.com",
                    "https://westeurope.api.cognitive.microsoft.com",
                    "https://northeurope.api.cognitive.microsoft.com",
                    "https://southeastasia.api.cognitive.microsoft.com",
                    "https://eastasia.api.cognitive.microsoft.com",
                    "https://australiaeast.api.cognitive.microsoft.com",
                    "https://brazilsouth.api.cognitive.microsoft.com",
                    "https://canadacentral.api.cognitive.microsoft.com",
                    "https://centralindia.api.cognitive.microsoft.com",
                    "https://uksouth.api.cognitive.microsoft.com",
                    "https://japaneast.api.cognitive.microsoft.com"
                };
            }
        }
    }
}