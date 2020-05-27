using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollCore.Entities
{
    public class User : INotifyPropertyChanged
    {
        public string userID { get; set; }
        public string fullName { get; set; }
        public bool fromAD { get; set; }
        public bool isDisabled { get; set; }
        public UserGroup userGroup { get; set; }
        public bool IsNewUser { get; set; }

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
