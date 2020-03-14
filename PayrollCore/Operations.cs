using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    public class Operations
    {
        DataAccess da = new DataAccess();

        public Operations(string DbConnString, string CardConnString) 
        {
            da.StoreConnStrings(DbConnString, CardConnString);
        }


        public async Task<LoginInfoReturn> GenerateUserState

        public async Task<SignInOut> GenerateSignInInfo(User user, DateTime startTime, Shift startShift, Shift endShift)
        {
            DateTime signInTime = DateTime.Now;

            SignInOut signInInfo = new SignInOut();

            TimeSpan.TryParse(DateTime.Now.ToString(), out TimeSpan currentTime);

            if (startShift.startTime >= currentTime)
            {
                signInInfo.RequireNotification = false;
                DateTime.TryParse(DateTime.Today.ToString() + startShift.startTime, out signInTime);
            }
            else if (startShift.startTime < currentTime)
            {
                signInTime = DateTime.Now;
                signInInfo.RequireNotification = true;
                signInInfo.NotificationReason = 1;
            }

            // Adds info to signInInfo
            signInInfo.startShiftId = startShift.shiftID;
            signInInfo.endShiftId = endShift.shiftID;
            signInInfo.inTime = signInTime;

            return signInInfo;
        }
        
        public async Task<SignInOut> GenerateSignOutInfo(User user, SignInOut signInInfo)
        {
            DateTime signInTime = signInInfo.inTime;
            DateTime signOutTime = DateTime.Now;

            if (signInTime.DayOfYear < signOutTime.DayOfYear)
            {
                signInInfo.RequireNotification = true;
                signInInfo.NotificationReason = 2;
            }
            else
            {
                signInInfo.RequireNotification = false;
            }

            return signInInfo;
        }

        public float CalcPay(float hours, float rate)
        {
            return hours * rate;
        }

    }
}
