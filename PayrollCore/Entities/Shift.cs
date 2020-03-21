using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollCore.Entities
{
    public class Shift : INotifyPropertyChanged
    {
        public int shiftID { get; set; }
        public string shiftName { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public int locationID { get; set; }
        public Rate DefaultRate { get; set; }
        public bool selected { get; set; }
        public bool isDisabled { get; set; }
        public string dg_isDisabled { get; set; }
        public bool WeekendOnly { get; set; }

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
