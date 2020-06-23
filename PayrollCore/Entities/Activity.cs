using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollCore.Entities
{
    public class Activity : INotifyPropertyChanged
    {
        public int ActivityID { get; set; }
        public string userID { get; set; }
        public int locationID { get; set; }
        public DateTime inTime { get; set; }
        public DateTime outTime { get; set; }
        public DateTime actualOutTime { get; set; }
        public Shift StartShift { get; set; }
        public Shift EndShift { get; set; }
        public Meeting meeting { get; set; }
        public bool IsSpecialTask { get; set; }
        public double ApprovedHours { get; set; }
        public float ClaimableAmount { get; set; }
        public Rate ApplicableRate { get; set; }
        public DateTime ClaimDate { get; set; }
        public bool RequireNotification { get; set; }
        public bool NoActivity { get; set; }

        /// <summary>
        /// The reason to send a notification
        /// 1 - Late sign in
        /// 2 - Late sign out
        /// </summary>
        public int NotificationReason { get; set; }

        public NotifyReason notifyReason { get; set; }

        public enum NotifyReason
        {
            LateSignIn,
            LateSignOut,
            EarlySignOut
        }

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
