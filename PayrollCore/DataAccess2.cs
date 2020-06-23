using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    public class DataAccess2
    {
        string DbConnString;
        string CardConnString;
        public Exception lastError;

        public void StoreConnStrings(string dbConnString, string cardConnString)
        {
            DbConnString = dbConnString;
            CardConnString = cardConnString;
        }

        /// <summary>
        /// Tries to connect to the database using the passed connection string
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        public async Task<bool> TestConnString(string connString)
        {
            lastError = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    await conn.OpenAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return false;
            }
        }

        #region Rate
        /// <summary>
        /// Gets the requested Rate
        /// </summary>
        /// <param name="RateID"></param>
        /// <returns></returns>
        public async Task<Rate> GetRateById(int RateID)
        {
            lastError = null;

            string Query = "SELECT * FROM Rate WHERE RateID=@RateID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@RateID", RateID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                Rate rate = new Rate();
                                rate.rateID = dr.GetInt32(0);
                                rate.rateDesc = dr.GetString(1);
                                rate.rate = dr.GetFloat(2);
                                rate.isDisabled = dr.GetBoolean(3);

                                return rate;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Get all rates on the database and returns it in an ObservableCollection
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<Rate>> GetAllRatesAsync(bool GetDisabled)
        {
            lastError = null;
            string Query = "SELECT * FROM Rate";

            if (!GetDisabled)
            {
                Query += " WHERE IsDisabled=0";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Rate> rates = new ObservableCollection<Rate>();

                            while (dr.Read())
                            {
                                Rate rate = new Rate();
                                rate.rateID = dr.GetInt32(0);
                                rate.rateDesc = dr.GetString(1);
                                rate.rate = dr.GetFloat(2);
                                rate.isDisabled = dr.GetBoolean(3);

                                rates.Add(rate);
                            }

                            return rates;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds a rate to the database
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public async Task<bool> AddRateAsync(Rate rate)
        {
            lastError = null;

            string Query = "INSERT INTO rate(rateDesc, rate) VALUES(@RateDesc, @Rate)";
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
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes the specified rate
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRateAsync(Rate rate)
        {
            string Query = "DELETE FROM Rate WHERE RateID=@RateID";
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@RateID", rate.rateID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates the specified rate
        /// </summary>
        /// <param name="rate">The rate to be updated</param>
        /// <returns></returns>
        public async Task<bool> UpdateRateAsync(Rate rate)
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
                        cmd.Parameters.Add(new SqlParameter("@RateDesc", rate.rateDesc));
                        cmd.Parameters.Add(new SqlParameter("@Rate", rate.rate));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", rate.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@RateID", rate.rateID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region User Group
        /// <summary>
        /// Gets the requested user group
        /// </summary>
        /// <param name="GroupID"></param>
        /// <returns></returns>
        public async Task<UserGroup> GetUserGroupByIdAsync(int GroupID)
        {
            string Query = "SELECT * FROM user_group WHERE GroupID=@GroupID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@GroupID", GroupID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                UserGroup userGroup = new UserGroup();
                                userGroup.groupID = dr.GetInt32(0);
                                userGroup.groupName = dr.GetString(1);
                                userGroup.DefaultRate = new Rate() { rateID = dr.GetInt32(2) };
                                userGroup.ShowAdminSettings = dr.GetBoolean(3);
                                userGroup.EnableFaceRec = dr.GetBoolean(4);

                                return userGroup;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets all user groups and returns in an ObservableCollection
        /// </summary>
        /// <param name="GetDisabled">True to include disabled user groups</param>
        /// <returns></returns>
        public async Task<ObservableCollection<UserGroup>> GetAllUserGroupsAsync(bool GetDisabled)
        {
            lastError = null;
            string Query = "SELECT * FROM user_group";

            if (!GetDisabled)
            {
                Query += " WHERE IsDisabled=0";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<UserGroup> userGroups = new ObservableCollection<UserGroup>();

                            while (dr.Read())
                            {
                                UserGroup userGroup = new UserGroup();
                                userGroup.groupID = dr.GetInt32(0);
                                userGroup.groupName = dr.GetString(1);
                                userGroup.DefaultRate = new Rate() { rateID = dr.GetInt32(2) };
                                userGroup.ShowAdminSettings = dr.GetBoolean(3);
                                userGroup.EnableFaceRec = dr.GetBoolean(4);

                                userGroups.Add(userGroup);
                            }

                            return userGroups;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds a new user group
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> AddNewUserGroupAsync(UserGroup userGroup)
        {
            lastError = null;

            string Query = "INSERT INTO user_group(GroupName, RateID, ShowAdminSettings, EnableFaceRect) VALUES(@GroupID, @GroupName, @RateID, @ShowAdminSettings, @EnableFaceRec)";
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
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes the specified user group from the database
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserGroupAsync(UserGroup userGroup)
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
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates the specified rate
        /// </summary>
        /// <param name="rate">The rate to be updated</param>
        /// <returns></returns>
        public async Task<bool> UpdateUserGroupAsync(UserGroup userGroup)
        {
            lastError = null;

            string Query = "UPDATE user_group SET GroupName=@GroupName, RateID=@RateID, ShowAdminSettings=@ShowAdminSettings, EnableFaceRec=@EnableFaceRec, IsDisabled=@IsDisabled WHERE GroupID=@GroupID";
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
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", userGroup.IsDisabled));
                        cmd.Parameters.Add(new SqlParameter("@GroupID", userGroup.groupID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region User

        /// <summary>
        /// Gets the requested user
        /// </summary>
        /// <param name="GroupID"></param>
        /// <returns></returns>
        public async Task<User> GetUserByIdAsync(string userID)
        {
            string Query = "SELECT * FROM Users WHERE UserID=@UserID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", userID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                User user = new User();
                                user.userID = dr.GetString(0);
                                user.fullName = dr.GetString(1);
                                user.fromAD = dr.GetBoolean(2);
                                user.isDisabled = dr.GetBoolean(3);
                                user.userGroup = new UserGroup() { groupID = dr.GetInt32(4) };

                                return user;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets all users and returns in an ObservableCollection
        /// </summary>
        /// <param name="GetDisabled">True to include disabled user groups</param>
        /// <returns></returns>
        public async Task<ObservableCollection<User>> GetAllUsersAsync(bool GetDisabled)
        {
            lastError = null;
            string Query = "SELECT * FROM Users";

            if (!GetDisabled)
            {
                Query += " WHERE IsDisabled=0";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<User> users = new ObservableCollection<User>();

                            while (dr.Read())
                            {
                                User user = new User();
                                user.userID = dr.GetString(0);
                                user.fullName = dr.GetString(1);
                                user.fromAD = dr.GetBoolean(2);
                                user.isDisabled = dr.GetBoolean(3);
                                user.userGroup = new UserGroup() { groupID = dr.GetInt32(4) };

                                users.Add(user);
                            }

                            return users;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds a new user group
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> AddNewUserAsync(User user)
        {
            lastError = null;

            string Query = "INSERT INTO Users(UserID, FullName, FromAD, GroupID) VALUES(@UserID, @FullName, @FromAD, @GroupID)";
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", user.userID));
                        cmd.Parameters.Add(new SqlParameter("@FullName", user.fullName));
                        cmd.Parameters.Add(new SqlParameter("@FromAD", user.fromAD));
                        cmd.Parameters.Add(new SqlParameter("@GroupID", user.userGroup.groupID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes the specified user group from the database
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserAsync(User user)
        {
            string Query = "DELETE FROM Users WHERE UserID=@UserID";
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", user.userID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates the specified rate
        /// </summary>
        /// <param name="rate">The rate to be updated</param>
        /// <returns></returns>
        public async Task<bool> UpdateUserAsync(User user)
        {
            lastError = null;

            string Query = "UPDATE Users SET FullName=@FullName, FromAD=@FromAD, IsDisabled=@IsDisabled, GroupID=@GroupID WHERE UserID=@UserID";
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", user.userID));
                        cmd.Parameters.Add(new SqlParameter("@FullName", user.fullName));
                        cmd.Parameters.Add(new SqlParameter("@FromAD", user.fromAD));
                        cmd.Parameters.Add(new SqlParameter("@GroupID", user.userGroup.groupID));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", user.userGroup.IsDisabled));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Location

        /// <summary>
        /// Gets the requested by its ID
        /// </summary>
        /// <param name="LocationID"></param>
        /// <returns></returns>
        public async Task<Location> GetLocationByIdAsync(int LocationID)
        {
            string Query = "SELECT * FROM Location WHERE locationID=@LocationID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", LocationID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                Location location = new Location();
                                location.locationID = dr.GetInt32(0);
                                location.locationName = dr.GetString(1);
                                location.enableGM = dr.GetBoolean(2);
                                location.isDisabled = dr.GetBoolean(3);
                                location.updateLvString();

                                return location;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets all location on the database
        /// </summary>
        /// <param name="GetDisabled">True to also get disabled locations</param>
        /// <returns></returns>
        public async Task<ObservableCollection<Location>> GetAllLocationAsync(bool GetDisabled)
        {
            lastError = null;
            string Query = "SELECT * FROM Location";

            if (!GetDisabled)
            {
                Query += " WHERE IsDisabled=0";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Location> locations = new ObservableCollection<Location>();

                            while (dr.Read())
                            {
                                Location location = new Location();
                                location.locationID = dr.GetInt32(0);
                                location.locationName = dr.GetString(1);
                                location.enableGM = dr.GetBoolean(2);
                                location.isDisabled = dr.GetBoolean(3);
                                location.updateLvString();

                                locations.Add(location);
                            }

                            return locations;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds a new location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<int> AddNewLocationAsync(Location location)
        {
            lastError = null;

            string Query = "INSERT INTO Location(LocationName, EnableGM, IsDisabled) VALUES(@LocationName, @EnableGM, @IsDisabled) select SCOPE_IDENTITY()";
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
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", location.isDisabled));

                        var _locationID = await cmd.ExecuteScalarAsync();
                        int.TryParse(_locationID.ToString(), out int locationID);
                        return locationID;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Deletes the specified location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<bool> DeleteLocationAsync(Location location)
        {
            string Query = "DELETE FROM Location WHERE LocationID=@LocationID";
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", location.locationID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates the specified location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task<bool> UpdateLocationAsync(Location location)
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
                        cmd.Parameters.Add(new SqlParameter("@LocationID", location.locationID));
                        cmd.Parameters.Add(new SqlParameter("@LocationName", location.locationName));
                        cmd.Parameters.Add(new SqlParameter("@EnableGM", location.enableGM));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", location.isDisabled));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Shfit

        /// <summary>
        /// Get the requested shift
        /// </summary>
        /// <param name="ShiftID"></param>
        /// <returns></returns>
        public async Task<Shift> GetShiftByIdAsync(int ShiftID)
        {
            string Query = "SELECT * FROM Shifts WHERE ShiftID=@ShiftID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@ShiftID", ShiftID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                Shift shift = new Shift();
                                shift.shiftID = dr.GetInt32(0);
                                shift.shiftName = dr.GetString(1);
                                shift.startTime = dr.GetTimeSpan(2);
                                shift.endTime = dr.GetTimeSpan(3);
                                shift.locationID = dr.GetInt32(4);
                                shift.DefaultRate = new Rate() { rateID = dr.GetInt32(5) };
                                shift.isDisabled = dr.GetBoolean(6);
                                shift.WeekendOnly = dr.GetBoolean(7);

                                return shift;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Get all shifts in the table
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Shift>> GetAllShiftsAsync(bool GetDisabled)
        {
            lastError = null;
            string Query = "SELECT * FROM Shifts";

            if (!GetDisabled)
            {
                Query += " WHERE IsDisabled=0";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Shift> shifts = new ObservableCollection<Shift>();

                            while (dr.Read())
                            {
                                Shift shift = new Shift();
                                shift.shiftID = dr.GetInt32(0);
                                shift.shiftName = dr.GetString(1);
                                shift.startTime = dr.GetTimeSpan(2);
                                shift.endTime = dr.GetTimeSpan(3);
                                shift.locationID = dr.GetInt32(4);
                                shift.DefaultRate = new Rate() { rateID = dr.GetInt32(5) };
                                shift.isDisabled = dr.GetBoolean(6);
                                shift.WeekendOnly = dr.GetBoolean(7);

                                shifts.Add(shift);
                            }

                            return shifts;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets all shifts on the specified location
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Shift>> GetAllShiftsAsync(bool GetDisabled, int locationID)
        {
            lastError = null;
            string Query = "SELECT * FROM Shifts WHERE LocationID=@LocationID";

            if (!GetDisabled)
            {
                Query += " AND IsDisabled=0";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Shift> shifts = new ObservableCollection<Shift>();

                            while (dr.Read())
                            {
                                Shift shift = new Shift();
                                shift.shiftID = dr.GetInt32(0);
                                shift.shiftName = dr.GetString(1);
                                shift.startTime = dr.GetTimeSpan(2);
                                shift.endTime = dr.GetTimeSpan(3);
                                shift.locationID = dr.GetInt32(4);
                                shift.DefaultRate = new Rate() { rateID = dr.GetInt32(5) };
                                shift.isDisabled = dr.GetBoolean(6);
                                shift.WeekendOnly = dr.GetBoolean(7);

                                shifts.Add(shift);
                            }

                            return shifts;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds a new shift
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
        public async Task<bool> AddNewShiftAsync(Shift shift)
        {
            lastError = null;

            string Query = "INSERT INTO Shifts(ShiftName, StartTime, EndTime, LocationID, RateID, WeekendOnly) VALUES(@ShiftName, @StartTime, @EndTime, @LocationID, @RateID, @WeekendOnly)";
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
                        cmd.Parameters.Add(new SqlParameter("@WeekendOnly", shift.WeekendOnly));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes the specified shift
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
        public async Task<bool> DeleteShiftAsync(Shift shift)
        {
            string Query = "DELETE FROM Shifts WHERE ShiftID=@ShiftID";
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@ShiftID", shift.shiftID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates the specified shift
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
        public async Task<bool> UpdateShiftAsync(Shift shift)
        {
            lastError = null;

            string Query = "UPDATE Shifts SET ShiftName=@ShiftName, StartTime=@StartTime, EndTime=@EndTIme, LocationID=@LocationID, RateID=@RateID, WeekendOnly=@WeekendOnly, IsDisabled=@IsDisabled WHERE ShiftID=@ShiftID";
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
                        cmd.Parameters.Add(new SqlParameter("@WeekendOnly", shift.WeekendOnly));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", shift.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@ShiftID", shift.shiftID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Meeting

        /// <summary>
        /// Gets the requested meeting
        /// </summary>
        /// <param name="MeetingID"></param>
        /// <returns></returns>
        public async Task<Meeting> GetMeetingByIdAsync(int MeetingID)
        {
            string Query = "SELECT * FROM Meeting WHERE MeetingID=@MeetingID";

            try
            {
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
                                Meeting meeting = new Meeting();
                                meeting.meetingID = dr.GetInt32(0);
                                meeting.meetingName = dr.GetString(1);
                                meeting.locationID = dr.GetInt32(2);
                                meeting.meetingDay = dr.GetInt32(3);
                                meeting.isDisabled = dr.GetBoolean(4);
                                meeting.rate = new Rate() { rateID = dr.GetInt32(5) };
                                meeting.StartTime = dr.GetTimeSpan(6);

                                return meeting;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets all meetings in the database
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Meeting>> GetAllMeetingsAsync(bool GetDisabled)
        {
            lastError = null;
            string Query = "SELECT * FROM Meetings";

            if (!GetDisabled)
            {
                Query += " WHERE IsDisabled=0";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Meeting> meetings = new ObservableCollection<Meeting>();

                            while (dr.Read())
                            {
                                Meeting meeting = new Meeting();
                                meeting.meetingID = dr.GetInt32(0);
                                meeting.meetingName = dr.GetString(1);
                                meeting.locationID = dr.GetInt32(2);
                                meeting.meetingDay = dr.GetInt32(3);
                                meeting.isDisabled = dr.GetBoolean(4);
                                meeting.rate = new Rate() { rateID = dr.GetInt32(5) };
                                meeting.StartTime = dr.GetTimeSpan(6);

                                meetings.Add(meeting);
                            }

                            return meetings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets all the meetings on the specified location
        /// </summary>
        /// <param name="GetDisabled"></param>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Meeting>> GetAllMeetingsAsync(bool GetDisabled, int locationID)
        {
            lastError = null;
            string Query = "SELECT * FROM Meetings WHERE LocationID=@LocationID";

            if (!GetDisabled)
            {
                Query += " AND IsDisabled=0";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Meeting> meetings = new ObservableCollection<Meeting>();

                            while (dr.Read())
                            {
                                Meeting meeting = new Meeting();
                                meeting.meetingID = dr.GetInt32(0);
                                meeting.meetingName = dr.GetString(1);
                                meeting.locationID = dr.GetInt32(2);
                                meeting.meetingDay = dr.GetInt32(3);
                                meeting.isDisabled = dr.GetBoolean(4);
                                meeting.rate = new Rate() { rateID = dr.GetInt32(5) };
                                meeting.StartTime = dr.GetTimeSpan(6);

                                meetings.Add(meeting);
                            }

                            return meetings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds a new meeting
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        public async Task<int> AddNewMeetingAsync(Meeting meeting)
        {
            lastError = null;

            string Query = "INSERT INTO Meeting(MeetingName, LocationID, MeetingDay, RateID, StartTime) VALUES(@MeetingName, @LocationID, @MeetingDay, @RateID, @StartTime) select SCOPE_IDENTITY()";
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
                        cmd.Parameters.Add(new SqlParameter("@RateID", meeting.rate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@StartTime", meeting.StartTime));

                        var _meetingID = await cmd.ExecuteScalarAsync();
                        int.TryParse(_meetingID.ToString(), out int meetingID);
                        return meetingID;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Deletes the specified meeting
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        public async Task<bool> DeleteMeetingAsync(Meeting meeting)
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
                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meeting.meetingID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates the specified meeting
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        public async Task<bool> UpdateMeetingAsync(Meeting meeting)
        {
            lastError = null;

            string Query = "UPDATE Meeting SET MeetingName=@MeetingName, LocationID=@LocationID, MeetingDay=@MeetingDay, IsDisabled=@IsDisabled, RateID=@RateID, StartTime=@StartTime WHERE MeetingID=@MeetingID";
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
                        cmd.Parameters.Add(new SqlParameter("@RateID", meeting.rate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@StartTime", meeting.StartTime));
                        cmd.Parameters.Add(new SqlParameter("@IsDisabled", meeting.isDisabled));
                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meeting.meetingID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets meetings based on the selected location, user group and meeting day
        /// </summary>
        /// <param name="locationID"></param>
        /// <param name="userGroupId"></param>
        /// <param name="meetingDay"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Meeting>> GetMeetingsAsync(int locationID, int userGroupId, int meetingDay)
        {
            lastError = null;
            string Query = "SELECT * FROM Meeting JOIN Meeting_Group ON Meeting_Group.MeetingID = Meeting.MeetingID WHERE LocationID=@LocationID Meeting_Group.UserGroupID=@UserGroupID AND MeetingDay=@MeetingDay";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));
                        cmd.Parameters.Add(new SqlParameter("@UserGroupID", userGroupId));
                        cmd.Parameters.Add(new SqlParameter("@MeetingDay", meetingDay));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {

                            ObservableCollection<Meeting> meetings = new ObservableCollection<Meeting>();

                            while (dr.Read())
                            {
                                Meeting meeting = new Meeting();
                                meeting.meetingID = dr.GetInt32(0);
                                meeting.meetingName = dr.GetString(1);
                                meeting.locationID = dr.GetInt32(2);
                                meeting.meetingDay = dr.GetInt32(3);
                                meeting.isDisabled = dr.GetBoolean(4);
                                meeting.rate = new Rate() { rateID = dr.GetInt32(5) };
                                meeting.StartTime = dr.GetTimeSpan(6);

                                meetings.Add(meeting);
                            }

                            return meetings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        #endregion

        #region MeetingGroup

        /// <summary>
        /// Gets the requested MeetingUserGroup
        /// </summary>
        /// <param name="GroupID"></param>
        /// <returns></returns>
        public async Task<MeetingUserGroup> GetMeetingGroupById(int GroupID)
        {
            string Query = "SELECT * FROM Meeting_Group WHERE meeting_group_id=@GroupID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@GroupID", GroupID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                MeetingUserGroup meetingGroup = new MeetingUserGroup();
                                meetingGroup.meeting_group_id = dr.GetInt32(0);
                                meetingGroup.meetingID = dr.GetInt32(1);
                                meetingGroup.usrGroupId = dr.GetInt32(2);

                                return meetingGroup;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets all MeetingUserGroup
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<MeetingUserGroup>> GetAllMeetingGroupAsync()
        {
            lastError = null;
            string Query = "SELECT * FROM Meeting_Group";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<MeetingUserGroup> meetingGroups = new ObservableCollection<MeetingUserGroup>();

                            while (dr.Read())
                            {
                                MeetingUserGroup meetingGroup = new MeetingUserGroup();
                                meetingGroup.meeting_group_id = dr.GetInt32(0);
                                meetingGroup.meetingID = dr.GetInt32(1);
                                meetingGroup.usrGroupId = dr.GetInt32(2);

                                meetingGroups.Add(meetingGroup);
                            }

                            return meetingGroups;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets all MeetingUserGroup of a meeting
        /// </summary>
        /// <param name="meetingID"></param>
        /// <returns></returns>
        public async Task<List<MeetingUserGroup>> GetAllMeetingGroupAsync(int meetingID)
        {
            lastError = null;
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

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            List<MeetingUserGroup> meetingGroups = new List<MeetingUserGroup>();

                            while (dr.Read())
                            {
                                MeetingUserGroup meetingGroup = new MeetingUserGroup();
                                meetingGroup.meeting_group_id = dr.GetInt32(0);
                                meetingGroup.meetingID = dr.GetInt32(1);
                                meetingGroup.usrGroupId = dr.GetInt32(2);

                                meetingGroups.Add(meetingGroup);
                            }

                            return meetingGroups;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds a new MeetingUserGroup
        /// </summary>
        /// <param name="meetingGroup"></param>
        /// <returns></returns>
        public async Task<bool> AddNewMeetingGroupAsync(MeetingUserGroup meetingGroup)
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
                        cmd.Parameters.Add(new SqlParameter("@MeetingID", meetingGroup.meetingID));
                        cmd.Parameters.Add(new SqlParameter("@UserGroupID", meetingGroup.usrGroupId));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes all MeetingUserGroup that belongs to a meeting
        /// </summary>
        /// <param name="meetingGroup"></param>
        /// <returns></returns>
        public async Task<bool> DeleteMeetingGroupAsync(int meetingID)
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

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Activity

        /// <summary>
        /// Gets the requested activity
        /// </summary>
        /// <param name="ActivityID"></param>
        /// <returns></returns>
        public async Task<Activity> GetActivityById(int ActivityID)
        {
            string Query = "SELECT * FROM Activity WHERE ActivityID=@ActivityID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@ActivityID", ActivityID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                Activity activity = new Activity();
                                activity.ActivityID = dr.GetInt32(0);
                                activity.userID = dr.GetString(1);
                                activity.locationID = dr.GetInt32(2);
                                activity.inTime = dr.GetDateTime(3);
                                
                                if (!dr.IsDBNull(4))
                                {
                                    activity.outTime = dr.GetDateTime(4);
                                }

                                if (!dr.IsDBNull(5))
                                {
                                    activity.StartShift = new Shift() { shiftID = dr.GetInt32(5) };
                                }

                                if (!dr.IsDBNull(6))
                                {
                                    activity.EndShift = new Shift() { shiftID = dr.GetInt32(6) };
                                }

                                if (!dr.IsDBNull(7))
                                {
                                    activity.meeting = new Meeting() { meetingID = dr.GetInt32(7) };
                                }

                                activity.IsSpecialTask = dr.GetBoolean(8);

                                if (!dr.IsDBNull(9))
                                {
                                    activity.ApprovedHours = dr.GetDouble(9);
                                    activity.ClaimableAmount = (float)dr.GetDouble(10);
                                    activity.ApplicableRate = new Rate() { rateID = dr.GetInt32(11) };
                                    activity.ClaimDate = dr.GetDateTime(12);
                                }

                                return activity;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets the latest activity by a user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<Activity> GetLatestActivity(string userID)
        {
            string Query = "SELECT TOP 1 * FROM Activity WHERE UserID=@UserID ORDER BY Activity DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", userID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                Activity activity = new Activity();
                                activity.ActivityID = dr.GetInt32(0);
                                activity.userID = dr.GetString(1);
                                activity.locationID = dr.GetInt32(2);
                                activity.inTime = dr.GetDateTime(3);

                                if (!dr.IsDBNull(4))
                                {
                                    activity.outTime = dr.GetDateTime(4);
                                }

                                if (!dr.IsDBNull(5))
                                {
                                    activity.StartShift = new Shift() { shiftID = dr.GetInt32(5) };
                                }

                                if (!dr.IsDBNull(6))
                                {
                                    activity.EndShift = new Shift() { shiftID = dr.GetInt32(6) };
                                }

                                if (!dr.IsDBNull(7))
                                {
                                    activity.meeting = new Meeting() { meetingID = dr.GetInt32(7) };
                                }

                                activity.IsSpecialTask = dr.GetBoolean(8);

                                if (!dr.IsDBNull(9))
                                {
                                    activity.ApprovedHours = dr.GetDouble(9);
                                    activity.ClaimableAmount = (float)dr.GetDouble(10);
                                    activity.ApplicableRate = new Rate() { rateID = dr.GetInt32(11) };
                                    activity.ClaimDate = dr.GetDateTime(12);
                                }

                                return activity;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets the latest activity by a user in a location
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public async Task<Activity> GetLatestActivity(string userID, int locationID)
        {
            string Query = "SELECT TOP 1 * FROM Activity WHERE UserID=@UserID AND LocationID=@LocationID ORDER BY ActivityID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", userID));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                Activity activity = new Activity();
                                activity.ActivityID = dr.GetInt32(0);
                                activity.userID = dr.GetString(1);
                                activity.locationID = dr.GetInt32(2);
                                activity.inTime = dr.GetDateTime(3);

                                if (!dr.IsDBNull(4))
                                {
                                    activity.outTime = dr.GetDateTime(4);
                                }

                                if (!dr.IsDBNull(5))
                                {
                                    activity.StartShift = new Shift() { shiftID = dr.GetInt32(5) };
                                }

                                if (!dr.IsDBNull(6))
                                {
                                    activity.EndShift = new Shift() { shiftID = dr.GetInt32(6) };
                                }

                                if (!dr.IsDBNull(7))
                                {
                                    activity.meeting = new Meeting() { meetingID = dr.GetInt32(7) };
                                }

                                activity.IsSpecialTask = dr.GetBoolean(8);

                                if (!dr.IsDBNull(9))
                                {
                                    activity.ApprovedHours = dr.GetDouble(9);
                                    activity.ClaimableAmount = (float)dr.GetDouble(10);
                                    activity.ApplicableRate = new Rate() { rateID = dr.GetInt32(11) };
                                    activity.ClaimDate = dr.GetDateTime(12);
                                }

                                return activity;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets the latest work or meeting activity of a user in a location
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="locationID"></param>
        /// <param name="GetWorkItem"></param>
        /// <returns></returns>
        public async Task<Activity> GetLatestActivity(string userID, int locationID, bool GetWorkItem)
        {
            string Query = "SELECT * FROM Activity WHERE UserID=@UserID AND LocationID=@LocationID";

            if (GetWorkItem)
            {
                Query += " AND StartShift IS NOT NULL ORDER BY ActivityID DESC";
            }
            else
            {
                Query += " AND MeetingID IS NOT NULL ORDER BY ActivityID DESC";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", userID));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                Activity activity = new Activity();
                                activity.ActivityID = dr.GetInt32(0);
                                activity.userID = dr.GetString(1);
                                activity.locationID = dr.GetInt32(2);
                                activity.inTime = dr.GetDateTime(3);

                                if (!dr.IsDBNull(4))
                                {
                                    activity.outTime = dr.GetDateTime(4);
                                }

                                if (!dr.IsDBNull(5))
                                {
                                    activity.StartShift = new Shift() { shiftID = dr.GetInt32(5) };
                                }

                                if (!dr.IsDBNull(6))
                                {
                                    activity.EndShift = new Shift() { shiftID = dr.GetInt32(6) };
                                }

                                if (!dr.IsDBNull(7))
                                {
                                    activity.meeting = new Meeting() { meetingID = dr.GetInt32(7) };
                                }

                                activity.IsSpecialTask = dr.GetBoolean(8);

                                if (!dr.IsDBNull(9))
                                {
                                    activity.ApprovedHours = dr.GetDouble(9);
                                    activity.ClaimableAmount = (float)dr.GetDouble(10);
                                    activity.ApplicableRate = new Rate() { rateID = dr.GetInt32(11) };
                                    activity.ClaimDate = dr.GetDateTime(12);
                                }

                                return activity;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gets all activity
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<Activity>> GetAllActivityAsync()
        {
            lastError = null;
            string Query = "SELECT * FROM Activity";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Activity> activities = new ObservableCollection<Activity>();

                            while (dr.Read())
                            {
                                Activity activity = new Activity();
                                activity.ActivityID = dr.GetInt32(0);
                                activity.userID = dr.GetString(1);
                                activity.locationID = dr.GetInt32(2);
                                activity.inTime = dr.GetDateTime(3);

                                if (!dr.IsDBNull(4))
                                {
                                    activity.outTime = dr.GetDateTime(4);
                                }

                                if (!dr.IsDBNull(5))
                                {
                                    activity.StartShift = new Shift() { shiftID = dr.GetInt32(5) };
                                }

                                if (!dr.IsDBNull(6))
                                {
                                    activity.EndShift = new Shift() { shiftID = dr.GetInt32(6) };
                                }

                                if (!dr.IsDBNull(7))
                                {
                                    activity.meeting = new Meeting() { meetingID = dr.GetInt32(7) };
                                }

                                activity.IsSpecialTask = dr.GetBoolean(8);

                                if (!dr.IsDBNull(9))
                                {
                                    activity.ApprovedHours = dr.GetDouble(9);
                                    activity.ClaimableAmount = (float)dr.GetDouble(10);
                                    activity.ApplicableRate = new Rate() { rateID = dr.GetInt32(11) };
                                    activity.ClaimDate = dr.GetDateTime(12);
                                }

                                activities.Add(activity);
                            }

                            return activities;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets all activity in a location
        /// </summary>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Activity>> GetAllActivityAsync(int locationID)
        {
            lastError = null;
            string Query = "SELECT * FROM Activity WHERE LocationID=@LocationID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@LocationID", locationID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Activity> activities = new ObservableCollection<Activity>();

                            while (dr.Read())
                            {
                                Activity activity = new Activity();
                                activity.ActivityID = dr.GetInt32(0);
                                activity.userID = dr.GetString(1);
                                activity.locationID = dr.GetInt32(2);
                                activity.inTime = dr.GetDateTime(3);

                                if (!dr.IsDBNull(4))
                                {
                                    activity.outTime = dr.GetDateTime(4);
                                }

                                if (!dr.IsDBNull(5))
                                {
                                    activity.StartShift = new Shift() { shiftID = dr.GetInt32(5) };
                                }

                                if (!dr.IsDBNull(6))
                                {
                                    activity.EndShift = new Shift() { shiftID = dr.GetInt32(6) };
                                }

                                if (!dr.IsDBNull(7))
                                {
                                    activity.meeting = new Meeting() { meetingID = dr.GetInt32(7) };
                                }

                                activity.IsSpecialTask = dr.GetBoolean(8);

                                if (!dr.IsDBNull(9))
                                {
                                    activity.ApprovedHours = dr.GetDouble(9);
                                    activity.ClaimableAmount = (float)dr.GetDouble(10);
                                    activity.ApplicableRate = new Rate() { rateID = dr.GetInt32(11) };
                                    activity.ClaimDate = dr.GetDateTime(12);
                                }

                                activities.Add(activity);
                            }

                            return activities;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets all activity by a user
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<ObservableCollection<Activity>> GetAllActivityAsync(string userID)
        {
            lastError = null;
            string Query = "SELECT * FROM Activity WHERE UserID=@UserID";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", userID));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            ObservableCollection<Activity> activities = new ObservableCollection<Activity>();

                            while (dr.Read())
                            {
                                Activity activity = new Activity();
                                activity.ActivityID = dr.GetInt32(0);
                                activity.userID = dr.GetString(1);
                                activity.locationID = dr.GetInt32(2);
                                activity.inTime = dr.GetDateTime(3);

                                if (!dr.IsDBNull(4))
                                {
                                    activity.outTime = dr.GetDateTime(4);
                                }

                                if (!dr.IsDBNull(5))
                                {
                                    activity.StartShift = new Shift() { shiftID = dr.GetInt32(5) };
                                }

                                if (!dr.IsDBNull(6))
                                {
                                    activity.EndShift = new Shift() { shiftID = dr.GetInt32(6) };
                                }

                                if (!dr.IsDBNull(7))
                                {
                                    activity.meeting = new Meeting() { meetingID = dr.GetInt32(7) };
                                }

                                activity.IsSpecialTask = dr.GetBoolean(8);

                                if (!dr.IsDBNull(9))
                                {
                                    activity.ApprovedHours = dr.GetDouble(9);
                                    activity.ClaimableAmount = (float)dr.GetDouble(10);
                                    activity.ApplicableRate = new Rate() { rateID = dr.GetInt32(11) };
                                    activity.ClaimDate = dr.GetDateTime(12);
                                }

                                activities.Add(activity);
                            }

                            return activities;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Adds an incomplete activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task<bool> AddNewIncompleteActivityAsync(Activity activity)
        {
            lastError = null;

            string Query;

            if (activity.meeting != null)
            {
                Query = "INSERT INTO Activity(UserID, LocationID, inTime, meetingID) VALUES(@UserID, @LocationID, @InTime, @MeetingID)";
            }
            else
            {
                Query = "INSERT INTO Activity(UserID, LocationID, inTime, startShift, endShift, SpecialTask) VALUES(@UserID, @LocationID, @InTime, @StartShift, @EndShift, @SpecialTask)";
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
                            cmd.Parameters.Add(new SqlParameter("@SpecialTask", activity.IsSpecialTask));
                        }

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Adds a complete activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task<bool> AddNewCompleteActivityAsync(Activity activity)
        {
            lastError = null;

            string Query;

            if (activity.meeting != null)
            {
                Query = "INSERT INTO Activity(UserID, LocationID, InTime, OutTime, MeetingID, ApprovedHours, ClaimableAmount, ApplicableRate, ClaimDate) VALUES(@UserID, @LocationID, @InTime, @OutTime, @StartShift, @EndShift, @MeetingID, @ApprovedHours, @ClaimableAmount, @ApplicableRate, @ClaimDate)";
            }
            else
            {
                Query = "INSERT INTO Activity(UserID, LocationID, InTime, OutTime, StartShift, EndShift, SpecialTask, ApprovedHours, ClaimableAmount, ApplicableHours, ClaimDate) VALUES(@UserID, @LocationID, @InTime, @OutTime, @StartShift, @EndShift, @SpecialTask, @ApprovedHours, @ClaimableAmount, @ApplicableHours)";
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
                        cmd.Parameters.Add(new SqlParameter("@OutTime", activity.outTime));
                        cmd.Parameters.Add(new SqlParameter("@ApprovedHours", activity.ApprovedHours));
                        cmd.Parameters.Add(new SqlParameter("@ClaimableAmount", activity.ClaimableAmount));
                        cmd.Parameters.Add(new SqlParameter("@ApplicableRate", activity.ApplicableRate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@ClaimDate", activity.ClaimDate));

                        if (activity.meeting != null)
                        {
                            cmd.Parameters.Add(new SqlParameter("@MeetingID", activity.meeting.meetingID));
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@StartShift", activity.StartShift.shiftID));
                            cmd.Parameters.Add(new SqlParameter("@EndShift", activity.EndShift.shiftID));
                            cmd.Parameters.Add(new SqlParameter("@SpecialTask", activity.IsSpecialTask));
                        }

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes the specified activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task<bool> DeleteActivityAsync(Activity activity)
        {
            string Query = "DELETE FROM Activity WHERE ActivityID=@ActivityID";
            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@ActivityID", activity.ActivityID));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates the specified activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateActivityAsync(Activity activity)
        {
            lastError = null;
            string Query;

            if (activity.meeting != null)
            {
                Query = "UPDATE Activity SET UserID=@UserID, LocationID=@LocationID, InTime=@InTime, OutTime=@OutTime, MeetingID=@MeetingID, ApprovedHours=@ApprovedHours, ClaimableAmount=@ClaimableAmount, ApplicableRate=@ApplicableRate, ClaimDate=@ClaimDate WHERE ActivityID=@ActivityID";
            }
            else
            {
                Query = "UPDATE Activity SET UserID=@UserID, LocationID=@LocationID, InTime=@InTime, OutTime=@OutTime, StartShift=@StartShift, EndShift=@EndShift, SpecialTask=@SpecialTask, ApprovedHours=@ApprovedHours, ClaimableAmount=@ClaimableAmount, ApplicableRate=@ApplicableRate, ClaimDate=@ClaimDate WHERE ActivityID=@ActivityID";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@ActivityID", activity.ActivityID));
                        cmd.Parameters.Add(new SqlParameter("@UserID", activity.userID));
                        cmd.Parameters.Add(new SqlParameter("@LocationID", activity.locationID));
                        cmd.Parameters.Add(new SqlParameter("@InTime", activity.inTime));
                        cmd.Parameters.Add(new SqlParameter("@OutTime", activity.outTime));
                        cmd.Parameters.Add(new SqlParameter("@ApprovedHours", activity.ApprovedHours));
                        cmd.Parameters.Add(new SqlParameter("@ClaimableAmount", activity.ClaimableAmount));
                        cmd.Parameters.Add(new SqlParameter("@ApplicableRate", activity.ApplicableRate.rateID));
                        cmd.Parameters.Add(new SqlParameter("@ClaimDate", activity.ClaimDate));

                        if (activity.meeting != null)
                        {
                            cmd.Parameters.Add(new SqlParameter("@MeetingID", activity.meeting.meetingID));
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@StartShift", activity.StartShift.shiftID));
                            cmd.Parameters.Add(new SqlParameter("@EndShift", activity.EndShift.shiftID));
                            cmd.Parameters.Add(new SqlParameter("@SpecialTask", activity.IsSpecialTask));
                        }

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Global Settings

        /// <summary>
        /// Gets the value of a global settings
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public async Task<string> GetGlobalSettingsByKeyAsync(string Key)
        {
            string Query = "SELECT SettingValue FROM Global_Settings WHERE SettingKey=@Key";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@Key", Key));

                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            while (dr.Read())
                            {
                                return dr.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Updates the value of a global settings
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public async Task<bool> UpdateGlobalSettingsAsync(string Key, string Value)
        {
            lastError = null;

            string Query = "UPDATE Global_Settings SET SettingValue=@Value WHERE SettingKey=@Key";

            try
            {
                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@Key", Key));
                        cmd.Parameters.Add(new SqlParameter("@Value", Value));

                        await cmd.ExecuteNonQueryAsync();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Gets the owner of a card
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public async Task<string> GetUsernameFromCardId(string cardId)
        {
            try
            {
                lastError = null;

                string Query = "SELECT Name FROM CardDetail WHERE Badgenumber=@BadgeNumber";

                using (SqlConnection conn = new SqlConnection(CardConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@BadgeNumber", cardId));
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            string username = string.Empty;
                            while (dr.Read())
                            {
                                username = dr.GetString(0);
                            }

                            return username;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                lastError = ex;
            }

            return null;
        }

        public async Task<double> GetApprovedHours(string upn)
        {
            try
            {
                lastError = null;

                string Query = "SELECT SUM(ApprovedHours) as 'ApprovedHours' FROM Activity WHERE UserID=@UserID AND ClaimDate >= DATEFROMPARTS(year(GETDATE()),month(GETDATE()),1)";

                using (SqlConnection conn = new SqlConnection(DbConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = Query;
                        cmd.Parameters.Add(new SqlParameter("@UserID", upn));
                        using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                        {
                            double ApprovedHours = 0;
                            while (dr.Read())
                            {
                                ApprovedHours = dr.GetDouble(0);
                            }

                            return ApprovedHours;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[DataAccess] Exception: " + ex.Message);
                lastError = ex;
            }

            return 0;
        }

    }
}
