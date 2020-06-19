using System;
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
         * Location (GET ID, GET ALL, GET ENABLED, INSERT, UPDATE, DELETE)
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

        public async Task<ObservableCollection<Rate>> GetAllRatesAsync()
        {
            lastError = null;
            string Query = "SELECT * FROM Rate";
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
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> AddRate(Rate rate)
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
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteRate(Rate rate)
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
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateRate(Rate rate)
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
                Debug.WriteLine("DataAccess Exception: " + ex.Message);
                return false;
            }
        }

    }
}
