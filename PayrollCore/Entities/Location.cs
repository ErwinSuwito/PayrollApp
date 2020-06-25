using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PayrollCore.Entities
{
    public class Location : INotifyPropertyChanged
    {
        public int locationID { get; set; }
        public string locationName { get; set; }
        public bool enableGM { get; set; }
        public bool isDisabled { get; set; }
        public bool Shiftless { get; set; }
        public bool isNewLocation { get; set; }
        public string lv_isDisabled { get; set; }
        public string lv_enableGM { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyEventChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void updateLvString()
        {
            if (isDisabled)
            {
                lv_isDisabled = "Disabled";
            }
            else
            {
                lv_isDisabled = "Enabled";
            }

            if (enableGM)
            {
                lv_enableGM = "Yes";
            }
            else
            {
                lv_enableGM = "No";
            }
        }
    }
}
