﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollApp
{
    class unhandledExParam
    {
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string TargetSite { get; set; }
        public bool hasCustomMessage { get; set; }
        public string customTitle { get; set; }
        public string customSubtitle { get; set; }
    }
}
