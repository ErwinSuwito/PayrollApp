﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using PayrollCore.Entities;

namespace PayrollCore
{
    class DataAccess2
    {
        string DbConnString;
        string CardConnString;
        public Exception lastError;

        /* Tables to do:
         * Rate (GET ALL, GET ID, INSERT, UPDATE, DELETE)           
         * user_group (GET ID, GET ALL, INSERT, UPDATE, DELETE)
         * Users (GET ID, GET ALL, INSERT, UPDATE, DELETE)
         * Location (GET ID, GET ALL, INSERT, UPDATE, DELETE)
         * Meeting (GET ID, LOCATION ALL, GET LOCATION ENABLED, 
         *          GET LOCATION DAY USERGROUP, INSERT, UPDATE, DELETE)
         * Meeting_Group (GET ALL MEETING ID, INSERT, UPDATE, DELETE)
         * Activity (GET ID, GET ALL, INSERT, INSERT ALL, UPDATE, DELETE)
         * Global_Settings (GET ID, UPDATE)
         */

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
        public bool TestConnString(string connString)
        {
            lastError = null;
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
                lastError = ex;
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return false;
            }
        }

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
            string Query = "UPDATE Rate SET RateDesc=@RateDesc AND Rate=@Rate AND IsDisabled=@IsDisabled WHERE RateID=@RateID";
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
            string Query = "INSERT INTO user_group(GroupName, RateID, ShowAdminSettings, EnableFaceRect) VALUES(@GroupID, @GroupName, @RateID, @ShowAdminSettings, @EnableFaceRec)";
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
            string Query = "UPDATE user_group SET GroupName=@GroupName AND RateID=@RateID AND ShowAdminSettings=@ShowAdminSettings AND EnableFaceRec=@EnableFaceRec AND IsDisabled=@IsDisabled WHERE GroupID=@GroupID";
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
                        cmd.Parameters.Add(new SqlParameter("@userID", userID));

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
            string Query = "INSERT INTO Users(UserID, FullName, FromAD, IsDisabled, GroupID) VALUES(@UserID, @FullName, @FromAD, @IsDisabled, @GroupID)";
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
            string Query = "UPDATE Users SET FullName=@FullName AND FromAD=@FromAD AND IsDisabled=@IsDisabled AND GroupID=@GroupID WHERE UserID=@UserID";
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
        public async Task<bool> AddNewLocationAsync(Location location)
        {
            string Query = "INSERT INTO Location(LocationName, EnableGM, IsDisabled) VALUES(@LocationName, @EnableGM, @IsDisabled)";
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
            string Query = "UPDATE Location SET LocationName=@LocationName AND EnableGM=@EnableGM AND IsDisabled=@IsDisabled WHERE LocationID=@LocationID";
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


    }
}
