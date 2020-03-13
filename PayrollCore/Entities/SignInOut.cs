﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollCore.Entities
{
    public class SignInOut : INotifyPropertyChanged
    {
        public int loginID { get; set; }
        public string userID { get; set; }
        public DateTime inTime { get; set; }
        public DateTime outTime { get; set; }
        public int startShiftId { get; set; }
        public int endShiftId { get; set; }
        public float approvedHours { get; set; }
        public float claimableAmount { get; set; }
        public bool RequireNotification { get; set; }
        /// <summary>
        /// The reason to send a notification
        /// 1 - Late sign in
        /// 2 - Late sign out
        /// </summary>
        public int NotificationReason { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyEventChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}