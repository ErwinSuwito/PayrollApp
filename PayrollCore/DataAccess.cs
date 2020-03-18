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
            }

            return null;
        }

        /// <summary>
        /// Get meetings that are in the passed location.
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<Meeting>> GetMeetings(Location appLocation)
        {
            const string GetLocationsQuery = "SELECT * FROM Meeting WHERE LocationID=@LocationID";

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

                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(6);
                                rate.rateDesc = dr.GetString(7);
                                rate.rate = dr.GetFloat(8);
                                rate.isDisabled = dr.GetBoolean(9);

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
            }

            return null;
        }

        /// <summary>
        /// Get location information about the passed location name
        /// </summary>
        /// <param name="selectedLocation"></param>
        /// <returns></returns>
        public Location GetLocationById(string selectedLocation)
        {
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
                        using (SqlDataReader dr = cmd.ExecuteReader())
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
            string Query = "UPDATE Location SET LocationName=@LocationName, EnabledGM=@EnableGM, IsDisabled=@IsDisabled WHERE LocationID=@LocationID";

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
            }

            return false;
        }

        /// <summary>
        /// Adds a new location into the database
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<bool> AddLocationAsync(Location location)
        {
            string Query = "INSERT INTO locations(LocationName, EnableGM, IsDisabled) VALUES(@LocationName, @EnableGM, 'false')";

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

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Saves the passed meeting into the database
        /// </summary>
        /// <param name="location">The location where the meeting is going to take place.</param>
        /// <param name="meetings">ObservableCollection of meetings.</param>
        /// <returns></returns>
        public async Task<Boolean> SaveMeetingSettings(Meeting meeting)
        {
            string Query;

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    Query = "UPDATE Meeting SET MeetingName=@MeetingName, MeetingDay=@MeetingDay, IsDisabled=@DisableMeeting, RateID=@RateID WHERE MeetingID=@MeetingID";

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meeting.meetingID));
                        cmd.Parameters.Add(new SqlParameter("@MeetingName", meeting.meetingName));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", meeting.locationID));
                        cmd.Parameters.Add(new SqlParameter("@MeetingDay", meeting.meetingDay));
                        cmd.Parameters.Add(new SqlParameter("@DisableMeeting", meeting.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@RateID", meeting.rate.rateID));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
            }

            return false;
        }

        public async Task<int> SaveMeetingAndReturnId(Meeting meeting)
        {
            string Query = "INSERT INTO meetings(MeetingName, LocationID, MeetingDay, IsDisabled, RateID) VALUES(@MeetingName, @LocationID, @MeetingDay, @DisableMeeting, @RateID) select SCOPE_IDENTITY()";
            
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

                        var savedMeeting = await cmd.ExecuteScalarAsync();

                        int.TryParse(savedMeeting.ToString(), out int meetingID);

                        return meetingID;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
            }

            return -1;
        }

        /// <summary>
        /// Get all users in the database.
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<User>> GetUsersList()
        {
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

                                if (shift.isDisabled)
                                {
                                    shift.dg_isDisabled = "Disabled";
                                }
                                else
                                {
                                    shift.dg_isDisabled = "Enabled";
                                }

                                Rate rate = new Rate();
                                rate.rateID = dr.GetInt32(7);
                                rate.rateDesc = dr.GetString(8);
                                rate.rate = dr.GetFloat(9);

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
                return null;
            }
            
        }

        /// <summary>
        /// Get all user groups
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<UserGroup>> GetAllUserGroups()
        {
            string Query = "SELECT * FROM usr_group JOIN rate ON rate.rateID=usr_group.RateID";
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
                return null;
            }

        }

        /// <summary>
        /// Get contents of meeting_group table based on the passed userGroup
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<List<MeetingUserGroup>> GetMeetingUserGroupByUserGroup(int userGroup)
        {
            List<MeetingUserGroup> meetingUserGroups = new List<MeetingUserGroup>();

            string Query = "SELECT * FROM Meeting_Group WHERE UserGroupID=@UserGroup";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserGroup", userGroup));

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
                return null;
            }

        }

        public async Task<bool> DeleteMeetingUserGroup(int meetingID)
        {
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
            }

            return false;

        }

        public async Task<bool> AddMeetingUserGroup(UserGroup userGroup, Meeting meeting)
        {
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
                return false;
            }
        }

        public async Task<bool> DeleteMeetingAsync(int meetingID)
        {
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
            }

            return false;

        }

        public async Task<int> GetMeetingAttendanceRecNum(Meeting meeting)
        {
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
            }

            return num;
        }

        public async Task<ObservableCollection<Rate>> GetAllRates(bool includeDisabled)
        {
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
                return null;
            }
        }

        public async Task<bool> AddNewRate(Rate rate)
        {
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
                return false;
            }
        }

        public async Task<bool> UpdateRateInfo(Rate rate)
        {
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
            }

            return false;
        }

        public async Task<Shift> GetShiftById (int shiftID)
        {
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
                            shift.locationID = dr.GetInt32(4);
                            shift.isDisabled = dr.GetBoolean(6);

                            Rate rate = new Rate();
                            rate.rateID = dr.GetInt32(7);
                            rate.rateDesc = dr.GetString(8);
                            rate.rate = dr.GetFloat(9);

                            shift.DefaultRate = rate;
                        }
                    }
                }

                return shift;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> AddNewShift(Shift shift)
        {
            bool IsSuccess = false;
            string Query = "INSERT INTO Shifts(ShiftName, StartTime, EndTime, LocationID, RateID) VALUES(@ShiftName, @StartTime, @EndTime, @LocationID, @RateID)";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;

                        cmd.Parameters.Add(new SqlParameter("@ShiftName", shift.shiftName));
                        cmd.Parameters.Add(new SqlParameter("@StartTime", shift.startTime));
                        cmd.Parameters.Add(new SqlParameter("@EndTime", shift.endTime));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", shift.locationID));
                        cmd.Parameters.Add(new SqlParameter("@RateID", shift.DefaultRate.rateID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return false;
            }


            return IsSuccess;
        }

        public async Task<bool> UpdateShiftInfo(Shift shift)
        {
            bool IsSuccess = false;
            string Query = "UPDATE Shifts SET ShiftName=@ShiftName, StartTime=@StartTime, EndTime=@EndTime, LocationID=@LocationID, RateID=@RateID, IsDisabled=@IsDisabled WHERE ShiftID=@ShiftID";

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


                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
            }

            return IsSuccess;
        }

        public async Task<string> GetMinHours()
        {
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
            }

            return MinHours;
        }

        public async Task<string> GetGlobalSetting(string SettingKey)
        {
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
            }

            return SettingValue;
        }

        public async Task<bool> UpdateGlobalSetting(string SettingKey, string SettingValue)
        {
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
            }

            return IsSuccess;
        }

        public async Task<Activity> GetLatestActivityByUserId(string upn, int locationID)
        {
            string Query = "SELECT TOP 1 * FROM Activity LEFT JOIN Meeting ON Meeting.MeetingID=Activity.MeetingID LEFT JOIN Shifts s1 ON s1.ShiftID=Activity.StartShift LEFT JOIN Shifts s2 ON s2.ShiftID=Activity.EndShift LEFT JOIN Rate ON Rate.RateID=Activity.ApplicableRate WHERE UserID=@UserID AND Activity.LocationID=@LocationID ORDER BY inTime DESC";
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
                            activity.userID = upn;
                            activity.inTime = dr.GetDateTime(3);
                            activity.IsSpecialTask = dr.GetBoolean(8);
                            
                            // Checks if out time is null and set their values if not empty
                            if (!dr.IsDBNull(4))
                            {
                                activity.outTime = dr.GetDateTime(4);
                            }

                            // Checks if start shift is not empty and set their values
                            if (!dr.IsDBNull(5))
                            {
                                var startShift = new Shift();
                                startShift.shiftID = dr.GetInt32(19);
                                startShift.shiftName = dr.GetString(20);
                                startShift.startTime = dr.GetTimeSpan(21);
                                startShift.endTime = dr.GetTimeSpan(22);
                                activity.StartShift = startShift;
                            }

                            // Checks if end shift is not empty and set their values
                            if (!dr.IsDBNull(6))
                            {
                                var endShift = new Shift();
                                endShift.shiftID = dr.GetInt32(26);
                                endShift.shiftName = dr.GetString(27);
                                endShift.startTime = dr.GetTimeSpan(28);
                                endShift.endTime = dr.GetTimeSpan(29);
                                activity.EndShift = endShift;
                            }

                            // Checks if meeting is not empty and set their values
                            if (!dr.IsDBNull(7))
                            {
                                var meeting = new Meeting();
                                meeting.meetingID = dr.GetInt32(13);
                                meeting.meetingName = dr.GetString(14);
                                meeting.meetingDay = dr.GetInt32(16);
                                activity.meeting = meeting;
                            }

                            // Checks if approved hours is not empty and set their values
                            if (!dr.IsDBNull(9))
                            {
                                activity.ApprovedHours = dr.GetFloat(9);
                                activity.ClaimableAmount = dr.GetFloat(10);
                                activity.ClaimDate = dr.GetDateTime(12);
                            }

                            if (!dr.IsDBNull(11))
                            {
                                var rate = new Rate();
                                rate.rateID = dr.GetInt32(34);
                                rate.rateDesc = dr.GetString(35);
                                rate.rate = dr.GetDouble(36);
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
            }

            return null;
        }

        public async Task<double> GetApprovedHours(string upn)
        {
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
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
            }

            return approvedHours;
        }

        public async Task<bool> AddNewActivity(Activity activity)
        {
            string Query;

            if (activity.meeting != null)
            {
                Query = "INSERT INTO Activity(UserID, LocationID, inTime, meetingID) VALUES(@UserID, @LocationID, @InTime, @MeetingID)";
            }
            else if (activity.IsSpecialTask == true)
            {
                Query = "INSERT INTO Activity(UserID, LocationID, inTime, SpecialTask) VALUES(@UserID, @LocationID, @InTime, 'true')";
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
                        else if (activity.IsSpecialTask == true)
                        {
                            
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
                return false;
            }
        }

        public async Task<bool> UpdateActivityInfo(Activity activity)
        {
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

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return false;
            }
        }

    }
}
