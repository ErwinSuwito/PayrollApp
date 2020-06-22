using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    public class Operationsv2
    {
        /* Methods needed:
         * Generate sign in, sign out shift/meetings (with time overrides)
         * Insert & update database items (do validation here also)
         * Get latest sign in activity (shift/special task and meeting)
         */

        private DataAccess2 da = new DataAccess2();

        /// <summary>
        /// Copies passed connection strings to DataAccess
        /// </summary>
        /// <param name="DbConnString"></param>
        /// <param name="CardConnString"></param>
        public void StoreConnString(string DbConnString, string CardConnString)
        {
            da.StoreConnStrings(DbConnString, CardConnString);
        }

        /// <summary>
        /// Gets the last exception in DataAccess
        /// </summary>
        /// <returns></returns>
        public Exception GetLastError()
        {
            return da.lastError;
        }

        /// <summary>
        /// Gets the requested Rate
        /// </summary>
        /// <param name="RateID"></param>
        /// <returns></returns>
        public async Task<Rate> GetRateById(int RateID)
        {
            return await da.GetRateById(RateID);
        }

        /// <summary>
        /// Get all rates on the database and returns it in an ObservableCollection
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Rate>> GetAllRates(bool GetDisabled)
        {
            return await da.GetAllRatesAsync(GetDisabled);
        }

        /// <summary>
        /// Adds a new rate
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public async Task<bool> AddNewRate(Rate rate)
        {
            if ((!float.IsNaN(rate.rate) || rate.rate != float.MinValue) && !string.IsNullOrEmpty(rate.rateDesc))
            {
                bool IsSuccess = await da.AddRateAsync(rate);
                return IsSuccess;
            }

            return false;
        }

        /// <summary>
        /// Updates the specified rate
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRateAsync(Rate rate)
        {
            if ((float.IsNaN(rate.rate) || rate.rate != float.MinValue) && !string.IsNullOrEmpty(rate.rateDesc))
            {
                bool IsSuccess = await da.UpdateRateAsync(rate);
                return IsSuccess;
            }

            return false;
        }

        /// <summary>
        /// Delete the specified rate
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRateAsync(Rate rate)
        {
            if (rate.rateID != int.MinValue)
            {
                bool IsSuccess = await da.DeleteRateAsync(rate);
                return IsSuccess;
            }

            return false;
        }

        /// <summary>
        /// Gets the requested user group
        /// </summary>
        /// <param name="GroupID"></param>
        /// <returns></returns>
        public async Task<UserGroup> GetUserGroupById(int GroupID)
        {
            UserGroup userGroup = await da.GetUserGroupByIdAsync(GroupID);
            userGroup.DefaultRate = await da.GetRateById(userGroup.DefaultRate.rateID);
            return userGroup;
        }

        /// <summary>
        /// Gets all user groups and returns in an ObservableCollection
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <param name="GetCompleteData">True to get default rate data instead of its ID only</param>
        /// <returns></returns>
        public async Task<ObservableCollection<UserGroup>> GetUserGroups(bool GetDisabled, bool GetCompleteData)
        {
            ObservableCollection<UserGroup> userGroups =  await da.GetAllUserGroupsAsync(GetDisabled);
            
            if (GetCompleteData)
            {
                foreach (UserGroup group in userGroups)
                {
                    group.DefaultRate = await da.GetRateById(group.DefaultRate.rateID);
                }
            }

            return userGroups;
        }

        /// <summary>
        /// Add new user group
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> AddNewUserGroup(UserGroup userGroup)
        {
            if (!string.IsNullOrEmpty(userGroup.groupName) && userGroup.DefaultRate != null)
            {
                bool IsSuccess = await da.AddNewUserGroupAsync(userGroup);
                return IsSuccess;
            }

            return false;
        }

        /// <summary>
        /// Updates user group
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUserGroup(UserGroup userGroup)
        {
            if (!string.IsNullOrEmpty(userGroup.groupName) && userGroup.DefaultRate != null)
            {
                bool IsSuccess = await da.UpdateUserGroupAsync(userGroup);
                return IsSuccess;
            }

            return false;
        }

        /// <summary>
        /// Delete user group
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserGroup(UserGroup userGroup)
        {
            bool IsSuccess = await da.DeleteUserGroupAsync(userGroup);
            return IsSuccess;
        }

        /// <summary>
        /// Gets the requested user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<User> GetUserById(string userID)
        {
            User user = await da.GetUserByIdAsync(userID);
            user.userGroup = await da.GetUserGroupByIdAsync(user.userGroup.groupID);
            user.userGroup.DefaultRate = await da.GetRateById(user.userGroup.DefaultRate.rateID);

            return user;
        }

        /// <summary>
        /// Gets all users in the database
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <param name="CompleteData">True to get user's usergroup and default rate data</param>
        /// <returns></returns>
        public async Task<ObservableCollection<User>> GetAllUsers(bool GetDisabled, bool CompleteData)
        {
            ObservableCollection<User> users = await da.GetAllUsersAsync(GetDisabled);
            if (CompleteData)
            {
                foreach (User user in users)
                {
                    user.userGroup = await da.GetUserGroupByIdAsync(user.userGroup.groupID);
                    user.userGroup.DefaultRate = await da.GetRateById(user.userGroup.DefaultRate.rateID);
                }
            }

            return users;
        }

        /// <summary>
        /// Adds a user to the database
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<bool> AddUser(User user)
        {
            if (!string.IsNullOrEmpty(user.userID) && !string.IsNullOrEmpty(user.fullName) && user.userGroup != null)
            {
                bool IsSuccess = await da.AddNewUserAsync(user);
                return IsSuccess;
            }
            return false;
        }

        /// <summary>
        /// Updates a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUser(User user)
        {
            if (!string.IsNullOrEmpty(user.userID) && !string.IsNullOrEmpty(user.fullName))
            {
                bool IsSuccess = await da.UpdateUserAsync(user);
                return IsSuccess;
            }
            return false;
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUser(User user)
        {
            if (!string.IsNullOrEmpty(user.userID))
            {
                bool IsSuccess = await da.DeleteUserAsync(user);
                return IsSuccess;
            }
            return false;
        }

        /// <summary>
        /// Gets the requested by its ID
        /// </summary>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public async Task<Location> GetLocationById(int locationID)
        {
            return await da.GetLocationByIdAsync(locationID);
        }

        /// <summary>
        /// Gets all location on the database
        /// </summary>
        /// <param name="GetDisabled">True to also get disabled locations</param>
        /// <returns></returns>
        public async Task<ObservableCollection<Location>> GetLocations(bool GetDisabled)
        {
            return await da.GetAllLocationAsync(GetDisabled);
        }

        /// <summary>
        /// Adds a new location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<bool> AddNewLocation(Location location)
        { 
            if (!string.IsNullOrEmpty(location.locationName))
            {
                bool IsSuccess = await da.AddNewLocationAsync(location);
                return IsSuccess;
            }
            return false;
        }

        /// <summary>
        /// Updates a location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<bool> UpdateLocation(Location location)
        {
            if (!string.IsNullOrEmpty(location.locationName) && location.locationID != int.MinValue)
            {
                bool IsSuccess = await da.UpdateLocationAsync(location);
                return IsSuccess;
            }
            return false;
        }

        /// <summary>
        /// Deletes a location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<bool> DeleteLocation(Location location)
        {
            if (location.locationID != int.MinValue)
            {
                bool IsSuccess = await da.DeleteLocationAsync(location);
                return IsSuccess;
            }
            return false;
        }

        /// <summary>
        /// Gets the requested shift
        /// </summary>
        /// <param name="ShiftID"></param>
        /// <returns></returns>
        public async Task<Shift> GetShiftById(int ShiftID)
        {
            Shift shift = await da.GetShiftByIdAsync(ShiftID);
            shift.DefaultRate = await da.GetRateById(shift.DefaultRate.rateID);

            return shift;
        }

        /// <summary>
        /// Get all shifts
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <param name="CompleteData">True to get shift's default rate data</param>
        /// <returns></returns>
        public async Task<ObservableCollection<Shift>> GetShifts(bool GetDisabled, bool CompleteData)
        {
            ObservableCollection<Shift> shifts = await da.GetAllShiftsAsync(GetDisabled);
            if (CompleteData)
            {
                foreach (Shift shift in shifts)
                {
                    shift.DefaultRate = await da.GetRateById(shift.DefaultRate.rateID);
                }
            }

            return shifts;
        }

        /// <summary>
        /// Gets all shift in a location
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <param name="locationID"></param>
        /// <param name="CompleteData">True to get shift's default rate data</param>
        /// <returns></returns>
        public async Task<ObservableCollection<Shift>> GetShifts(bool GetDisabled, int locationID, bool CompleteData)
        {
            ObservableCollection<Shift> shifts = await da.GetAllShiftsAsync(GetDisabled, locationID);
            if (CompleteData)
            {
                foreach (Shift shift in shifts)
                {
                    shift.DefaultRate = await da.GetRateById(shift.DefaultRate.rateID);
                }
            }

            return shifts;
        }

        /// <summary>
        /// Adds a new shift
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates shift info
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Deletes a shift
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
        public async Task<bool> DeleteShift(Shift shift)
        {
            if (shift.shiftID != int.MinValue)
            {
                bool IsSuccess = await da.DeleteShiftAsync(shift);
                return IsSuccess;
            }
            return false;
        }


        public async Task<Meeting> GetMeetingById(int MeetingID)
        {
            Meeting meeting = await da.GetMeetingByIdAsync(MeetingID);
            meeting.rate = await da.GetRateById(meeting.rate.rateID);

            return meeting;
        }

        /// <summary>
        /// Adds a new meeting
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="meetingUserGroups"></param>
        /// <returns></returns>
        public async Task<bool> AddNewMeeting(Meeting meeting, List<MeetingUserGroup> meetingUserGroups)
        {
            if (!string.IsNullOrEmpty(meeting.meetingName) && meeting.meetingDay != int.MinValue && meeting.StartTime != TimeSpan.MinValue)
            {
                int meetingID = await da.AddNewMeetingAsync(meeting);
                bool IsSuccess = false;
                
                foreach (MeetingUserGroup meetingGroup in meetingUserGroups)
                {
                    meetingGroup.meetingID = meetingID;
                    IsSuccess = await da.AddNewMeetingGroupAsync(meetingGroup);
                    if (IsSuccess == false)
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Updates the specified meeting
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="meetingUserGroups"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Deletes the specified meeting
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Takes an activity object and calculates approved work hours, applicable rate and claimable amount (Shift/Special Task/Shiftless)
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="user"></param>
        /// <param name="OverrideTime"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Takes an activity object and calculates approved work hours, applicable rate and claimable amount (Meetings)
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="user"></param>
        /// <param name="OverrideTime"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculates the claimable amount
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public float CalcPay(double hours, float rate)
        {
            return (float)hours * rate;
        }

        /// <summary>
        /// Gets the owner of the card
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public async Task<string> GetUserIdFromCard(string cardId)
        {
            string username = await da.GetUsernameFromCardId(cardId);
            string pattern = @"\D\D\d\d\d\d\d";

            if (username != null)
            {
                RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;

                if (Regex.IsMatch(username, pattern, options))
                {
                    username += "@mail.apu.edu.my";
                }
                else
                {
                    username += "@cloudmails.apu.edu.my";
                }

                return username;
            }
            else
            {
                return null;
            }
        }
    }
}
