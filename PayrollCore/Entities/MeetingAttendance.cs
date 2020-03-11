using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PayrollCore.Entities
{
    public class MeetingAttendance : INotifyPropertyChanged
    {
        public int Attendance_ID { get; set; }
        public string UserID { get; set; }
        public int meetingID { get; set; }
        public DateTime timeIn { get; set; }
        public DateTime timeOut { get; set; }

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
