using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    class Operationsv2
    {
        /* Methods needed:
         * Generate sign in, sign out shift/meetings (with time overrides)
         * Insert & update database items (do validation here also)
         * Get latest sign in activity (shift/special task and meeting)
         */

        public DataAccess2 da = new DataAccess2();
        public Operationsv2(string DbConnString, string CardConnString)
        {
            da.StoreConnStrings(DbConnString, CardConnString);
        }

        public async Task<bool> AddNewRate(Rate rate)
        {
            if ((!float.IsNaN(rate.rate) || rate.rate != float.MinValue) && !string.IsNullOrEmpty(rate.rateDesc))
            {
                bool IsSuccess = await da.AddRateAsync(rate);
                return IsSuccess;
            }

            return false;
        }

        public async Task<bool> UpdateRateAsync(Rate rate)
        {
            if ((float.IsNaN(rate.rate) || rate.rate != float.MinValue) && !string.IsNullOrEmpty(rate.rateDesc))
            {
                bool IsSuccess = await da.UpdateRateAsync(rate);
                return IsSuccess;
            }

            return false;
        }
        public async Task<bool> DeleteRateAsync(Rate rate)
        {
            if (rate.rateID != int.MinValue)
            {
                bool IsSuccess = await da.DeleteRateAsync(rate);
                return IsSuccess;
            }

            return false;
        }

        public async Task<bool> AddNewUserGroup(UserGroup userGroup)
        {
            if (!string.IsNullOrEmpty(userGroup.groupName) && userGroup.DefaultRate != null)
            {
                bool IsSuccess = await da.AddNewUserGroupAsync(userGroup);
                return IsSuccess;
            }

            return false;
        }

        public async Task<bool> UpdateUserGroup(UserGroup userGroup)
        {
            if (!string.IsNullOrEmpty(userGroup.groupName) && userGroup.DefaultRate != null)
            {
                bool IsSuccess = await da.UpdateUserGroupAsync(userGroup);
                return IsSuccess;
            }

            return false;
        }

        public async Task<bool> DeleteUserGroup(UserGroup userGroup)
        {
            bool IsSuccess = await da.DeleteUserGroupAsync(userGroup);
            return IsSuccess;
        }

        public async Task<bool> UpdateUser(User user)
        {
            if (!string.IsNullOrEmpty(user.userID) && !string.IsNullOrEmpty(user.fullName))
            {
                bool IsSuccess = await da.UpdateUserAsync(user);
                return IsSuccess;
            }
            return false;
        }

        public async Task<bool> DeleteUser(User user)
        {
            if (!string.IsNullOrEmpty(user.userID))
            {
                bool IsSuccess = await da.DeleteUserAsync(user);
                return IsSuccess;
            }
            return false;
        }

        public async Task<bool> AddNewLocation(Location location)
        { 
            if (!string.IsNullOrEmpty(location.locationName))
            {
                bool IsSuccess = await da.AddNewLocationAsync(location);
                return IsSuccess;
            }
            return false;
        }

        public async Task<bool> UpdateLocation(Location location)
        {
            if (!string.IsNullOrEmpty(location.locationName) && location.locationID != int.MinValue)
            {
                bool IsSuccess = await da.UpdateLocationAsync(location);
                return IsSuccess;
            }
            return false;
        }

        public async Task<bool> DeleteLocation(Location location)
        {
            if (location.locationID != int.MinValue)
            {
                bool IsSuccess = await da.DeleteLocationAsync(location);
                return IsSuccess;
            }
            return false;
        }

        public async Task<bool> AddNewShift(Shift shift)
        {
            if (!string.IsNullOrEmpty(shift.shiftName) && shift.startTime != TimeSpan.MinValue 
                && shift.endTime != TimeSpan.MinValue && shift.DefaultRate != null 
                && shift.locationID != int.MinValue)
            {
                bool IsSuccess = await da.AddNewShiftAsync(shift);
                return IsSuccess;
            }
            return false;
        }

        public async Task<bool> UpdateShift(Shift shift)
        {
            if (!string.IsNullOrEmpty(shift.shiftName) && shift.startTime != TimeSpan.MinValue
                && shift.endTime != TimeSpan.MinValue && shift.DefaultRate != null
                && shift.locationID != int.MinValue && shift.shiftID != int.MinValue)
            {
                bool IsSuccess = await da.UpdateShiftAsync(shift);
                return IsSuccess;
            }
            return false;
        }

        public async Task<bool> DeleteShift(Shift shift)
        {
            if (shift.shiftID != int.MinValue)
            {
                bool IsSuccess = await da.DeleteShiftAsync(shift);
                return IsSuccess;
            }
            return false;
        }

        public async Task<bool> AddNewMeeting(Meeting meeting, List<MeetingUserGroup> meetingUserGroups)
        {
            if (!string.IsNullOrEmpty(meeting.meetingName) && meeting.meetingDay != int.MinValue && meeting.StartTime != TimeSpan.MinValue)
            {
                // To-Do: Re-do AddNewMeetingAsync() method to return ID of added 
                //        meeting then loop through MeetingUserGroups to copy that ID
                //        to each MeetingUserGroup and add them to database.
                bool IsSuccess = await da.AddNewMeetingAsync(meeting);
                
            }
            return false;
        }

        public async Task<bool> UpdateMeeting(Meeting meeting, List<MeetingUserGroup> meetingUserGroups)
        {
            if (meeting.meetingID != int.MinValue && string.IsNullOrEmpty(meeting.meetingName) && meeting.meetingDay != int.MinValue && meeting.StartTime != TimeSpan.MinValue)
            {
                bool IsSuccess = await da.UpdateMeetingAsync(meeting);
                if (IsSuccess)
                {
                    IsSuccess = await da.DeleteMeetingGroupAsync(meeting.meetingID);
                    if (IsSuccess)
                    {
                        foreach(var group in meetingUserGroups)
                        {
                            IsSuccess = await da.AddNewMeetingGroupAsync(group);
                            if (!IsSuccess)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public async Task<bool> DeleteMeeting(Meeting meeting)
        {
            if (meeting.meetingID != int.MinValue)
            {
                bool IsSuccess = await da.DeleteMeetingGroupAsync(meeting.meetingID);
                if (IsSuccess)
                {
                    IsSuccess = await da.DeleteMeetingAsync(meeting);
                    return IsSuccess;
                }
            }
            return false;
        }

        /// <summary>
        /// Generate an object with required information to store activity (Shift/Special Task/Shiftless)
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="startShift"></param>
        /// <param name="endShift"></param>
        /// <returns></returns>
        public Activity GenerateWorkActivity(string UserID, Shift startShift, Shift endShift)
        {
            Activity activity = new Activity()
            {
                userID = UserID,
                locationID = startShift.locationID,
                StartShift = startShift,
                EndShift = endShift
            };

            if (startShift.startTime >= DateTime.Now.TimeOfDay)
            {
                activity.RequireNotification = false;
                string d = DateTime.Today.ToShortDateString() + " " + startShift.startTime.ToString();
                DateTime.TryParse(d, out DateTime inTime);
                activity.inTime = inTime;
            }
            else
            {
                activity.RequireNotification = true;
                activity.notifyReason = Activity.NotifyReason.LateSignIn;
                activity.inTime = DateTime.Now;
            }

            return activity;
        }

        /// <summary>
        /// Generate an object with required information to store activity (Meeting)
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="meeting"></param>
        /// <returns></returns>
        public Activity GenerateMeetingActivity(string UserID, Meeting meeting)
        {
            Activity activity = new Activity()
            {
                userID = UserID,
                locationID = meeting.locationID,
                meeting = meeting
            };

            if (meeting.StartTime >= DateTime.Now.TimeOfDay)
            {
                activity.RequireNotification = false;
                string d = DateTime.Today.ToShortDateString() + " " + meeting.StartTime.ToString();
                DateTime.TryParse(d, out DateTime inTime);
                activity.inTime = inTime;
            }
            else
            {
                activity.RequireNotification = true;
                activity.notifyReason = Activity.NotifyReason.LateSignIn;
                activity.inTime = DateTime.Now;
            }

            return activity;
        }

        public Activity CompleteWorkActivity(Activity activity, User user, bool OverrideTime)
        {
            if (OverrideTime == false)
            {
                if (activity.StartShift.shiftName != "Special Task")
                {
                    if (activity.inTime.DayOfYear < DateTime.Now.DayOfYear)
                    {
                        activity.RequireNotification = true;
                        activity.notifyReason = Activity.NotifyReason.LateSignOut;
                        string s = activity.inTime.ToShortDateString() + " " + activity.EndShift.endTime.ToString();
                        DateTime.TryParse(s, out DateTime signOutTime);
                        activity.outTime = signOutTime;
                        activity.actualOutTime = DateTime.Now;
                    }
                    else if (DateTime.Now.TimeOfDay < activity.EndShift.endTime)
                    {
                        activity.RequireNotification = true;
                        activity.notifyReason = Activity.NotifyReason.EarlySignOut;
                    }

                    if (activity.outTime == DateTime.MinValue)
                    {
                        activity.outTime = DateTime.Now;
                    }
                }
            }

            TimeSpan workHour = activity.outTime.Subtract(activity.inTime);

            var d = workHour.TotalHours / 6;
            int removeTimes = Convert.ToInt32(Math.Floor(d));
            if (removeTimes > 0)
            {
                for (int i = 0; i <= removeTimes; i++)
                {
                    workHour = workHour.Subtract(new TimeSpan(0, 30, 0));
                }
            }

            if (activity.StartShift.DefaultRate.rate != 0)
            {
                if (user.userGroup.DefaultRate.rate > activity.StartShift.DefaultRate.rate)
                {
                    activity.ApplicableRate = user.userGroup.DefaultRate;
                }
                else
                {
                    activity.ApplicableRate = activity.StartShift.DefaultRate;
                }
            }

            activity.ClaimableAmount = CalcPay(workHour.TotalHours, activity.ApplicableRate.rate);
            activity.ApprovedHours = workHour.TotalHours;
            activity.ClaimDate = DateTime.Today;

            return activity;
        }

        public Activity CompleteMeetingActivity(Activity activity, User user, bool OverrideTime)
        {
            if (OverrideTime == false)
            {
                if (activity.inTime.DayOfYear < DateTime.Now.DayOfYear)
                {
                    activity.RequireNotification = true;
                    activity.notifyReason = Activity.NotifyReason.LateSignOut;
                    string s = activity.inTime.ToShortDateString() + " " + activity.EndShift.endTime.ToString();
                    DateTime.TryParse(s, out DateTime signOutTime);
                }

                activity.outTime = DateTime.Now;
            }

            TimeSpan workHour = activity.outTime.Subtract(activity.inTime);

            var d = workHour.TotalHours / 6;
            int removeTimes = Convert.ToInt32(Math.Floor(d));
            if (removeTimes > 0)
            {
                for (int i = 0; i <= removeTimes; i++)
                {
                    workHour = workHour.Subtract(new TimeSpan(0, 30, 0));
                }
            }

            if (activity.meeting.rate.rate != 0)
            {
                if (user.userGroup.DefaultRate.rate > activity.meeting.rate.rate)
                {
                    activity.ApplicableRate = user.userGroup.DefaultRate;
                }
                else
                {
                    activity.ApplicableRate = activity.meeting.rate;
                }
            }

            activity.ClaimableAmount = CalcPay(workHour.TotalHours, activity.ApplicableRate.rate);
            activity.ApprovedHours = workHour.TotalHours;
            activity.ClaimDate = DateTime.Today;

            return activity;
        }

        public float CalcPay(double hours, float rate)
        {
            return (float)hours * rate;
        }
    }
}
