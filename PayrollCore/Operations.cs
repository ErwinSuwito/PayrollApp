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
        
        public async Task<UserState> GenerateSignOutInfo(UserState userState)
        {
            DateTime signInTime = userState.LatestActivity.inTime;
            DateTime signOutTime = DateTime.Now;

            if (signInTime.DayOfYear < signOutTime.DayOfYear)
            {
                userState.LatestActivity.RequireNotification = true;
                userState.LatestActivity.NotificationReason = 2;
                string s = userState.LatestActivity.inTime.ToShortDateString() + " " + userState.LatestActivity.EndShift.startTime.ToString();
                DateTime.TryParse(s, out signOutTime);
            }
            else
            {
                userState.LatestActivity.RequireNotification = false;
            }

            userState.LatestActivity.outTime = signOutTime;

            TimeSpan activityOffset = signOutTime.Subtract(signInTime);

            if (userState.user.userGroup.DefaultRate.rate > userState.LatestActivity.StartShift.DefaultRate.rate)
            {
                // Use user's default rate
                userState.LatestActivity.ApplicableRate = userState.user.userGroup.DefaultRate;
            }
            else
            {
                // Use shift's default rate
                userState.LatestActivity.ApplicableRate = userState.LatestActivity.StartShift.DefaultRate;
            }

            userState.LatestActivity.ClaimableAmount = CalcPay(activityOffset.TotalHours, userState.LatestActivity.ApplicableRate.rate);
            userState.LatestActivity.ApprovedHours = activityOffset.TotalHours;

            return userState;
        }

        public float CalcPay(double hours, float rate)
        {
            return (float)hours * rate;
        }

    }
}
