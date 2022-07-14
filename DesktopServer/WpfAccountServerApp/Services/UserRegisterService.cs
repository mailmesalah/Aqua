using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using AquaServer.StorageModel;
using System.ServiceModel;
using AquaServer.Local;

namespace AquaServer.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    public class UserRegisterService : IUserRegister
    {
        public CBoolMessage CreateUser(CUser oUser, long companyId, string sessionId)
        {
            CBoolMessage cbm = new CBoolMessage();
            cbm.IsSuccess = false;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null || si.UserType == CUser.USER)
            {
                cbm.Message = "Session Error";
                return cbm;
            }

            try
            {
                if (IsUsernameAlreadyUsed(oUser.Username, companyId))
                {
                    cbm.Message = "Username already Exist";
                    return cbm;
                }

                using (var dataB = new AquaStorage())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        User u = dataB.Users.Create();
                        u.CompanyId = companyId;
                        u.Name = oUser.Name;
                        u.Username = oUser.Username;
                        u.Password = oUser.Password;
                        u.UserType = CUser.USER;
                        dataB.Users.Add(u);

                        dataB.SaveChanges();
                        //Success
                        cbm.IsSuccess = true;

                        dataBTransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        cbm.IsSuccess = false;
                        cbm.Message = e.Message;

                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }

            }
            catch (Exception e) { cbm.Message = e.Message; }
            return cbm;
        }

        public CBoolMessage UpdateUser(CUser oUser, long companyId, string sessionId)
        {
            CBoolMessage cbm = new CBoolMessage();
            cbm.IsSuccess = false;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null || si.UserType == CUser.USER)
            {
                cbm.Message = "Session Error";
                return cbm;
            }

            try
            {

                if (IsUsernameAlreadyUsed(oUser.Id, oUser.Username, companyId))
                {
                    cbm.Message = "Username already Exist";
                    return cbm;
                }

                using (var dataB = new AquaStorage())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        User u = dataB.Users.Select(c => c).Where(x => x.Id == oUser.Id && x.CompanyId == companyId).FirstOrDefault();
                        u.Name = oUser.Name;
                        u.Username = oUser.Username;
                        u.Password = oUser.Password;

                        dataB.SaveChanges();

                        //Success
                        cbm.IsSuccess = true;

                        dataBTransaction.Commit();
                    }
                    catch (Exception e)
                    {
                        cbm.IsSuccess = false;
                        cbm.Message = e.Message;

                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }

                }

            }
            catch (Exception e) { cbm.Message = e.Message; }
            return cbm;
        }

        public CBoolMessage DeleteUser(long userId, long companyId, string sessionId)
        {
            CBoolMessage cbm = new CBoolMessage();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null || si.UserType == CUser.USER)
            {
                cbm.Message = "Session Error";
                return cbm;
            }

            try
            {
                if (IsUserRemovable(userId) != true)
                {
                    cbm.IsSuccess = false;
                    cbm.Message = "Permission Denied To Remove this User";
                    return cbm;
                }
               
                if (IsUserUsedInTransaction(userId) == true)
                {
                    cbm.IsSuccess = false;
                    cbm.Message = "Please Remove User from Transactions, First";
                    return cbm;
                }

                cbm.IsSuccess = false;

                using (var dataB = new AquaStorage())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        var cr = dataB.Users.Select(c => c).Where(x => x.Id == userId && x.CompanyId == companyId);
                        var r = dataB.Users.RemoveRange(cr);
                        if (r.Count() > 0)
                        {

                            dataB.SaveChanges();
                            dataBTransaction.Commit();
                            cbm.IsSuccess = true;
                        }
                        else
                        {
                            cbm.IsSuccess = false;
                            cbm.Message = "User Doesnt Exist";
                        }

                    }
                    catch (Exception e)
                    {
                        cbm.IsSuccess = false;
                        cbm.Message = e.Message;
                        dataBTransaction.Rollback();
                    }
                }

            }
            catch (Exception e) { cbm.Message = e.Message; }
            return cbm;
        }

        public CUser ReadUser(long userId, long companyId, string sessionId)
        {
            CUser u = null;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                return u;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var cps = dataB.Users.Select(c => c).Where(x => x.Id == userId && x.CompanyId == companyId);

                    if (cps.Count() > 0)
                    {
                        u = new CUser();

                        var cp = cps.FirstOrDefault();
                        u.Id = cp.Id;
                        u.Name = cp.Name;
                        u.Username = cp.Username;
                        //u.Password = cp.Password;
                        u.UserType = cp.UserType;
                        u.CompanyId = u.CompanyId;
                    }

                }
            }
            catch { }
            return u;
        }

        public CUser ReadUser(long userCode, long companyId)
        {
            CUser u = null;

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var cps = dataB.Users.Select(c => c).Where(x => x.Id == userCode && x.CompanyId == companyId);

                    if (cps.Count() > 0)
                    {
                        u = new CUser();

                        var cp = cps.FirstOrDefault();
                        u.Id = cp.Id;
                        u.Name = cp.Name;
                        u.Username = cp.Username;
                        //u.Password = cp.Password;
                        u.UserType = cp.UserType;
                        u.CompanyId = u.CompanyId;
                    }

                }
            }
            catch { }
            return u;
        }

        public CUser ReadSuperAdmin(long companyCode)
        {
            CUser u = null;
            try
            {
                using (var dataB = new AquaStorage())
                {
                    var cps = dataB.Users.Select(c => c).Where(x => x.UserType == CUser.SUPER_ADMIN && x.CompanyId == companyCode);

                    if (cps.Count() > 0)
                    {
                        u = new CUser();

                        var cp = cps.FirstOrDefault();
                        u.Id = cp.Id;
                        u.Name = cp.Name;
                        u.Username = cp.Username;
                        u.Password = cp.Password;
                        u.UserType = cp.UserType;
                        u.CompanyId = u.CompanyId;
                    }

                }
            }
            catch { }
            return u;
        }

        public CUser ReadUser(long userId)
        {
            CUser u = null;
            try
            {
                using (var dataB = new AquaStorage())
                {
                    var cps = dataB.Users.Select(c => c).Where(x => x.Id == userId);

                    if (cps.Count() > 0)
                    {
                        u = new CUser();

                        var cp = cps.FirstOrDefault();
                        u.Id = cp.Id;
                        u.Name = cp.Name;
                        u.Username = cp.Username;
                        //u.Password = cp.Password;
                        u.UserType = cp.UserType;
                        u.CompanyId = u.CompanyId;
                    }

                }
            }
            catch { }
            return u;
        }

        private bool IsUsernameAlreadyUsed(string username, long companyId)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Users.Select(e => e).Where(e => e.Username == username && e.CompanyId == companyId);
                if (data.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsUsernameAlreadyUsed(long userId, string username, long companyId)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Users.Select(e => e).Where(e => e.Username == username && e.CompanyId == companyId && e.Id != userId);
                if (data.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsUserRemovable(long userId)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Users.Select(e => e).Where(e => e.Id == userId).FirstOrDefault();
                if (data.UserType == CUser.SUPER_ADMIN)
                {
                    return false;
                }
            }
            return true;
        }


        private bool IsUserUsedInTransaction(long userId)
        {
            bool isUsed = false;
            using (var dataB = new AquaStorage())
            {
                var dTS = dataB.Transactions.Select(e => e).Where(e => e.AddedUserId == userId);
                if (dTS.Count() > 0)
                {
                    isUsed = true;
                }

            }
            return isUsed;
        }


        public List<CUser> ReadAllUsers(long companyId, string sessionId)
        {
            List<CUser> users = new List<CUser>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null || si.UserType != CUser.SUPER_ADMIN)
            {
                return users;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var datas = dataB.Users.Select(e => e).Where(e => e.CompanyId == companyId).OrderBy(e => e.UserType).ThenBy(e => e.Name);
                    foreach (var i in datas)
                    {
                        users.Add(new CUser() { Id = i.Id, UserType = i.UserType, Name = i.Name, Username = i.Username, CompanyId = i.CompanyId, Password = i.Password });
                    }
                }
            }
            catch { }
            return users;
        }
    }
}
