using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    public class DataAccess
    {
        string DbConnString;
        string CardConnString;
        Exception lastError;

        /// <summary>
        /// Saves the connection string to memory.
        /// </summary>
        /// <param name="dbConnString"></param>
        /// <param name="cardConnString"></param>
        public void StoreConnStrings(string dbConnString, string cardConnString)
        {
            DbConnString = dbConnString;
            CardConnString = cardConnString;
        }

        /// <summary>
        /// Tries to connect to the database using the passed connection string.
        /// </summary>
        /// <param name="connString">The connection string to be tested.</param>
        /// <returns>Boolean</returns>
        public bool TestConnString(string connString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get all locations stored in the database.
        /// </summary>
        public async Task<ObservableCollection<Location>> GetLocations(bool showDisabled)
        {
            lastError = null;
            const string GetLocationsQuery = "SELECT * FROM Location";

            var items = new ObservableCollection<Location>();
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = GetLocationsQuery;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                if (showDisabled == false && dr.GetBoolean(3) == true)
                                {
                                    continue;
                                }

                                if (dr.GetString(1).Contains("new-sys"))
                                {
                                    continue;
                                }

                                var item = new Location();
                                item.locationID = dr.GetInt32(0);
                                item.locationName = dr.GetString(1);
                                item.enableGM = dr.GetBoolean(2);
                                item.isDisabled = dr.GetBoolean(3);
                                item.updateLvString();
                                items.Add(item);
                            }
                        }
                    }

                }

                return items;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                lastError = ex;
            }

            return null;
        }

        /// <summary>
        /// Get meetings that are in the passed location.
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<Meeting>> GetMeetings(Location appLocation)
        {
            lastError = null;
            const string GetLocationsQuery = "SELECT * FROM Meeting JOIN Rate ON Rate.RateID=Meeting.RateID WHERE LocationID=@LocationID";

            var items = new ObservableCollection<Meeting>();
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = GetLocationsQuery;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", appLocation.locationID));
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                var item = new Meeting();
                                item.meetingID = dr.GetInt32(0);
                                item.meetingName = dr.GetString(1);
                                item.locationID = dr.GetInt32(2);
                                item.meetingDay = dr.GetInt32(3);
                                item.isDisabled = dr.GetBoolean(4);
                                item.StartTime = dr.GetTimeSpan(6);

                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(7);
                                rate.rateDesc = dr.GetString(8);
                                rate.rate = dr.GetFloat(9);
                                rate.isDisabled = dr.GetBoolean(10);

                                item.rate = rate;

                                items.Add(item);
                            }
                        }
                    }

                }

                return items;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        /// <summary>
        /// Get location information about the passed location name
        /// </summary>
        /// <param name="selectedLocation"></param>
        /// <returns></returns>
        public async Task<Location> GetLocationById(string selectedLocation)
        {
            lastError = null;
            string GetLocationSettingsQuery = "SELECT * FROM Location WHERE locationID=@LocationID";
            try
            {
                Location appLocation;

                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = GetLocationSettingsQuery;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", selectedLocation));
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                appLocation = new Location();
                                appLocation.locationID = dr.GetInt32(0);
                                appLocation.locationName = dr.GetString(1);
                                appLocation.enableGM = dr.GetBoolean(2);
                                appLocation.isDisabled = dr.GetBoolean(3);
                                return appLocation;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        /// <summary>
        /// Adds a new location to the database
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<bool> SaveLocationAsync(Location location)
        {
            lastError = null;
            string Query = "UPDATE Location SET LocationName=@LocationName, EnableGM=@EnableGM, IsDisabled=@IsDisabled WHERE LocationID=@LocationID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("LocationID", location.locationID));
                        cmd.Parameters.Add(new SqlParameter("LocationName", location.locationName));
                        cmd.Parameters.Add(new SqlParameter("EnableGM", location.enableGM));
                        cmd.Parameters.Add(new SqlParameter("IsDisabled", location.isDisabled));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return false;
        }

        /// <summary>
        /// Adds a new location into the database
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<int> AddLocationAsync(Location location)
        {
            lastError = null;
            string Query = "INSERT INTO Location(LocationName, EnableGM, IsDisabled) VALUES(@LocationName, @EnableGM, 'false') select SCOPE_IDENTITY()";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationName", location.locationName));
                        cmd.Parameters.Add(new SqlParameter("@EnableGM", location.enableGM));

                        var savedLocation = await cmd.ExecuteScalarAsync();

                        int.TryParse(savedLocation.ToString(), out int locationID);

                        return locationID;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return -1;
        }

        /// <summary>
        /// Saves the passed meeting into the database
        /// </summary>
        /// <param name="location">The location where the meeting is going to take place.</param>
        /// <param name="meetings">ObservableCollection of meetings.</param>
        /// <returns></returns>
        public async Task<Boolean> SaveMeetingSettings(Meeting meeting)
        {
            lastError = null;
            string Query;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    Query = "UPDATE Meeting SET MeetingName=@MeetingName, MeetingDay=@MeetingDay, IsDisabled=@DisableMeeting, RateID=@RateID, StartTime=@StartTime WHERE MeetingID=@MeetingID";

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meeting.meetingID));
                        cmd.Parameters.Add(new SqlParameter("@MeetingName", meeting.meetingName));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", meeting.locationID));
                        cmd.Parameters.Add(new SqlParameter("@MeetingDay", meeting.meetingDay));
                        cmd.Parameters.Add(new SqlParameter("@DisableMeeting", meeting.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@RateID", meeting.rate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@StartTime", meeting.StartTime));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return false;
        }

        public async Task<int> SaveMeetingAndReturnId(Meeting meeting)
        {
            lastError = null;
            string Query = "INSERT INTO Meeting(MeetingName, LocationID, MeetingDay, IsDisabled, RateID, StartTime) VALUES(@MeetingName, @LocationID, @MeetingDay, @DisableMeeting, @RateID, @StartTime) select SCOPE_IDENTITY()";
            
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@MeetingName", meeting.meetingName));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", meeting.locationID));
                        cmd.Parameters.Add(new SqlParameter("@MeetingDay", meeting.meetingDay));
                        cmd.Parameters.Add(new SqlParameter("@DisableMeeting", meeting.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@RateID", meeting.rate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@StartTime", meeting.StartTime));

                        var savedMeeting = await cmd.ExecuteScalarAsync();

                        int.TryParse(savedMeeting.ToString(), out int meetingID);

                        return meetingID;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return -1;
        }

        /// <summary>
        /// Get all users in the database.
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<User>> GetUsersList()
        {
            lastError = null;
            const string GetLocationsQuery = "SELECT * FROM Users JOIN user_group ON user_group.GroupID = Users.GroupID";

            var items = new ObservableCollection<User>();
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = GetLocationsQuery;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                var user = new User();
                                var user_group = new UserGroup();

                                user.userID = dr.GetString(0);
                                user.fullName = dr.GetString(1);
                                user.fromAD = dr.GetBoolean(2);
                                user.isDisabled = dr.GetBoolean(3);

                                user_group.groupID = dr.GetInt32(5);
                                user_group.groupName = dr.GetString(6);
                                user_group.ShowAdminSettings = dr.GetBoolean(8);
                                user_group.EnableFaceRec = dr.GetBoolean(9);

                                user.userGroup = user_group;

                                items.Add(user);
                            }
                        }
                    }

                }

                return items;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        /// <summary>
        /// Get location information based on the passed location name
        /// </summary>
        /// <param name="locationName"></param>
        /// <returns></returns>
        public async Task<Location> GetLocationByName(string locationName)
        {
            lastError = null;
            string GetLocationSettingsQuery = "SELECT * FROM Location WHERE LocationName=@LocationName";
            try
            {
                Location appLocation;

                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = GetLocationSettingsQuery;
                        cmd.Parameters.Add(new SqlParameter("@LocationName", locationName));
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                appLocation = new Location();
                                appLocation.locationID = dr.GetInt32(0);
                                appLocation.locationName = dr.GetString(1);
                                appLocation.enableGM = dr.GetBoolean(2);
                                return appLocation;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        /// <summary>
        /// Get user information based on passed username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<User> GetUserFromDbById(string username)
        {
            lastError = null;
            string GetUserQuery = "SELECT * FROM Users JOIN user_group ON user_group.GroupID=Users.GroupID JOIN Rate ON Rate.RateID=user_group.RateID WHERE UserID=@UserId";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = GetUserQuery;
                        cmd.Parameters.Add(new SqlParameter("@UserId", username));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                var user = new User();
                                var user_group = new UserGroup();
                                var rate = new Rate();

                                user.userID = dr.GetString(0);
                                user.fullName = dr.GetString(1);
                                user.fromAD = dr.GetBoolean(2);
                                user.isDisabled = dr.GetBoolean(3);

                                user_group.groupID = dr.GetInt32(5);
                                user_group.groupName = dr.GetString(6);
                                user_group.ShowAdminSettings = dr.GetBoolean(8);
                                user_group.EnableFaceRec = dr.GetBoolean(9);

                                rate.rateID = dr.GetInt32(10);
                                rate.rateDesc = dr.GetString(11);
                                rate.rate = dr.GetFloat(12);

                                user_group.DefaultRate = rate;
                                user.userGroup = user_group;

                                return user;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        /// <summary>
        /// Updates user info into the database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUserInfo(User user)
        {
            lastError = null;
            string Query;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    Query = "UPDATE Users SET FullName=@FullName, FromAD=@FromAD, IsDisabled=@IsDisabled, GroupID=@GroupID WHERE UserID=@UserID";

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@UserID", user.userID));
                        cmd.Parameters.Add(new SqlParameter("@FullName", user.fullName));
                        cmd.Parameters.Add(new SqlParameter("@FromAD", user.fromAD));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", user.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@GroupID", user.userGroup.groupID));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return false;
        }

        /// <summary>
        /// Adds a new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> AddNewUser(User user)
        {
            lastError = null;
            string Query;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    Query = "INSERT INTO Users VALUES(@UserID, @FullName,  @FromAD, @IsDisabled, @GroupID);";

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@UserID", user.userID));
                        cmd.Parameters.Add(new SqlParameter("@FullName", user.fullName));
                        cmd.Parameters.Add(new SqlParameter("@FromAD", user.fromAD));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", user.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@GroupID", user.userGroup.groupID));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return false;
        }

        /// <summary>
        /// Get available shifts based on the passed location
        /// </summary>
        /// <param name="locationID"></param>
        /// <param name="showDisabled"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Shift>> GetShiftsFromLocation(string locationID, bool showDisabled)
        {
            lastError = null;
            string Query = "SELECT * FROM Shifts JOIN Rate ON Rate.RateID=Shifts.RateID WHERE LocationID=@LocationID";
            ObservableCollection<Shift> shifts = new ObservableCollection<Shift>();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        SqlDataReader dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            if (dr.GetBoolean(6) == true && showDisabled == false)
                            {
                                continue;
                            }
                            else
                            {
                                var shift = new Shift();
                                shift.shiftID = dr.GetInt32(0);
                                shift.shiftName = dr.GetString(1);
                                shift.startTime = dr.GetTimeSpan(2);
                                shift.endTime = dr.GetTimeSpan(3);
                                shift.isDisabled = dr.GetBoolean(6);
                                shift.WeekendOnly = dr.GetBoolean(7);
                                shift.locationID = dr.GetInt32(4);

                                if (shift.isDisabled)
                                {
                                    shift.dg_isDisabled = "Disabled";
                                }
                                else
                                {
                                    shift.dg_isDisabled = "Enabled";
                                }

                                Rate rate = new Rate();
                                rate.rateID = dr.GetInt32(8);
                                rate.rateDesc = dr.GetString(9);
                                rate.rate = dr.GetFloat(10);

                                shift.DefaultRate = rate;

                                shifts.Add(shift);
                            }
                        }
                    }
                }

                return shifts;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return null;
            }
            
        }

        public async Task<ObservableCollection<Shift>> GetAvailableShifts(string locationID, bool weekendOnly)
        {
            ObservableCollection<Shift> shifts = await GetShiftsFromLocation(locationID, false);

            foreach (Shift shift in shifts.ToList())
            {
                if (shift.WeekendOnly != weekendOnly)
                {
                    shifts.Remove(shift);
                }
            }

            return shifts;
        }

        /// <summary>
        /// Get all user groups
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<UserGroup>> GetAllUserGroups()
        {
            lastError = null;
            string Query = "SELECT * FROM user_group JOIN Rate ON Rate.RateID=user_group.RateID";
            ObservableCollection<UserGroup> userGroups = new ObservableCollection<UserGroup>();

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            var userGroup = new UserGroup();
                            userGroup.groupID = dr.GetInt32(0);
                            userGroup.groupName = dr.GetString(1);
                            userGroup.ShowAdminSettings = dr.GetBoolean(3);
                            userGroup.EnableFaceRec = dr.GetBoolean(4);

                            var rate = new Rate();
                            rate.rateID = dr.GetInt32(5);
                            rate.rateDesc = dr.GetString(6);
                            rate.rate = dr.GetFloat(7);
                            rate.isDisabled = dr.GetBoolean(8);

                            userGroup.DefaultRate = rate;

                            userGroups.Add(userGroup);
                        }
                    }
                }

                return userGroups;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return null;
            }
        }

        /// <summary>
        /// Get a usergroup based on its ID
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<UserGroup> GetUserGroupById(int groupId)
        {
            lastError = null;
            string Query = "SELECT * FROM user_group JOIN Rate ON Rate.RateID=user_group.RateID WHERE GroupID=@GroupID";
            UserGroup userGroup = new UserGroup();

            try
            {

                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@GroupID", groupId));
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                userGroup.groupID = dr.GetInt32(0);
                                userGroup.groupName = dr.GetString(1);
                                userGroup.ShowAdminSettings = dr.GetBoolean(3);
                                userGroup.EnableFaceRec = dr.GetBoolean(4);

                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(5);
                                rate.rateDesc = dr.GetString(6);
                                rate.rate = dr.GetFloat(7);
                                rate.isDisabled = dr.GetBoolean(8);

                                userGroup.DefaultRate = rate;

                                return userGroup;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        /// <summary>
        /// Adds a new user group
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> AddNewUserGroup(UserGroup userGroup)
        {
            lastError = null;
            string Query = "INSERT INTO user_group(GroupName, RateID, ShowAdminSettings, EnableFaceRec) VALUES(@GroupName, @RateID, @ShowAdminSettings, @EnableFaceRec)";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@GroupName", userGroup.groupName));
                        cmd.Parameters.Add(new SqlParameter("@RateID", userGroup.DefaultRate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@ShowAdminSettings", userGroup.ShowAdminSettings));
                        cmd.Parameters.Add(new SqlParameter("@EnableFaceRec", userGroup.EnableFaceRec));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return false;
            }
        }

        /// <summary>
        /// Updates the usergroup info in the database.
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUserGroupInfo(UserGroup userGroup)
        {
            lastError = null;
            string Query = "UPDATE user_group SET GroupName=@GroupName, RateID=@RateID, ShowAdminSettings=@ShowAdminSettings, EnableFaceRec=@EnableFaceRec WHERE GroupID=@GroupID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@GroupID", userGroup.groupID));
                        cmd.Parameters.Add(new SqlParameter("@GroupName", userGroup.groupName));
                        cmd.Parameters.Add(new SqlParameter("@RateID", userGroup.DefaultRate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@ShowAdminSettings", userGroup.ShowAdminSettings));
                        cmd.Parameters.Add(new SqlParameter("@EnableFaceRec", userGroup.EnableFaceRec));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return false;
            }
        }

        /// <summary>
        /// Deletes a user group
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserGroup(UserGroup userGroup)
        {
            lastError = null;
            string Query = "DELETE FROM user_group WHERE GroupID=@GroupID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@GroupID", userGroup.groupID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return false;
            }
        }

        /// <summary>
        /// Get contents of meeting_group table based on the passed meetingID
        /// </summary>
        /// <param name="meetingID"></param>
        /// <returns></returns>
        public async Task<List<MeetingUserGroup>> GetMeetingUserGroupByMeetingId(int meetingID)
        {
            lastError = null;
            List<MeetingUserGroup> meetingUserGroups = new List<MeetingUserGroup>();

            string Query = "SELECT * FROM Meeting_Group WHERE MeetingID=@MeetingID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meetingID));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            var meetingUserGroup = new MeetingUserGroup();

                            meetingUserGroup.meeting_group_id = dr.GetInt32(0);
                            meetingUserGroup.meetingID = dr.GetInt32(1);
                            meetingUserGroup.usrGroupId = dr.GetInt32(2);

                            meetingUserGroups.Add(meetingUserGroup);
                        }
                    }
                }

                return meetingUserGroups;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return null;
            }

        }

        /// <summary>
        /// Get available meetings based on the user group and location
        /// </summary>
        /// <param name="userGroup">User group for the meeting</param>
        /// <param name="LocationID">The location of the meeting</param>
        /// <param name="ShowDisabled">If disabled meetings are to be returned</param>
        /// <returns></returns>
        public async Task<ObservableCollection<MeetingUserGroup>> GetMeetingUserGroupByUserGroup(int userGroup, int LocationID, bool ShowDisabled)
        {
            lastError = null;
            ObservableCollection<MeetingUserGroup> meetingUserGroups = new ObservableCollection<MeetingUserGroup>();

            string Query = "SELECT * FROM Meeting_Group JOIN Meeting ON Meeting.MeetingID=Meeting_Group.MeetingID WHERE UserGroupID=@UserGroup AND LocationID=@LocationID";

            if (ShowDisabled == false)
            {
                Query += " AND IsDisabled='0'";
            }
     
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserGroup", userGroup));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", LocationID));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            var meetingUserGroup = new MeetingUserGroup();

                            meetingUserGroup.meeting_group_id = dr.GetInt32(0);
                            meetingUserGroup.meetingID = dr.GetInt32(1);
                            meetingUserGroup.usrGroupId = dr.GetInt32(2);
                            meetingUserGroup.meetingName = dr.GetString(4);
                            meetingUserGroup.StartTime = dr.GetTimeSpan(9);

                            meetingUserGroups.Add(meetingUserGroup);
                        }
                    }
                }

                return meetingUserGroups;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return null;
            }

        }

        public async Task<bool> DeleteMeetingUserGroup(int meetingID)
        {
            lastError = null;
            string Query = "DELETE FROM Meeting_Group WHERE MeetingID=@MeetingID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meetingID));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return false;

        }

        public async Task<bool> AddMeetingUserGroup(UserGroup userGroup, Meeting meeting)
        {
            lastError = null;
            string Query = "INSERT INTO Meeting_Group(MeetingID, UserGroupID) VALUES(@MeetingID, @UserGroupID)";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meeting.meetingID));
                        cmd.Parameters.Add(new SqlParameter("@UserGroupID", userGroup.groupID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return false;
            }
        }

        public async Task<bool> DeleteMeetingAsync(int meetingID)
        {
            lastError = null;
            string Query = "DELETE FROM Meeting WHERE MeetingID=@MeetingID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meetingID));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return false;

        }

        public async Task<int> GetMeetingAttendanceRecNum(Meeting meeting)
        {
            lastError = null;
            int num = 0;

            string Query = "SELECT MeetingID FROM Activity WHERE MeetingID=@MeetingID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meeting.meetingID));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            if (dr.GetInt32(0) == meeting.meetingID)
                            {
                                num++; 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return num;
        }

        public async Task<ObservableCollection<Rate>> GetAllRates(bool includeDisabled)
        {
            lastError = null;
            string Query = "SELECT * FROM Rate";
            ObservableCollection<Rate> rates = new ObservableCollection<Rate>();

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            if (includeDisabled == false && dr.GetBoolean(3) == true)
                            {
                                continue;
                            }
                            else
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(0);
                                rate.rateDesc = dr.GetString(1);
                                rate.rate = dr.GetFloat(2);
                                rate.isDisabled = dr.GetBoolean(3);

                                rates.Add(rate);
                            }
                        }
                    }
                }

                return rates;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return null;
            }
        }

        public async Task<bool> AddNewRate(Rate rate)
        {
            lastError = null;
            string Query = "INSERT INTO Rate(RateDesc, Rate) VALUES(@RateDesc, @Rate)";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@RateDesc", rate.rateDesc));
                        cmd.Parameters.Add(new SqlParameter("@Rate", rate.rate));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return false;
            }
        }

        public async Task<bool> UpdateRateInfo(Rate rate)
        {
            lastError = null;
            string Query = "UPDATE Rate SET RateDesc=@RateDesc, Rate=@Rate, IsDisabled=@IsDisabled WHERE RateID=@RateID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@RateID", rate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@RateDesc", rate.rateDesc));
                        cmd.Parameters.Add(new SqlParameter("@Rate", rate.rate));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", rate.isDisabled));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return false;
        }

        public async Task<Shift> GetShiftById (int shiftID)
        {
            lastError = null;
            Shift shift = new Shift();
            string Query = "SELECT * FROM Shifts JOIN Rate ON Rate.RateID=Shifts.RateID WHERE ShiftID=@ShiftID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@ShiftID", shiftID));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            shift.shiftID = dr.GetInt32(0);
                            shift.shiftName = dr.GetString(1);
                            shift.startTime = dr.GetTimeSpan(2);
                            shift.endTime = dr.GetTimeSpan(3);
                            shift.isDisabled = dr.GetBoolean(6);
                            shift.WeekendOnly = dr.GetBoolean(7);

                            if (shift.isDisabled)
                            {
                                shift.dg_isDisabled = "Disabled";
                            }
                            else
                            {
                                shift.dg_isDisabled = "Enabled";
                            }

                            Rate rate = new Rate();
                            rate.rateID = dr.GetInt32(8);
                            rate.rateDesc = dr.GetString(9);
                            rate.rate = dr.GetFloat(10);

                            shift.DefaultRate = rate;
                        }
                    }
                }

                return shift;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return null;
            }
        }

        public async Task<Shift> GetSpecialTaskShift(int locationID)
        {
            lastError = null;
            Shift shift = new Shift();
            string Query = "SELECT * FROM Shifts JOIN Rate ON Rate.RateID=Shifts.RateID WHERE ShiftName='Special Task' AND LocationID=@LocationID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            shift.shiftID = dr.GetInt32(0);
                            shift.shiftName = dr.GetString(1);
                            shift.startTime = dr.GetTimeSpan(2);
                            shift.endTime = dr.GetTimeSpan(3);
                            shift.isDisabled = dr.GetBoolean(6);
                            shift.WeekendOnly = dr.GetBoolean(7);
                            shift.locationID = dr.GetInt32(4);

                            if (shift.isDisabled)
                            {
                                shift.dg_isDisabled = "Disabled";
                            }
                            else
                            {
                                shift.dg_isDisabled = "Enabled";
                            }

                            Rate rate = new Rate();
                            rate.rateID = dr.GetInt32(8);
                            rate.rateDesc = dr.GetString(9);
                            rate.rate = dr.GetFloat(10);

                            shift.DefaultRate = rate;
                        }
                    }
                }

                return shift;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return null;
            }
        }

        public async Task<bool> AddNewShift(Shift shift)
        {
            lastError = null;
            bool IsSuccess = false;
            string Query;
            
            if (shift.shiftName == "Special Task")
            {
                Query = "INSERT INTO Shifts(ShiftName, StartTime, EndTime, LocationID, RateID, WeekendOnly, IsDisabled) VALUES(@ShiftName, '0:00:00', '23:59:59', @LocationID, @RateID, @WeekendOnly, @IsDisabled)";
            }
            else
            {
                Query = "INSERT INTO Shifts(ShiftName, StartTime, EndTime, LocationID, RateID, WeekendOnly, IsDisabled) VALUES(@ShiftName, @StartTime, @EndTime, @LocationID, @RateID, @WeekendOnly, @IsDisabled)";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@ShiftName", shift.shiftName));
                        if (shift.shiftName != "Special Task")
                        {
                            cmd.Parameters.Add(new SqlParameter("@StartTime", shift.startTime));
                            cmd.Parameters.Add(new SqlParameter("@EndTime", shift.endTime));
                        }
                        cmd.Parameters.Add(new SqlParameter("@LocationID", shift.locationID));
                        cmd.Parameters.Add(new SqlParameter("@RateID", shift.DefaultRate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@WeekendOnly", shift.WeekendOnly));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", shift.isDisabled));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return false;
            }
        }

        public async Task<bool> UpdateShiftInfo(Shift shift)
        {
            lastError = null;
            bool IsSuccess = false;
            string Query = "UPDATE Shifts SET ShiftName=@ShiftName, StartTime=@StartTime, EndTime=@EndTime, LocationID=@LocationID, RateID=@RateID, IsDisabled=@IsDisabled, WeekendOnly=@WeekendOnly WHERE ShiftID=@ShiftID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@ShiftID", shift.shiftID));
                        cmd.Parameters.Add(new SqlParameter("@ShiftName", shift.shiftName));
                        cmd.Parameters.Add(new SqlParameter("@StartTime", shift.startTime));
                        cmd.Parameters.Add(new SqlParameter("@EndTime", shift.endTime));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", shift.locationID));
                        cmd.Parameters.Add(new SqlParameter("@RateID", shift.DefaultRate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", shift.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@WeekendOnly", shift.WeekendOnly));


                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return IsSuccess;
        }

        public async Task<string> GetMinHours()
        {
            lastError = null;
            string MinHours = "";
            string Query = "SELECT SettingValue FROM Global_Settings WHERE SettingKey='MinHours'";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            MinHours = dr.GetString(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return MinHours;
        }

        public async Task<string> GetGlobalSetting(string SettingKey)
        {
            lastError = null;
            string SettingValue = "";
            string Query = "SELECT SettingValue FROM Global_Settings WHERE SettingKey=@SettingKey";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@SettingKey", SettingKey));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            if (!dr.IsDBNull(0))
                            {
                                SettingValue = dr.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return SettingValue;
        }

        public async Task<bool> UpdateGlobalSetting(string SettingKey, string SettingValue)
        {
            lastError = null;
            bool IsSuccess = false;
            string Query = "UPDATE Global_Settings SET SettingValue=@SettingValue WHERE SettingKey=@SettingKey";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@SettingKey", SettingKey));
                        cmd.Parameters.Add(new SqlParameter("@SettingValue", SettingValue));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return IsSuccess;
        }

        public async Task<Activity> GetLatestActivity(string upn, int locationID)
        {
            lastError = null;
            string Query = "SELECT TOP 1 * FROM Activity LEFT JOIN Meeting ON Meeting.MeetingID=Activity.MeetingID LEFT JOIN Shifts s1 ON s1.ShiftID=Activity.StartShift LEFT JOIN Shifts s2 ON s2.ShiftID=Activity.EndShift LEFT JOIN Rate aR ON aR.RateID=Activity.ApplicableRate LEFT JOIN Rate startRate ON startRate.RateID=s1.RateID LEFT JOIN Rate endRate ON endRate.RateID=s2.RateID LEFT JOIN Rate mRate ON mRate.RateID=Meeting.RateID WHERE UserID=@UserID AND Activity.LocationID=@LocationID ORDER BY ActivityID DESC";
            Activity activity;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", upn));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            activity = new Activity();
                            activity.ActivityID = dr.GetInt32(0);
                            activity.userID = dr.GetString(1);
                            activity.inTime = dr.GetDateTime(3);
                            activity.IsSpecialTask = dr.GetBoolean(8);
                            activity.locationID = locationID;
                            
                            // Checks if out time is null and set their values if not empty
                            if (!dr.IsDBNull(4))
                            {
                                activity.outTime = dr.GetDateTime(4);
                            }

                            // Checks if start shift is not empty and set their values
                            if (!dr.IsDBNull(5))
                            {
                                var startShift = new Shift();
                                startShift.shiftID = dr.GetInt32(20);
                                startShift.shiftName = dr.GetString(21);
                                startShift.startTime = dr.GetTimeSpan(22);
                                startShift.endTime = dr.GetTimeSpan(23);
                                startShift.WeekendOnly = dr.GetBoolean(27);
                                activity.StartShift = startShift;
                            }

                            // Checks if end shift is not empty and set their values
                            if (!dr.IsDBNull(6))
                            {
                                var endShift = new Shift();
                                endShift.shiftID = dr.GetInt32(28);
                                endShift.shiftName = dr.GetString(29);
                                endShift.startTime = dr.GetTimeSpan(30);
                                endShift.endTime = dr.GetTimeSpan(31);
                                endShift.WeekendOnly = dr.GetBoolean(35);
                                activity.EndShift = endShift;
                            }

                            // Checks if meeting is not empty and set their values
                            if (!dr.IsDBNull(7))
                            {
                                var meeting = new Meeting();
                                meeting.meetingID = dr.GetInt32(13);
                                meeting.meetingName = dr.GetString(14);
                                meeting.meetingDay = dr.GetInt32(16);
                                meeting.StartTime = dr.GetTimeSpan(19);
                                activity.meeting = meeting;
                            }

                            // Checks if approved hours is not empty and set their values
                            if (!dr.IsDBNull(9))
                            {
                                activity.ApprovedHours = dr.GetDouble(9);
                                activity.ClaimableAmount = (float)dr.GetDouble(10);
                                activity.ClaimDate = dr.GetDateTime(12);
                            }

                            // Checks if applicable rate is not empty and set their values
                            if (!dr.IsDBNull(11))
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(36);
                                rate.rateDesc = dr.GetString(37);
                                rate.rate = dr.GetFloat(38);
                                activity.ApplicableRate = rate;
                            }

                            // Checks if the start shift rate is not empty and set their values
                            if (!dr.IsDBNull(40))
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(40);
                                rate.rateDesc = dr.GetString(41);
                                rate.rate = dr.GetFloat(42);
                                activity.StartShift.DefaultRate = rate;
                            }

                            //Checks if the end shift rate is not empty and set their values
                            if (!dr.IsDBNull(44))
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(44);
                                rate.rateDesc = dr.GetString(45);
                                rate.rate = dr.GetFloat(46);
                                activity.EndShift.DefaultRate = rate;
                            }

                            if (!dr.IsDBNull(48))
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(48);
                                rate.rateDesc = dr.GetString(49);
                                rate.rate = dr.GetFloat(50);
                                activity.meeting.rate = rate;
                            }

                            return activity;
                        }
                    }
                }

                // Initializes activity when theres no result and set NoActivity to true.
                activity = new Activity();
                activity.NoActivity = true;
                return activity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        public async Task<Activity> GetLatestSignIn(string upn, int locationID)
        {
            lastError = null;
            string Query = "SELECT TOP 1 * FROM Activity LEFT JOIN Shifts s1 ON s1.ShiftID=Activity.StartShift LEFT JOIN Shifts s2 ON s2.ShiftID=Activity.EndShift LEFT JOIN Rate aR ON aR.RateID=Activity.ApplicableRate LEFT JOIN Rate startRate ON startRate.RateID=s1.RateID LEFT JOIN Rate endRate ON endRate.RateID=s2.RateID WHERE UserID=@UserID AND Activity.LocationID=@LocationID AND MeetingID IS NULL ORDER BY ActivityID DESC";
            Activity activity;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", upn));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            activity = new Activity();
                            activity.ActivityID = dr.GetInt32(0);
                            activity.userID = dr.GetString(1);
                            activity.inTime = dr.GetDateTime(3);
                            activity.IsSpecialTask = dr.GetBoolean(8);
                            activity.locationID = locationID;

                            // Checks if out time is null and set their values if not empty
                            if (!dr.IsDBNull(4))
                            {
                                activity.outTime = dr.GetDateTime(4);
                            }

                            // Checks if start shift is not empty and set their values
                            if (!dr.IsDBNull(5))
                            {
                                var startShift = new Shift();
                                startShift.shiftID = dr.GetInt32(13);
                                startShift.shiftName = dr.GetString(14);
                                startShift.startTime = dr.GetTimeSpan(15);
                                startShift.endTime = dr.GetTimeSpan(16);
                                startShift.WeekendOnly = dr.GetBoolean(20);
                                activity.StartShift = startShift;
                            }

                            // Checks if end shift is not empty and set their values
                            if (!dr.IsDBNull(6))
                            {
                                var endShift = new Shift();
                                endShift.shiftID = dr.GetInt32(21);
                                endShift.shiftName = dr.GetString(22);
                                endShift.startTime = dr.GetTimeSpan(23);
                                endShift.endTime = dr.GetTimeSpan(24);
                                endShift.WeekendOnly = dr.GetBoolean(28);
                                activity.EndShift = endShift;
                            }

                            // Checks if approved hours is not empty and set their values
                            if (!dr.IsDBNull(9))
                            {
                                activity.ApprovedHours = dr.GetDouble(9);
                                activity.ClaimableAmount = (float)dr.GetDouble(10);
                                activity.ClaimDate = dr.GetDateTime(12);
                            }

                            // Checks if the applicable rate is not empty and set their values
                            if (!dr.IsDBNull(29))
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(29);
                                rate.rateDesc = dr.GetString(30);
                                rate.rate = dr.GetFloat(31);
                                activity.ApplicableRate = rate;
                            }

                            //Checks if the start shift rate is not empty and set their values
                            if (!dr.IsDBNull(33))
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(33);
                                rate.rateDesc = dr.GetString(34);
                                rate.rate = dr.GetFloat(35);
                                activity.StartShift.DefaultRate = rate;
                            }

                            // Checks if end shift default rate is not empty and set their values
                            if (!dr.IsDBNull(11))
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(37);
                                rate.rateDesc = dr.GetString(38);
                                rate.rate = dr.GetFloat(39);
                                activity.EndShift.DefaultRate = rate;
                            }

                            return activity;
                        }
                    }
                }

                // Initializes activity when theres no result and set NoActivity to true.
                activity = new Activity();
                activity.NoActivity = true;
                return activity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        public async Task<Activity> GetLatestMeeting(string upn, int locationID)
        {
            lastError = null;
            string Query = "SELECT TOP 1 * FROM Activity LEFT JOIN Meeting ON Meeting.MeetingID=Activity.MeetingID LEFT JOIN Rate mR ON mR.RateID=Meeting.RateID LEFT JOIN Rate aR ON aR.RateID=Activity.ApplicableRate WHERE UserID=@UserID AND Activity.LocationID=@LocationID AND Activity.MeetingID IS NOT NULL ORDER BY ActivityID DESC";
            Activity activity;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", upn));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            activity = new Activity();
                            activity.ActivityID = dr.GetInt32(0);
                            activity.userID = dr.GetString(1);
                            activity.inTime = dr.GetDateTime(3);
                            activity.IsSpecialTask = dr.GetBoolean(8);
                            activity.locationID = locationID;

                            var meeting = new Meeting();
                            meeting.meetingID = dr.GetInt32(13);
                            meeting.meetingName = dr.GetString(14);
                            meeting.meetingDay = dr.GetInt32(16);

                            var meetingRate = new Rate();
                            meetingRate.rateID = dr.GetInt32(20);
                            meetingRate.rateDesc = dr.GetString(21);
                            meetingRate.rate = dr.GetFloat(22);
                            meeting.rate = meetingRate;

                            activity.meeting = meeting;

                            // Checks if sign out time is not empty and set their values
                            if (!dr.IsDBNull(4))
                            {
                                activity.outTime = dr.GetDateTime(4);

                                activity.ApprovedHours = dr.GetDouble(9);
                                activity.ClaimableAmount = (float)dr.GetDouble(10);
                                activity.ClaimDate = dr.GetDateTime(12);

                                var aRate = new Rate();
                                aRate.rateID = dr.GetInt32(24);
                                aRate.rateDesc = dr.GetString(25);
                                aRate.rate = dr.GetFloat(26);
                                activity.ApplicableRate = aRate;
                            }

                            return activity;
                        }
                    }
                }

                // Initializes activity when theres no result and set NoActivity to true.
                activity = new Activity();
                activity.NoActivity = true;
                return activity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }

        public async Task<double> GetApprovedHours(string upn)
        {
            lastError = null;
            string Query = "SELECT SUM(ApprovedHours) as 'ApprovedHours' FROM Activity WHERE UserID=@UserID AND ClaimDate >= DATEFROMPARTS(year(GETDATE()),month(GETDATE()),1)";
            double approvedHours = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", upn));

                        SqlDataReader dr = await cmd.ExecuteReaderAsync();
                        while (dr.Read())
                        {
                            if (!dr.IsDBNull(0))
                            {
                                approvedHours = dr.GetDouble(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); lastError = ex;
            }

            return approvedHours;
        }

        public async Task<bool> AddNewActivity(Activity activity)
        {
            lastError = null;
            string Query;

            if (activity.meeting != null)
            {
                Query = "INSERT INTO Activity(UserID, LocationID, inTime, meetingID) VALUES(@UserID, @LocationID, @InTime, @MeetingID)";
            }
            else if (activity.IsSpecialTask == true)
            {
                Query = "INSERT INTO Activity(UserID, LocationID, inTime, startShift, endShift, SpecialTask) VALUES(@UserID, @LocationID, @InTime, @StartShift, @EndShift, 'true')";
            }
            else
            {
                Query = "INSERT INTO Activity(UserID, LocationID, inTime, startShift, endShift) VALUES(@UserID, @LocationID, @InTime, @StartShift, @EndShift)";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@UserID", activity.userID));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", activity.locationID));
                        cmd.Parameters.Add(new SqlParameter("@InTime", activity.inTime));

                        if (activity.meeting != null)
                        {
                            cmd.Parameters.Add(new SqlParameter("@MeetingID", activity.meeting.meetingID));
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@StartShift", activity.StartShift.shiftID));
                            cmd.Parameters.Add(new SqlParameter("@EndShift", activity.EndShift.shiftID));
                        }

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
                return false;
            }
        }

        public async Task<bool> UpdateActivityInfo(Activity activity)
        {
            lastError = null;
            string Query = "UPDATE Activity SET OutTime=@OutTime, ApprovedHours=@ApprovedHours, ClaimableAmount=@ClaimableAmount, ApplicableRate=@ApplicableRate, ClaimDate=@ClaimDate WHERE ActivityID=@ActivityID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@OutTime", activity.outTime));
                        cmd.Parameters.Add(new SqlParameter("@ApprovedHours", activity.ApprovedHours));
                        cmd.Parameters.Add(new SqlParameter("@ClaimableAmount", activity.ClaimableAmount));
                        cmd.Parameters.Add(new SqlParameter("@ClaimDate", activity.ClaimDate));
                        cmd.Parameters.Add(new SqlParameter("@ApplicableRate", activity.ApplicableRate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@ActivityID", activity.ActivityID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); lastError = ex;
                return false;
            }
        }

        public async Task<Meeting> GetMeetingById(int MeetingID)
        {
            lastError = null;
            string Query = "SELECT * FROM Meeting JOIN Rate on Rate.RateID=Meeting.RateID WHERE MeetingID=@MeetingID";
            try
            {
                Meeting meeting;

                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@MeetingID", MeetingID));
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                meeting = new Meeting();
                                meeting.meetingID = dr.GetInt32(0);
                                meeting.meetingName = dr.GetString(1);
                                meeting.locationID = dr.GetInt32(2);
                                meeting.meetingDay = dr.GetInt32(3);
                                meeting.isDisabled = dr.GetBoolean(4);

                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(5);
                                rate.rateDesc = dr.GetString(8);
                                rate.rate = dr.GetFloat(9);
                                rate.isDisabled = dr.GetBoolean(10);
                                
                                return meeting;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message); 
                lastError = ex;
            }

            return null;
        }
    }
}
