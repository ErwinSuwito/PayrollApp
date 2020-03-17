using System;
using System.Collections.Generic;
using System.Diagnostics;
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


        public async Task<UserState> GenerateUserState(User user)
        {
            UserState state = new UserState();



            return state;
        }

        public async Task<Activity> GenerateSignInInfo(User user, Shift startShift, Shift endShift, Location location)
        {
            DateTime signInTime = DateTime.Now;

            Activity signInInfo = new Activity();

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            if (startShift.startTime >= currentTime)
            {
                signInInfo.RequireNotification = false;
                string s = DateTime.Today.ToShortDateString() + " " + startShift.startTime.ToString();
                Debug.WriteLine(s);
                DateTime.TryParse(s, out signInTime);
            }
            else if (startShift.startTime < currentTime)
            {
                signInTime = DateTime.Now;
                signInInfo.RequireNotification = true;
                signInInfo.NotificationReason = 1;
            }

            // Adds info to signInInfo
            signInInfo.userID = user.userID;
            signInInfo.locationID = location.locationID;
            signInInfo.StartShift = startShift;
            signInInfo.EndShift = endShift;
            signInInfo.inTime = signInTime;

            return signInInfo;
        }
        
        public async Task<Activity> GenerateSignOutInfo(Activity signInInfo)
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
