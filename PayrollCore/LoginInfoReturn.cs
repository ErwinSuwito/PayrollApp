using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    public  class LoginInfoReturn
    {
        public bool IsSuccess { get; set; }
        public bool AllowLogin { get; set; }
        public bool NewUser { get; set; }
        public User user { get; set; }
    }
}
