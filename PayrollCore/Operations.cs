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

        /// <summary>
        /// Creates a new location and returns the new location for further
        /// customization (adding meetings, special task shift)
        /// </summary>
        /// <returns></returns>
        public async Task<Location> PrepareNewLocation()
        {
            Location location = new Location();
            location.isDisabled = false;
            location.locationName = Guid.NewGuid().ToString();
            location.enableGM = false;

            int LocationID = await da.AddLocationAsync(location);

            location = await da.GetLocationById(LocationID.ToString());
            location.isNewLocation = true;

            return location;
        }

        /// <summary>
        /// Generates Activity object for regular shift sign in
        /// </summary>
        /// <param name="user"></param>
        /// <param name="startShift"></param>
        /// <param name="endShift"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<Activity> GenerateSignInInfo(User user, Shift startShift, Shift endShift)
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
            signInInfo.locationID = startShift.locationID;
            signInInfo.StartShift = startShift;
            signInInfo.EndShift = endShift;
            signInInfo.inTime = signInTime;

            return signInInfo;
        }

        /// <summary>
        /// Generate Activity object for special task
        /// </summary>
        /// <param name="user"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        //public  Activity GenerateSpecialTask(User user, Location location)
        //{
        //    Activity signInInfo = new Activity();

        //    signInInfo.userID = user.userID;
        //    signInInfo.locationID = location.locationID;
        //    signInInfo.inTime = DateTime.Now;
        //    signInInfo.RequireNotification = false;
        //    signInInfo.IsSpecialTask = true;

        //    return signInInfo;
        //}

        //public Activity CompleteSpecialTask(Activity activity, User user)
        //{
        //    DateTime signInTime = activity.inTime;
        //    DateTime signOutTime = DateTime.Now;

        //    if (signInTime.DayOfYear < signOutTime.DayOfYear)
        //    {
        //        activity.RequireNotification = false;
        //        activity.NotificationReason = 2;
        //    }
        //    else
        //    {
        //        activity.RequireNotification = false;
        //    }

        //    if (user.userGroup.DefaultRate.rate > activity.StartShift.DefaultRate.rate)
        //    {
        //        activity.ApplicableRate = user.userGroup.DefaultRate;
        //    }
        //    else
        //    {
        //        activity.ApplicableRate = 
        //    }

        //    TimeSpan activityOffset = signOutTime.Subtract(signInTime);
        //    activity.ApprovedHours = activityOffset.TotalHours;
        //    activity.ClaimDate = DateTime.Today;
        //    activity.ClaimableAmount = CalcPay(activityOffset.TotalHours, activity.ApplicableRate.rate);

        //    return activity;
        //}
        
        public async Task<Activity> GenerateSignOutInfo(Activity activity, User user)
        {
            DateTime signInTime = activity.inTime;
            DateTime signOutTime = DateTime.Now;

            if (activity.StartShift.shiftName != "Special Task")
            {
                if (signInTime.DayOfYear < signOutTime.DayOfYear)
                {
                    activity.RequireNotification = true;
                    activity.NotificationReason = 2;
                    string s = activity.inTime.ToShortDateString() + " " + activity.EndShift.startTime.ToString();
                    DateTime.TryParse(s, out signOutTime);
                }
                else
                {
                    activity.RequireNotification = false;
                }
            }

            activity.outTime = signOutTime;

            TimeSpan activityOffset = signOutTime.Subtract(signInTime);

            if (user.userGroup.DefaultRate.rate > activity.StartShift.DefaultRate.rate)
            {
                // Use user's default rate
                activity.ApplicableRate = user.userGroup.DefaultRate;
            }
            else
            {
                // Use shift's default rate
                activity.ApplicableRate = activity.StartShift.DefaultRate;
            }

            activity.ClaimDate = DateTime.Today;
            activity.ClaimableAmount = CalcPay(activityOffset.TotalHours, activity.ApplicableRate.rate);
            activity.ApprovedHours = activityOffset.TotalHours;

            return activity;
        }

        /// <summary>
        /// Generates an Activity object that stores meeting attendance data
        /// </summary>
        /// <param name="user"></param>
        /// <param name="MeetingID"></param>
        /// <returns></returns>
        public async Task<Activity> GenerateMeetingAttendance(User user, int MeetingID)
        {
            Activity activity = new Activity();

            activity.meeting = await da.GetMeetingById(MeetingID);

            if (activity.meeting != null)
            {
                activity.userID = user.userID;
                activity.inTime = DateTime.Now;
                activity.NoActivity = false;
                activity.locationID = activity.meeting.locationID;
                
                if (activity.meeting.StartTime > DateTime.Now.TimeOfDay)
                {
                    activity.RequireNotification = true;
                    activity.NotificationReason = 1;
                }

                return activity;
            }
            else
            {
                return null;
            }
        }

        public Activity CompleteMeetingAttendance(Activity activity, User user)
        {
            activity.outTime = DateTime.Now;
            TimeSpan activityOffset = activity.outTime.Subtract(activity.inTime);
            activity.ApprovedHours = activityOffset.TotalHours;
            activity.ClaimDate = DateTime.Today;

            if (user.userGroup.DefaultRate.rate > activity.meeting.rate.rate)
            {
                activity.ApplicableRate = user.userGroup.DefaultRate;
            }
            else
            {
                activity.ApplicableRate = activity.meeting.rate;
            }

            activity.ClaimableAmount = CalcPay(activityOffset.TotalHours, activity.ApplicableRate.rate);

            return activity;
        }

        public float CalcPay(double hours, float rate)
        {
            return (float)hours * rate;
        }

        public async Task<UserState> GetUserState(User user, int locationID)
        {
            UserState userState = new UserState();
            userState.user = user;
            userState.LatestActivity = await da.GetLatestSignIn(user.userID, locationID);

            if (userState.LatestActivity.NoActivity == false)
            {
                userState.LatestMeeting = await da.GetLatestMeeting(user.userID, locationID);
            }

            userState.ApprovedHours = await da.GetApprovedHours(user.userID);

            return userState;
        }

    }
}
