﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    [Obsolete("Operations is depracated. Use Operationsv2 instead.", true)]
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
        /// Calculates the applicable rates, work hours, and claimable amount from an Activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="user"></param>
        /// <param name="OverrideTime">Set to true to use the sign in and sign out time in the Activity object</param>
        /// <returns></returns>
        public async Task<Activity> GenerateSignOutInfo(Activity activity, User user, bool OverrideTime)
        {
            TimeSpan activityOffset = activity.outTime.Subtract(activity.inTime);
            TimeSpan approvedWork;

            if (OverrideTime == false)
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
                        DateTime.TryParse(s, out DateTime actualSignOutTime);
                        activity.actualOutTime = actualSignOutTime;
                    }
                    else
                    {
                        activity.RequireNotification = false;
                    }
                }

                activity.outTime = signOutTime;
            }

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
            int removeTimes = Convert.ToInt32(activityOffset.TotalHours / 6);
            
            for (int i =0; i != removeTimes; i++)
            {
                approvedWork = activityOffset.Subtract(new TimeSpan(0, 30, 0));
            }

            activity.ClaimableAmount = CalcPay(activityOffset.TotalHours, activity.ApplicableRate.rate);
            activity.ApprovedHours = approvedWork.TotalHours;

            return activity;
        }

        public async Task<Activity> GenerateSignOut(Activity activity, User user, bool OverrideTime)
        {
            DateTime signInTime = activity.inTime;
            DateTime signOutTime = activity.outTime;

            // Checks if there is no need to assign out time.
            if (OverrideTime == false)
            {
                signOutTime = DateTime.Now;

                if (activity.StartShift.shiftName != "Special Task")
                {
                    if (signInTime.DayOfYear < signOutTime.DayOfYear)
                    {
                        activity.RequireNotification = true;
                        activity.NotificationReason = 2;
                        string s = activity.inTime.ToShortDateString() + " " + activity.EndShift.startTime.ToString();
                        DateTime.TryParse(s, out DateTime actualSignOutTime);
                        activity.actualOutTime = actualSignOutTime;
                    }
                    else
                    {
                        activity.RequireNotification = false;
                    }
                }

                activity.outTime = signOutTime;
            }

            // Calculates approved work hour
            TimeSpan workHour = signOutTime.Subtract(signInTime);

            // Calculates how much times the workHour needs to be decreased by 30 minutes
            var d = workHour.TotalHours / 6;
            int removeTimes = Convert.ToInt32(Math.Floor(d));
            if (removeTimes > 0)
            {
                for (int i = 0; i <= removeTimes; i++)
                {
                    workHour = workHour.Subtract(new TimeSpan(0, 30, 0));
                }
            }

            // Checks which rate applies
            if (activity.StartShift.DefaultRate.rate != 0)
            {
                // Applies the bigger rate
                if (user.userGroup.DefaultRate.rate > activity.StartShift.DefaultRate.rate)
                {
                    activity.ApplicableRate = user.userGroup.DefaultRate;
                }
                else
                {
                    activity.ApplicableRate = activity.StartShift.DefaultRate;
                }
            }
            else
            {
                activity.ApplicableRate = activity.StartShift.DefaultRate;
            }

            // Calculates the claimable amount
            activity.ClaimableAmount = CalcPay(workHour.TotalHours, activity.ApplicableRate.rate);
            activity.ApprovedHours = workHour.TotalHours;
            activity.ClaimDate = DateTime.Today;

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

        public Activity CompleteMeetingAttendance(Activity activity, User user, bool OverrideTime)
        {
            if (OverrideTime == false)
            {
                activity.outTime = DateTime.Now;
            }
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
