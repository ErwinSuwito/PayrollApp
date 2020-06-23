using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollCore.Entities
{
    public class UserGroup : INotifyPropertyChanged
    {
        public int groupID { get; set; }
        public string groupName { get; set; }
        public Rate DefaultRate { get; set; }
        public bool ShowAdminSettings { get; set; }
        public bool EnableFaceRec { get; set; }
        public bool IsDisabled { get; set; }

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
