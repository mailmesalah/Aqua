using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AquaServer.General;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AquaServer.StorageModel;
using AquaServer.Services;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace AquaServer.Local
{
    public class CSession
    {

        public const int DEVICE_MOBILE=222;
        public const int DEVICE_COMPUTER = 333;

        public string GetNewSession(long userId)
        {
            string seed = userId +""+ DateTime.Now;
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(seed));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool SetSession(long userId, int userType, string session, int deviceType)
        {
            using (var dataB = new AquaStorage())
            {
                Session sesinfo=dataB.Sessions.Where<Session>(x => x.UserId == userId && x.DeviceType == deviceType).FirstOrDefault();
                if (sesinfo == null)
                {
                    using (var dbTransaction = dataB.Database.BeginTransaction())
                    {
                        try
                        {

                            //Use ExecuteSQLCommand
                            string sInsert = "Insert Into Sessions (UserId, UserType, SessionId, DeviceType) ";
                            string sValues = "Values (@userId, @userType, @sesId, @deviceType) ";
                            string sQuery = sInsert + sValues;

                            dataB.Database.ExecuteSqlCommand(sQuery,
                                new MySqlParameter("@userId", userId),
                                new MySqlParameter("@userType", userType),
                                new MySqlParameter("@deviceType", deviceType),
                                new MySqlParameter("@sesId", session));

                            dbTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            dbTransaction.Rollback();
                            return false;
                        }
                    }
                }
                else
                {
                    using (var dbTransaction = dataB.Database.BeginTransaction())
                    {
                        try
                        {

                            //Use ExecuteSQLCommand
                            string sUpdate = "Update Sessions ";
                            string sValues = "Set SessionId=@sesId Where (UserId=@userId and DeviceType=@deviceType) ";
                            string sQuery = sUpdate + sValues;

                            dataB.Database.ExecuteSqlCommand(sQuery,
                                new MySqlParameter("@userId", userId),
                                new MySqlParameter("@deviceType", deviceType),
                                new MySqlParameter("@sesId", session));

                            dbTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            dbTransaction.Rollback();

                            return false;
                        }
                    }

                }
            }
            return true;
        }

        public Session GetSession(string session)
        {
            Session sesinfo = null;
            using (var dataB = new AquaStorage())
            {
                sesinfo = dataB.Sessions.Where<Session>(x => x.SessionId == session).FirstOrDefault();                
            }
            return sesinfo;
        }

        public Session GetSession(long userId)
        {
            Session sesinfo = null;
            using (var dataB = new AquaStorage())
            {
                sesinfo = dataB.Sessions.Where<Session>(x => x.UserId == userId).FirstOrDefault();
            }
            return sesinfo;
        }
    }
}
