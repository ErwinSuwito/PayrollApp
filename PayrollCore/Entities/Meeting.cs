using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PayrollCore.Entities
{
    public class Meeting : INotifyPropertyChanged
    {
        public int meetingID { get; set; }
        public int locationID { get; set; }
        public string meetingName { get; set; }
        public int meetingDay { get; set; }
        public bool isDisabled { get; set; }
        public bool newMeeting { get; set; }
        public TimeSpan StartTime { get; set; }
        public Rate rate { get; set; }

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
