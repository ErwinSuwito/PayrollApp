using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    public  class UserState
    {
        public User user { get; set; }
        public MeetingAttendance meetingAttendance { get; set; }
        public SignInOut signInInfo { get; set; }
    }
}
