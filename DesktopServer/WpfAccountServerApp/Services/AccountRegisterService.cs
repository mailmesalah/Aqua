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
using System.ServiceModel;
using MySql.Data.MySqlClient;
using AquaServer.Local;

namespace AquaServer.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    public class AccountRegisterService : IAccountRegister
    {
        public CBoolMessage CreateAccount(CAccount oAccount, long companyId, string sessionId)
        {
            CBoolMessage cbm = new CBoolMessage();
            cbm.IsSuccess = false;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                cbm.Message = "Session Error";
                return cbm;
            }

            try
            {
                if (IsAccountNameAlreadyUsed(oAccount.Name, companyId))
                {
                    cbm.Message = "Account Name already Exist";
                    return cbm;
                }

                using (var dataB = new AquaStorage())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        Account u = dataB.Accounts.Create();
                        u.CompanyId = companyId;
                        u.Name = oAccount.Name;
                        u.ShortName = oAccount.ShortName;
                        u.Details1 = oAccount.Details1;
                        u.Details2 = oAccount.Details2;
                        u.Details3 = oAccount.Details3;
                        u.MainGroup = oAccount.MainGroup;
                        u.ParentGroupId = oAccount.ParentGroupId;
                        u.AccountType = oAccount.AccountType;
                        u.Visibility = 1;
                       
                        dataB.Accounts.Add(u);

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

        public CBoolMessage UpdateAccount(CAccount oAccount, long companyId, string sessionId)
        {
            CBoolMessage cbm = new CBoolMessage();
            cbm.IsSuccess = false;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                cbm.Message = "Session Error";
                return cbm;
            }

            try
            {

                if (IsAccountNameAlreadyUsed(oAccount.Id, oAccount.Name, companyId))
                {
                    cbm.Message = "Account Name already Exist";
                    return cbm;
                }


                using (var dataB = new AquaStorage())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        Account u = dataB.Accounts.Select(c => c).Where(x => x.Id == oAccount.Id && x.CompanyId == companyId && x.IsConcrete==false).FirstOrDefault();
                        if (u!=null)
                        {
                            u.Name = oAccount.Name;
                            u.ShortName = oAccount.ShortName;
                            u.Details1 = oAccount.Details1;
                            u.Details2 = oAccount.Details2;
                            u.Details3 = oAccount.Details3;
                            u.Visibility = 1;

                            dataB.SaveChanges();
                            dataBTransaction.Commit();

                            //Success
                            cbm.IsSuccess = true;
                        }
                        else
                        {
                            cbm.IsSuccess = false;
                            cbm.Message = "Not Allowed to Edit";
                        }
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

        public CBoolMessage DeleteAccount(long accountId, long companyId, string sessionId)
        {
            CBoolMessage cbm = new CBoolMessage();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                cbm.Message = "Session Error";
                return cbm;
            }

            try
            {
                if (IsAccountRemovable(accountId) != true)
                {
                    cbm.IsSuccess = false;
                    cbm.Message = "Permission Denied To Remove this Account";
                    return cbm;
                }

                if (IsAccountHasChild(accountId) == true)
                {
                    cbm.IsSuccess = false;
                    cbm.Message = "Please Remove the Accounts Under this Account, First";
                    return cbm;
                }

                if (IsAccountUsedInTransaction(accountId) == true)
                {
                    cbm.IsSuccess = false;
                    cbm.Message = "Please Remove Account from Transactions, First";
                    return cbm;
                }

                cbm.IsSuccess = false;

                using (var dataB = new AquaStorage())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        var cr = dataB.Accounts.Select(c => c).Where(x => x.Id == accountId && x.CompanyId == companyId);
                        var r = dataB.Accounts.RemoveRange(cr);
                        if (r.Count() > 0)
                        {
             
                            dataB.SaveChanges();
                            dataBTransaction.Commit();
                            cbm.IsSuccess = true;
                        }
                        else
                        {
                            cbm.IsSuccess = false;
                            cbm.Message = "Account Doesnt Exist";
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

        public CAccount ReadAccount(long accountId, long companyId, string sessionId)
        {
            CAccount u = null;

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
                    var cps = dataB.Accounts.Select(c => c).Where(x => x.Id == accountId && x.CompanyId == companyId);

                    if (cps.Count() > 0)
                    {
                        u = new CAccount();

                        var cp = cps.FirstOrDefault();
                        u.Id = cp.Id;
                        u.Name = cp.Name;
                        u.ShortName = cp.ShortName;                       
                        u.AccountType = cp.AccountType;
                        u.CompanyId = companyId;
                        u.Details1 = cp.Details1;
                        u.Details2 = cp.Details2;
                        u.Details3 = cp.Details3;
                        u.MainGroup = cp.MainGroup;
                        u.ParentGroupId = cp.ParentGroupId;
                    }

                }
            }
            catch { }
            return u;
        }

        public CAccount ReadAccount(long accountId, long companyId)
        {
            CAccount u = null;

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var cps = dataB.Accounts.Select(c => c).Where(x => x.Id == accountId && x.CompanyId == companyId);

                    if (cps.Count() > 0)
                    {
                        u = new CAccount();

                        var cp = cps.FirstOrDefault();
                        u.Id = cp.Id;
                        u.Name = cp.Name;
                        u.ShortName = cp.ShortName;
                        u.AccountType = cp.AccountType;
                        u.CompanyId = companyId;
                        u.Details1 = cp.Details1;
                        u.Details2 = cp.Details2;
                        u.Details3 = cp.Details3;
                        u.MainGroup = cp.MainGroup;
                        u.ParentGroupId = cp.ParentGroupId;
                    }

                }
            }
            catch { }
            return u;
        }

        public CAccount ReadAccount(long accountId)
        {
            CAccount u = null;
            try
            {
                using (var dataB = new AquaStorage())
                {
                    var cps = dataB.Accounts.Select(c => c).Where(x => x.Id == accountId);

                    if (cps.Count() > 0)
                    {
                        u = new CAccount();

                        var cp = cps.FirstOrDefault();
                        u.Id = cp.Id;
                        u.Name = cp.Name;
                        u.ShortName = cp.ShortName;
                        u.AccountType = cp.AccountType;
                        u.CompanyId = cp.CompanyId;
                        u.Details1 = cp.Details1;
                        u.Details2 = cp.Details2;
                        u.Details3 = cp.Details3;
                        u.MainGroup = cp.MainGroup;
                        u.ParentGroupId = cp.ParentGroupId;
                    }

                }
            }
            catch { }
            return u;
        }

        private bool IsAccountNameAlreadyUsed(string accountname, long companyId)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Accounts.Select(e => e).Where(e => e.Name == accountname && e.CompanyId == companyId);
                if (data.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsAccountNameAlreadyUsed(long accountId, string accountname, long companyId)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Accounts.Select(e => e).Where(e => e.Name == accountname && e.CompanyId == companyId && e.Id != accountId);
                if (data.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsAccountRemovable(long AccountId)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Accounts.Select(e => e).Where(e => e.Id == AccountId).FirstOrDefault();
                return !data.IsConcrete;                
            }            
        }

        private bool IsAccountHasChild(long AccountId)
        {
            bool hasChild = false;
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Accounts.Select(e => e).Where(e => e.ParentGroupId == AccountId);
                if (data.Count() > 0)
                {
                    hasChild = true;
                }
            }
            return hasChild;
        }

        private bool IsAccountUsedInTransaction(long AccountId)
        {
            bool isUsed = false;
            using (var dataB = new AquaStorage())
            {
                var dTS = dataB.Transactions.Select(e => e).Where(e => e.AccountId == AccountId);
                if (dTS.Count() > 0)
                {
                    isUsed = true;
                }
            }
            return isUsed;
        }


        
        public List<CAccount> ReadAllAccounts(long companyId, string sessionId)
        {
            List<CAccount> accounts = new List<CAccount>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                return accounts;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var datas = dataB.Accounts.Select(e => e).Where(e => e.CompanyId == companyId).OrderBy(e => e.AccountType).ThenBy(e => e.Name);
                    foreach (var i in datas)
                    {
                        accounts.Add(new CAccount() { Id = i.Id, AccountType = i.AccountType, Name = i.Name, ShortName = i.ShortName, CompanyId = i.CompanyId, Details1 =i.Details1, Details2=i.Details2, Details3=i.Details3, MainGroup=i.MainGroup, ParentGroupId=i.ParentGroupId });
                    }
                }
            }
            catch { }
            return accounts;
        }

        public List<CAccount> ReadAllAccountTypes(long companyId, string sessionId)
        {
            List<CAccount> accounts = new List<CAccount>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                return accounts;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var datas = dataB.Accounts.Select(e => e).Where(e => e.CompanyId == companyId && e.AccountType== CAccount.ACCOUNT).OrderBy(e => e.Name);
                    foreach (var i in datas)
                    {
                        accounts.Add(new CAccount() { Id = i.Id, AccountType = i.AccountType, Name = i.Name, ShortName = i.ShortName, CompanyId = i.CompanyId, Details1 = i.Details1, Details2 = i.Details2, Details3 = i.Details3, MainGroup = i.MainGroup, ParentGroupId = i.ParentGroupId });
                    }
                }
            }
            catch { }
            return accounts;
        }
        public List<CAccount> ReadAllAccountsUnderMainGroup(int MainGroup, long companyId, string sessionId)
        {
            List<CAccount> accounts = new List<CAccount>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                return accounts;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var datas = dataB.Accounts.Select(e => e).Where(e => e.CompanyId == companyId && e.AccountType==CAccount.ACCOUNT && e.MainGroup==MainGroup).OrderBy(e => e.Name);
                    foreach (var i in datas)
                    {
                        accounts.Add(new CAccount() { Id = i.Id, AccountType = i.AccountType, Name = i.Name, ShortName = i.ShortName, CompanyId = i.CompanyId, Details1 = i.Details1, Details2 = i.Details2, Details3 = i.Details3, MainGroup = i.MainGroup, ParentGroupId = i.ParentGroupId });
                    }
                }
            }
            catch { }
            return accounts;
        }

        public List<CAccount> ReadAllParentGroupsUnderMainGroup(int MainGroup, long companyId, string sessionId)
        {
            List<CAccount> accounts = new List<CAccount>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                return accounts;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var datas = dataB.Accounts.Select(e => e).Where(e => e.CompanyId == companyId && e.AccountType == CAccount.GROUP && e.MainGroup == MainGroup).OrderBy(e => e.Name);
                    foreach (var i in datas)
                    {
                        accounts.Add(new CAccount() { Id = i.Id, AccountType = i.AccountType, Name = i.Name, ShortName = i.ShortName, CompanyId = i.CompanyId, Details1 = i.Details1, Details2 = i.Details2, Details3 = i.Details3, MainGroup = i.MainGroup, ParentGroupId = i.ParentGroupId });
                    }
                }
            }
            catch { }
            return accounts;
        }

        public List<CAccount> ReadAllAccountsUnderParentGroup(long ParentGroupId, long companyId, string sessionId)
        {
            List<CAccount> accounts = new List<CAccount>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                return accounts;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var datas = dataB.Accounts.Select(e => e).Where(e => e.CompanyId == companyId && e.AccountType == CAccount.ACCOUNT && e.ParentGroupId == ParentGroupId).OrderBy(e => e.Name);
                    foreach (var i in datas)
                    {
                        accounts.Add(new CAccount() { Id = i.Id, AccountType = i.AccountType, Name = i.Name, ShortName = i.ShortName, CompanyId = i.CompanyId, Details1 = i.Details1, Details2 = i.Details2, Details3 = i.Details3, MainGroup = i.MainGroup, ParentGroupId = i.ParentGroupId });
                    }
                }
            }
            catch { }
            return accounts;
        }
    }
}
