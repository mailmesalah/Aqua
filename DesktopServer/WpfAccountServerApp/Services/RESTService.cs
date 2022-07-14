using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using AquaServer.StorageModel;
using ServerServiceInterface.RestClasses;
using MySql.Data.MySqlClient;
using System.ServiceModel;
using AquaServer.Local;
using System.IO;
using System.ServiceModel.Web;
using AquaServer.General;

namespace AquaServer.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    public class RESTService : IRESTService
    {
     /*   public CServerMessage GetLoginDetails(CClientLocalInfo culi)
        {
            CClientLocalInfo rculi = new CClientLocalInfo();
            rculi.ReturnObjectHint = CServerMessage.CClientLocalInfo;

            try
            {
                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        
                        //lock (Synchronizer.@lock)
                        {
                            var company = dataB.Companies.Where<Company>(sup => sup.ExpiryDate > DateTime.Now && sup.IsActive == true && sup.CompanyId.Equals(culi.CompanyUsername)).FirstOrDefault();
                            if (company != null)
                            {
                                var userData = dataB.Accounts.Where<Account>(user => (user.CompanyId == company.Id) && (user.Username == culi.Username) && (user.Password == culi.Password) && (user.Status != CUser.DEACTIVATED)).FirstOrDefault();
                                if (userData != null)
                                {

                                    if (userData.UserType == CUser.DEALER || userData.UserType == CUser.SUB_DEALER)
                                    {
                                        SettingsService ss = new SettingsService();
                                        if (ss.IsSessionLocked(culi.CompanyId, SettingsService.ENABLE_DISABLE_SESSION_LOCK))
                                        {                                            
                                            CSession sess = new CSession();
                                            Session sUser = sess.GetSession(userData.Id);
                                            if (sUser != null)
                                            {
                                                if (!sUser.SessionId.Equals(culi.SessionId))
                                                {
                                                    rculi.IsSuccess = false;
                                                    rculi.Message = "Already Logged In";
                                                    return rculi;
                                                }
                                            }
                                        }
                                    }
                                    
                                    rculi.ClientDevice = culi.ClientDevice;
                                    rculi.CompanyId = company.Id;
                                    rculi.CompanyUsername = company.CompanyId;
                                    rculi.Username = userData.Username;
                                    rculi.Password = userData.Password;
                                    rculi.UserType = userData.UserType;
                                    rculi.UserId = userData.Id;
                                    rculi.ParentUserId = userData.ParentUserId;
                                    rculi.Name = userData.Name;

                                    /*if (rculi.ClientDeviceCode == 0)
                                    {
                                        BillNoGeneratorService bgs = new BillNoGeneratorService();
                                        long code = bgs.GetNextBillNo(userData.Id, company.Id, BillNoGeneratorService.DEVICE_CODE);
                                        bgs.SetBillNo(userData.Id, company.Id, BillNoGeneratorService.DEVICE_CODE, code);
                                        rculi.ClientDeviceCode = code;
                                    }*//*
                                    rculi.ApkVersion = 4.1;
                                    rculi.IsSuccess = true;
                                    rculi.Message = "Successful Login";



                                    //Create Session for this Login     
                                    CSession ses = new CSession();
                                    string session= ses.GetNewSession(userData.Id);
                                    if (ses.SetSession(userData.Id, userData.UserType, session, CSession.DEVICE_MOBILE))
                                    {
                                        rculi.SessionId = session;
                                    }else
                                    {
                                        rculi.IsSuccess = false;
                                        rculi.Message = "Username or Password does not Exist";
                                    }
                                }
                                else
                                {
                                    rculi.IsSuccess = false;
                                    rculi.Message = "Username or Password does not Exist";
                                }
                            }
                            else
                            {
                                rculi.IsSuccess = false;
                                rculi.Message = "Username or Password does not Exist";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        rculi.IsSuccess = false;
                        rculi.Message = "Error " + e.Message;
                    }
                }
            }
            catch (Exception e)
            {
                rculi.IsSuccess = false;
                rculi.Message = "Error " + e.Message;
            }

            return rculi;
        }

        public List<CTicket> GetTickets(CGetTicketsParam param)
        {
            List<CTicket> tickets = new List<CTicket>();
            try
            {                
                CSession sesObj = new CSession();
                Session si = sesObj.GetSession(param.SessionId);
                if (si == null)
                {
                    return tickets;
                }
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {

                            var ticketData = dataB.Tickets.Where<Ticket>(t => (t.CompanyId == param.CompanyId && t.IsActive == true)).OrderBy(t => t.NoOfDigits).ThenBy(t => t.Type);
                            if (ticketData != null)
                            {
                                foreach (var i in ticketData)
                                {
                                    CTicket ct = new CTicket();
                                    ct.CompanyId = i.CompanyId;
                                    ct.IsActive = i.IsActive;
                                    ct.Mask = i.Mask;
                                    ct.Name = i.Name;
                                    ct.NoOfDigits = i.NoOfDigits;
                                    ct.TicketId = i.Id;
                                    ct.Type = i.Type;
                                    ct.Cost = i.Cost;

                                    try
                                    {
                                        UserTicketSettingsService dts = new UserTicketSettingsService();
                                        CUserTicketRateItemParam dtRateItem = dts.ReadTicketRateForUser(param.UserId, i.Id, param.CompanyId);
                                        ct.Commission = i.Cost - dtRateItem.Commission;
                                    }
                                    catch { }
                                    tickets.Add(ct);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return tickets;
        }

        public List<CUser> GetUserNSubUsers(CGetUserNSubUsersParam param)
        {
            List<CUser> users = new List<CUser>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.UserId)
            {
                return users;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {
                            if (si.UserType == CUser.SUPER_ADMIN)
                            {
                                var datas = dataB.Accounts.Select(e => e).Where(e => (e.Status != CUser.DEACTIVATED && (e.UserType != CUser.SUPER_ADMIN && e.UserType!=CUser.OUT_DEALER) && e.CompanyId == param.CompanyId)).OrderBy(e => e.UserType).ThenBy(e => e.Id);

                                if (datas != null)
                                {
                                    foreach (var i in datas)
                                    {
                                        CUser u = new CUser();
                                        u.CompanyId = i.CompanyId;
                                        u.Status = i.Status;
                                        u.Name = i.Name;
                                        u.UserType = i.UserType;
                                        u.Id = i.Id;
                                        u.ParentUserId = i.ParentUserId;
                                        users.Add(u);
                                    }
                                }
                            }
                            else if (si.UserType == CUser.ADMIN)
                            {
                                var datas = dataB.Accounts.Select(e => e).Where(e => (e.Status != CUser.DEACTIVATED && ((e.UserType == CUser.DEALER || e.UserType == CUser.SUB_DEALER) || (e.UserType == CUser.ADMIN && e.Id == param.UserId)) && e.CompanyId == param.CompanyId)).OrderBy(e => e.UserType).ThenBy(e => e.Id);

                                if (datas != null)
                                {
                                    foreach (var i in datas)
                                    {
                                        CUser u = new CUser();
                                        u.CompanyId = i.CompanyId;
                                        u.Status = i.Status;
                                        u.Name = i.Name;
                                        u.UserType = i.UserType;
                                        u.Id = i.Id;
                                        u.ParentUserId = i.ParentUserId;
                                        users.Add(u);
                                    }
                                }
                            }
                            else if (si.UserType == CUser.DEALER)
                            {
                                var datas = dataB.Accounts.Select(e => e).Where(e => (e.Status != CUser.DEACTIVATED && ((e.Id == param.UserId || e.ParentUserId == param.UserId)) && e.CompanyId == param.CompanyId)).OrderBy(e => e.UserType).ThenBy(e => e.Id);

                                if (datas != null)
                                {
                                    foreach (var i in datas)
                                    {
                                        CUser u = new CUser();
                                        u.CompanyId = i.CompanyId;
                                        u.Status = i.Status;
                                        u.Name = i.Name;
                                        u.UserType = i.UserType;
                                        u.Id = i.Id;
                                        u.ParentUserId = i.ParentUserId;
                                        users.Add(u);
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return users;
        }

        public List<CUser> GetSubUsersWithStatus(CGetSubUsersParam param)
        {
            List<CUser> users = new List<CUser>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                return users;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {
                            if (si.UserType == CUser.SUPER_ADMIN)
                            {
                                var datas = dataB.Accounts.Select(e => e).Where(e => ((e.UserType == CUser.DEALER || e.UserType == CUser.SUB_DEALER) && e.CompanyId == param.CompanyId)).OrderBy(e => e.UserType).ThenBy(e => e.Id);

                                if (datas != null)
                                {
                                    foreach (var i in datas)
                                    {
                                        CUser u = new CUser();
                                        u.CompanyId = i.CompanyId;
                                        u.Status = i.Status;
                                        u.Name = i.Name;
                                        u.Username = i.Username;
                                        u.UserType = i.UserType;
                                        u.Id = i.Id;
                                        u.ParentUserId = i.ParentUserId;
                                        u.Password = i.Password;
                                        users.Add(u);
                                    }
                                }
                            }
                            else if (si.UserType == CUser.ADMIN)
                            {
                                var datas = dataB.Accounts.Select(e => e).Where(e => ((e.UserType == CUser.DEALER || e.UserType == CUser.SUB_DEALER) && e.CompanyId == param.CompanyId)).OrderBy(e => e.UserType).ThenBy(e => e.Id);

                                if (datas != null)
                                {
                                    foreach (var i in datas)
                                    {
                                        CUser u = new CUser();
                                        u.CompanyId = i.CompanyId;
                                        u.Status = i.Status;
                                        u.Name = i.Name;
                                        u.Username = i.Username;
                                        u.UserType = i.UserType;
                                        u.Id = i.Id;
                                        u.ParentUserId = i.ParentUserId;
                                        u.Password = i.Password;
                                        users.Add(u);
                                    }
                                }
                            }
                            else if (si.UserType == CUser.DEALER)
                            {
                                var datas = dataB.Accounts.Select(e => e).Where(e => (e.ParentUserId == param.RequestUserId && e.CompanyId == param.CompanyId)).OrderBy(e => e.UserType).ThenBy(e => e.Id);

                                if (datas != null)
                                {
                                    foreach (var i in datas)
                                    {
                                        CUser u = new CUser();
                                        u.CompanyId = i.CompanyId;
                                        u.Status = i.Status;
                                        u.Name = i.Name;
                                        u.Username = i.Username;
                                        u.UserType = i.UserType;
                                        u.Id = i.Id;
                                        u.ParentUserId = i.ParentUserId;
                                        u.Password = i.Password;
                                        users.Add(u);
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return users;
        }

        public List<CUser> GetDealers(CGetDealersParam param)
        {
            List<CUser> users = new List<CUser>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType!=CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                return users;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {
                            var datas = dataB.Accounts.Select(e => e).Where(e => (e.UserType == CUser.DEALER && e.CompanyId == param.CompanyId)).OrderBy(e => e.Id);

                            if (datas != null)
                            {
                                foreach (var i in datas)
                                {
                                    CUser u = new CUser();
                                    u.CompanyId = i.CompanyId;
                                    u.Status = i.Status;
                                    u.Name = i.Name;
                                    u.Username = i.Username;
                                    u.UserType = i.UserType;
                                    u.Id = i.Id;
                                    u.ParentUserId = i.ParentUserId;
                                    users.Add(u);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return users;
        }

        public List<CUser> GetOutDealers(CGetDealersParam param)
        {
            List<CUser> users = new List<CUser>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                return users;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {
                            var datas = dataB.Accounts.Select(e => e).Where(e => (e.UserType == CUser.OUT_DEALER && e.CompanyId == param.CompanyId)).OrderBy(e => e.Id);

                            if (datas != null)
                            {
                                foreach (var i in datas)
                                {
                                    CUser u = new CUser();
                                    u.CompanyId = i.CompanyId;
                                    u.Status = i.Status;
                                    u.Name = i.Name;
                                    u.Username = i.Username;
                                    u.UserType = i.UserType;
                                    u.Id = i.Id;
                                    u.ParentUserId = i.ParentUserId;
                                    users.Add(u);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return users;
        }

        public CBool ToggleUserStatus(CToggleUserStatusParam param)
        {
            CBool isSuccess = new CBool();
            isSuccess.Value=false;
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);            
            if (si == null || si.UserId != param.RequestUserId)
            {
                return isSuccess;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {
                            if (si.UserType == CUser.SUPER_ADMIN || si.UserType==CUser.ADMIN)
                            {
                                var dataBTransaction = dataB.Database.BeginTransaction();
                                try
                                {
                                    Account u = dataB.Accounts.Select(c => c).Where(x => x.Id == param.UserId && x.CompanyId == param.CompanyId).FirstOrDefault();
                                    if (u!=null)
                                    {
                                        u.Status = param.IsActive?CUser.ACTIVATED:CUser.DEACTIVATED;

                                    }                                    

                                    dataB.SaveChanges();                                                                        
                                    dataBTransaction.Commit();
                                    isSuccess.Value = true;
                                }
                                catch (Exception e)
                                {                                    
                                    dataBTransaction.Rollback();                                    
                                }
                                finally
                                {

                                }
                            }                            
                            else if (si.UserType == CUser.DEALER)
                            {
                                var dataBTransaction = dataB.Database.BeginTransaction();
                                try
                                {
                                    Account u = dataB.Accounts.Select(c => c).Where(x => x.Id == param.UserId && x.CompanyId == param.CompanyId && x.ParentUserId==param.RequestUserId).FirstOrDefault();
                                    if (u != null)
                                    {
                                        u.Status = param.IsActive ? CUser.ACTIVATED : CUser.DEACTIVATED;

                                    }

                                    dataB.SaveChanges();
                                    dataBTransaction.Commit();
                                    isSuccess.Value = true;
                                }
                                catch (Exception e)
                                {
                                    dataBTransaction.Rollback();
                                }
                                finally
                                {

                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return isSuccess;
        }

        public List<CTicket> GetTicketsByNoOfDigits(CGetTicketsByNoOfDigitsParam param)
        {
            List<CTicket> tickets = new List<CTicket>();
            try
            {
                CSession sesObj = new CSession();                
                Session si = sesObj.GetSession(param.SessionId);
                if (si == null )
                {
                    return tickets;
                }
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {
                            IOrderedQueryable<Ticket> ticketData;
                            if (param.NoOfDigits == 0)
                            {
                                ticketData = dataB.Tickets.Where<Ticket>(t => (t.CompanyId == param.CompanyId && t.IsActive == true)).OrderBy(t => t.NoOfDigits).ThenBy(t => t.Type);
                            }
                            else
                            {
                                ticketData = dataB.Tickets.Where<Ticket>(t => (t.NoOfDigits == (param.NoOfDigits - 1) && t.CompanyId == param.CompanyId && t.IsActive == true)).OrderBy(t => t.NoOfDigits).ThenBy(t => t.Type);
                            }
                            if (ticketData != null)
                            {
                                foreach (var i in ticketData)
                                {
                                    CTicket ct = new CTicket();
                                    ct.CompanyId = i.CompanyId;
                                    ct.IsActive = i.IsActive;
                                    ct.Mask = i.Mask;
                                    ct.Name = i.Name;
                                    ct.NoOfDigits = i.NoOfDigits;
                                    ct.TicketId = i.Id;
                                    ct.Type = i.Type;
                                    tickets.Add(ct);
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return tickets;
        }


        public CBulkTicketSalesMessage AddSalesBillItems(List<CTicketSalesParam> oSales)
        {
            CBulkTicketSalesMessage cbm = new CBulkTicketSalesMessage();
            cbm.IsSuccess = false;
            try
            {
                SettingsService ss = new SettingsService();
                long dayCode = ss.GetDayCode(oSales.ElementAt(0).CompanyId, oSales.ElementAt(0).AddedUserId);
                if (dayCode == -1)
                {
                    cbm.Message = "Not Allowed to Enter Item at this Time";
                    cbm.SuccessScale = CBulkTicketSalesMessage.FAILED;
                    cbm.DisplayMessage = "Not Allowed at this Time.";
                    return cbm;
                }

                long companyId = oSales.ElementAt(0).CompanyId;
                long userId = oSales.ElementAt(0).UserId;
                UserRegisterService urs = new UserRegisterService();
                CUser user = urs.ReadUser(userId, companyId, oSales.ElementAt(0).SessionId);

                //long userIdx = oSales.ElementAt(0).UserType == CUser.ADMIN || oSales.ElementAt(0).UserType == CUser.DEALER ? oSales.ElementAt(0).UserId : oSales.ElementAt(0).ParentUserId;
                //long subDealerId = oSales.ElementAt(0).UserType == CUser.SUB_DEALER || oSales.ElementAt(0).UserType == CUser.DEALER ? oSales.ElementAt(0).UserId : 0;
                long userIdx = user.UserType == CUser.ADMIN || user.UserType == CUser.DEALER ? userId : user.ParentUserId;
                long subDealerId = user.UserType == CUser.SUB_DEALER || user.UserType == CUser.DEALER ? userId : 0;

                //lock (Synchronizer.@lock)
                CSession sesObj = new CSession();
                Session si = sesObj.GetSession(oSales.ElementAt(0).SessionId);
                if (si != null && si.UserId == oSales.ElementAt(0).AddedUserId)
                {

                    //Right Checks to Enter Data
                    if(si.UserType!=CUser.SUPER_ADMIN&& si.UserType != CUser.ADMIN)
                    {
                        if (si.UserType == CUser.DEALER && (user.ParentUserId!=si.UserId && user.Id != si.UserId))
                        {
                            cbm.Message = "Not Allowed : Wrong User Associations.";
                            cbm.SuccessScale = CBulkTicketSalesMessage.FAILED;
                            cbm.DisplayMessage = "Not Allowed : Wrong User Associations";
                            return cbm;
                        }
                        else if (si.UserType != CUser.DEALER && user.Id != si.UserId)
                        {
                            cbm.Message = "Not Allowed : Wrong User Associations.";
                            cbm.SuccessScale = CBulkTicketSalesMessage.FAILED;
                            cbm.DisplayMessage = "Not Allowed : Wrong User Associations";
                            return cbm;
                        }
                    }


                    using (var dataB = new AquaStorage())
                    {

                        try
                        {
                            //Set success for initialize
                            cbm.SuccessScale = CBulkTicketSalesMessage.SUCCESS;

                            BillNoGeneratorService bns = new BillNoGeneratorService();
                            long billNo = bns.GetNextBillNo(userId, companyId, BillNoGeneratorService.ENTRY_BILLNO);

                            UserTicketSettingsService ds = new UserTicketSettingsService();
                            TicketRegisterService trs = new TicketRegisterService();

                            //AccountsService ass = new AccountsService();

                            int serialNo = 1;
                            string numbers = "";
                            foreach (var i in oSales)
                            {

                                CUserTicketRateItemParam companyUserRate = ds.ReadTicketRateForUser(userIdx, i.TicketId, i.CompanyId);
                                CUserTicketRateItemParam dealerUserRate = ds.ReadTicketRateForUser(subDealerId, i.TicketId, i.CompanyId);
                                CTicket ct= trs.ReadTicket(i.TicketId, i.CompanyId);
                                using (var dbTransaction = dataB.Database.BeginTransaction())
                                {
                                    try
                                    {

                                        //Use ExecuteSQLCommand
                                        string sInsert = "Insert Into TicketSalesCaches (BillNo, BillDate, SerialNo, TicketNo, Count, Cost, CompanyCommission, DealerCommission, DayCode, TicketId, TicketName, NoOfDigits, Mask, TicketType, TicketMask, UserId, UserName, UserType, ParentUserId, AddedUserId, CompanyId) ";
                                        string sValues = "Values (@billNo, @billDate, @serialNo, @ticketNo, @Count, @cost, @companyCommission, @dealerCommission, @dayCode, @ticketId, @ticketName, @noOfDigits, @mask, @ticketType, @ticketMask, @userId, @userName, @userType, @parentUserId, @addedUserId, @companyId) ";
                                        string sQuery = sInsert + sValues;

                                        dataB.Database.ExecuteSqlCommand(sQuery,
                                            new MySqlParameter("@billNo", billNo),
                                            new MySqlParameter("@billDate", System.DateTime.Now),
                                            new MySqlParameter("@serialNo", serialNo++),
                                            new MySqlParameter("@ticketNo", i.TicketNo),
                                            new MySqlParameter("@Count", i.Count),
                                            new MySqlParameter("@cost", ct.Cost),
                                            new MySqlParameter("@companyCommission", companyUserRate.Commission),                                            
                                            new MySqlParameter("@dealerCommission", dealerUserRate == null ? 0 : dealerUserRate.Commission),
                                            new MySqlParameter("@dayCode", dayCode),
                                            new MySqlParameter("@ticketId", ct.TicketId),
                                            new MySqlParameter("@ticketName", ct.Name),
                                            new MySqlParameter("@noOfDigits", ct.NoOfDigits),
                                            new MySqlParameter("@mask", ct.Mask),
                                            new MySqlParameter("@ticketType", ct.Type),
                                            new MySqlParameter("@ticketMask", ct.Mask),
                                            new MySqlParameter("@userId", userId),
                                            new MySqlParameter("@userName", user.Name),
                                            new MySqlParameter("@userType", user.UserType),
                                            new MySqlParameter("@parentUserId", user.UserType==CUser.ADMIN|| user.UserType == CUser.DEALER?userId: user.ParentUserId),
                                            new MySqlParameter("@addedUserId", i.AddedUserId),
                                            new MySqlParameter("@companyId", companyId));

                                        dbTransaction.Commit();
                                    }
                                    catch (Exception e)
                                    {
                                        dbTransaction.Rollback();

                                        cbm.Message = "Some Of Items are not added due to Day Limit.";
                                        cbm.SuccessScale = CBulkTicketSalesMessage.PARTIAL;
                                        cbm.BlockedNumbers += i.TicketId + ":" + i.TicketNo + ":" + i.Count + ",";
                                       
                                        numbers += i.TicketNo + "("+e.Message+"),";
                                    }
                                }
                            }
                            

                            bns.SetBillNo(userId, companyId, BillNoGeneratorService.ENTRY_BILLNO, billNo + 1);


                            //Success
                            cbm.IsSuccess = true;
                            cbm.Message += "Bill No is " + billNo;

                            //dataBTransaction.Commit();

                            if (cbm.SuccessScale == CBulkTicketSalesMessage.PARTIAL)
                            {
                                numbers = numbers.Substring(0, numbers.Length - 1);
                                cbm.DisplayMessage = "Partially Saved with Bill No " + billNo + ", These Numbers are Blocked [" + numbers + "]";
                            }
                            else
                            {
                                cbm.DisplayMessage = "Successfully Saved with Bill No " + billNo;
                            }
                        }
                        catch (Exception e)
                        {
                            cbm.IsSuccess = false;
                            cbm.Message = e.Message;
                            cbm.SuccessScale = CBulkTicketSalesMessage.FAILED;
                            cbm.DisplayMessage = "Error : Couldnt Save, Please try again later.";
                            //dataBTransaction.Rollback();
                        }
                        finally
                        {

                        }
                    }
                }
                else
                {
                    cbm.Message = "Session Error";
                    cbm.SuccessScale = CBulkTicketSalesMessage.FAILED;
                    cbm.DisplayMessage = "Session Error";
                }
            }
            catch (Exception ex)
            {
                cbm.Message = ex.Message;
                cbm.SuccessScale = CBulkTicketSalesMessage.FAILED;
                cbm.DisplayMessage = "Error : Couldnt Save, Please try again later.";
            }
            return cbm;
        }

        public CBoolMessage DeleteSalesBillItems(List<CDeleteSalesBillItemsParam> oSales)
        {
            CBoolMessage cbm = new CBoolMessage();

            cbm.IsSuccess = true;
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(oSales.ElementAt(0).SessionId);
            if (si != null && si.UserId == oSales.ElementAt(0).RequestUserId)
            {                
                try
                {
                    foreach (var i in oSales)
                    {
                        CBoolMessage tm = DeleteBillItem(i);
                        if (!tm.IsSuccess)
                        {
                            cbm.IsSuccess = false;
                            cbm.Message += "-" + tm.Message;
                        }
                    }
                }
                catch (Exception e) { cbm.Message = e.Message; }
            }else
            {
                cbm.IsSuccess = false;
                cbm.Message = "Session Error";
            }
            return cbm;
        }

        private CBoolMessage DeleteBillItem(CDeleteSalesBillItemsParam oSales)
        {
            CBoolMessage cbm = new CBoolMessage();

            cbm.IsSuccess = false;
            try
            {
                SettingsService ss = new SettingsService();
                long dayCode = ss.GetDayCode(oSales.CompanyId,oSales.RequestUserId);
                if (dayCode == -1)
                {
                    cbm.Message = "Not Allowed to Delete Item at this Time";
                    return cbm;
                }

                //lock (Synchronizer.@lock)
                { 
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {

                            var cr = dataB.TicketSalesCache.Select(c => c).Where(x => x.DayCode == dayCode && x.Id == oSales.BillId);
                            var r = dataB.TicketSalesCache.RemoveRange(cr);
                            if (r.Count() > 0)
                            {
                                //Decrement Serial no of all items in the bill that follows this item

                                var bupdate = dataB.TicketSalesCache.Select(c => c).Where(x => x.BillNo == oSales.BillNo && x.CompanyId == oSales.CompanyId && x.UserId == oSales.UserId && x.SerialNo > oSales.SerialNo);
                                foreach (var i in bupdate)
                                {
                                    i.SerialNo = i.SerialNo > oSales.SerialNo ? --i.SerialNo : i.SerialNo;
                                }

                                dataB.SaveChanges();
                                

                                dataBTransaction.Commit();
                                cbm.IsSuccess = true;
                            }
                            else
                            {
                                cbm.IsSuccess = false;
                                cbm.Message = "Bill Item Doesnt Exist Or Is not available to Delete";
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
            }
            catch (Exception e) { cbm.Message = e.Message; }
            return cbm;
        }


        public CBoolMessage DeleteOutDealerSalesBillItems(List<CDeleteSalesBillItemsParam> oSales)
        {
            CBoolMessage cbm = new CBoolMessage();

            cbm.IsSuccess = true;
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(oSales.ElementAt(0).SessionId);
            if (si != null && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN))
            {
                try
                {
                    foreach (var i in oSales)
                    {
                        CBoolMessage tm = DeleteOutDealerBillItem(i);
                        if (!tm.IsSuccess)
                        {
                            cbm.IsSuccess = false;
                            cbm.Message += "-" + tm.Message;
                        }
                    }
                }
                catch (Exception e) { cbm.Message = e.Message; }
            }
            else
            {
                cbm.IsSuccess = false;
                cbm.Message = "Session Error";
            }
            return cbm;
        }

        private CBoolMessage DeleteOutDealerBillItem(CDeleteSalesBillItemsParam oSales)
        {
            CBoolMessage cbm = new CBoolMessage();

            cbm.IsSuccess = false;
            try
            {
                SettingsService ss = new SettingsService();
                long dayCode = ss.GetDayCode(oSales.CompanyId, oSales.RequestUserId);
                if (dayCode == -1)
                {
                    cbm.Message = "Not Allowed to Delete Item at this Time";
                    return cbm;
                }

                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {

                            var cr = dataB.OutDealerSales.Select(c => c).Where(x => x.DayCode == dayCode && x.Id == oSales.BillId);
                            var r = dataB.OutDealerSales.RemoveRange(cr);
                            if (r.Count() > 0)
                            {
                                //Decrement Serial no of all items in the bill that follows this item

                                var bupdate = dataB.OutDealerSales.Select(c => c).Where(x => x.CompanyId == oSales.CompanyId && x.UserId == oSales.UserId && x.BillNo == oSales.BillNo && x.SerialNo > oSales.SerialNo);
                                foreach (var i in bupdate)
                                {
                                    i.SerialNo = i.SerialNo > oSales.SerialNo ? --i.SerialNo : i.SerialNo;
                                }

                                dataB.SaveChanges();

                                dataBTransaction.Commit();
                                cbm.IsSuccess = true;
                            }
                            else
                            {
                                cbm.IsSuccess = false;
                                cbm.Message = "Bill Item Doesnt Exist Or Is not available to Delete";
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
            }
            catch (Exception e) { cbm.Message = e.Message; }
            return cbm;
        }


        public CLong ReadNextBillNo(CReadNextBillNoParam param)
        {
            CLong cl = new CLong();
            try
            {
                BillNoGeneratorService bns = new BillNoGeneratorService();
                cl.Value = bns.GetNextBillNo(param.UserId, param.CompanyId, BillNoGeneratorService.ENTRY_BILLNO);
            }
            catch { cl.Value = -1; }
            return cl;
        }


        public CRTicketSalesReportReply GetSalesReport(CGetSalesReportParam param)
        {
            CRTicketSalesReportReply retObj = new CRTicketSalesReportReply();
            retObj.Success = true;

            List<CRTicketSalesReport> ticketSReport = new List<CRTicketSalesReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId!=param.RequestUserId)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = ticketSReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        //DayCodes                    
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        int SelectTable = -1;
                        SettingsService ss = new SettingsService();
                        long dayCode = ss.GetDayCode(param.CompanyId, param.RequestUserId);
                        if (endD >= dayCode && startD >= dayCode && dayCode != -1)
                        {
                            SelectTable = 0;
                        }
                        else if (endD >= dayCode || dayCode == -1)
                        {
                            SelectTable = 1;
                        }
                        else
                        {
                            SelectTable = 2;
                        }

                        string companyIdQuery = " && (TS.CompanyId=@companyId)";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (TS.ParentUserId = @userId)" : " && (TS.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (TS.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (TS.NoOfDigits=@noOfDigits) ";

                        string amountQ = si.UserType == CUser.DEALER || si.UserType == CUser.SUB_DEALER ? "(TS.Count*(TS.Cost-TS.DealerCommission))" : "(TS.Count*(TS.Cost-TS.CompanyCommission))";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        if (SelectTable == 0)
                        {
                            ticketSReport = dataB.Database.SqlQuery<CRTicketSalesReport>("Select TS.DayCode, TS.UserType, TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, TS.UserId, TS.UserName, TS.TicketName, TS.TicketNo, TS.Count, " + amountQ + " As Amount From TicketSalesCaches TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + " Order By TS.DayCode Desc,TS.UserType,TS.UserName,TS.BillNo Desc,TS.SerialNo Limit @limitIndex,@limitSize",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),                            
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                        }
                        else if (SelectTable == 1)
                        {
                            ticketSReport = dataB.Database.SqlQuery<CRTicketSalesReport>("(Select TS.DayCode, TS.UserType, TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, TS.UserId, TS.UserName, TS.TicketName, TS.TicketNo, TS.Count, " + amountQ + " As Amount From TicketSalesCaches TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + ") Union All (Select TS.DayCode, TS.UserType, TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, TS.UserId, TS.UserName, TS.TicketName, TS.TicketNo, TS.Count, " + amountQ + " As Amount From TicketSales TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + ") Order By DayCode Desc,UserType,UserName,BillNo Desc,SerialNo Limit @limitIndex,@limitSize",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),                            
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                        }
                        else
                        {
                            ticketSReport = dataB.Database.SqlQuery<CRTicketSalesReport>("Select TS.DayCode, TS.UserType, TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, TS.UserId, TS.UserName, TS.TicketName, TS.TicketNo, TS.Count, " + amountQ + " As Amount From TicketSales TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + " Order By TS.DayCode Desc,TS.UserType,TS.UserName,TS.BillNo Desc,TS.SerialNo Limit @limitIndex,@limitSize",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),                            
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                        }
                    }
                }
            }
            catch(Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = ticketSReport;
            return retObj;
        }

        public CRTicketSalesReportReply GetSalesDetailedReport(CGetSalesReportParam param)
        {
            CRTicketSalesReportReply retObj = new CRTicketSalesReportReply();
            retObj.Success = true;

            List<CRTicketSalesReport> ticketSReport = new List<CRTicketSalesReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = ticketSReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        //DayCodes                    
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        int SelectTable = -1;
                        SettingsService ss = new SettingsService();
                        long dayCode = ss.GetDayCode(param.CompanyId,param.RequestUserId);
                        if (endD >= dayCode && startD >= dayCode && dayCode != -1)
                        {
                            SelectTable = 0;
                        }
                        else if (endD >= dayCode || dayCode == -1)
                        {
                            SelectTable = 1;
                        }
                        else
                        {
                            SelectTable = 2;
                        }

                        string companyIdQuery = " && (TS.CompanyId=@companyId)";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (TS.ParentUserId = @userId)" : " && (TS.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (TS.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (TS.NoOfDigits=@noOfDigits) ";

                        string amountQ = si.UserType == CUser.DEALER || si.UserType == CUser.SUB_DEALER ? "(TS.Count*(TS.Cost-TS.DealerCommission))" : "(TS.Count*(TS.Cost-TS.CompanyCommission))";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        if (SelectTable == 0)
                        {
                            ticketSReport = dataB.Database.SqlQuery<CRTicketSalesReport>("(Select TS.DayCode, TS.UserType, TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, TS.UserId, TS.UserName, TS.TicketName, TS.TicketNo, TS.Count, " + amountQ + " As Amount From TicketSalesCaches TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + ") Union All (Select TS.DayCode, TS.UserType, 0 As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, 0 As SerialNo, TS.UserId, TS.UserName, '' As TicketName, '' As TicketNo, Sum(TS.Count) As Count, Sum(" + amountQ + ") As Amount From TicketSalesCaches TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + " Group By TS.DayCode, TS.UserType, BillYear, BillMonth, BillDay, TS.BillNo, TS.UserId, TS.UserName) Order By DayCode Desc,UserType,UserName,BillNo Desc,SerialNo Limit @limitIndex,@limitSize",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@requestUserId", param.RequestUserId),
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                        }
                        else if (SelectTable == 1)
                        {
                            ticketSReport = dataB.Database.SqlQuery<CRTicketSalesReport>("(Select DayCode, UserType, BillId, BillYear, BillMonth, BillDay, BillNo, SerialNo, UserId, UserName, TicketName, TicketNo, Count, Amount From (Select TS.DayCode, TS.UserType, TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, TS.UserId, TS.UserName, TS.TicketName, TS.TicketNo, TS.Count, " + amountQ + " As Amount From TicketSalesCaches TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + " Union All Select TS.DayCode, TS.UserType, 0 As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, 0 As SerialNo, TS.UserId, TS.UserName, '' As TicketName, '' As TicketNo, Sum(TS.Count) As Count, Sum(" + amountQ + ") As Amount From TicketSalesCaches TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + " Group By TS.DayCode, TS.UserType, BillYear, BillMonth, BillDay, TS.BillNo, TS.UserId, TS.UserName) As T) Union All (Select DayCode, UserType, BillId, BillYear, BillMonth, BillDay, BillNo, SerialNo, UserId, UserName, TicketName, TicketNo, Count, Amount From (Select TS.DayCode, TS.UserType, TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, TS.UserId, TS.UserName, TS.TicketName, TS.TicketNo, TS.Count, " + amountQ + " As Amount From TicketSales TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + " Union All Select TS.DayCode, TS.UserType, 0 As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, 0 As SerialNo, TS.UserId, TS.UserName, '' As TicketName, '' As TicketNo, Sum(TS.Count) As Count, Sum(" + amountQ + ") As Amount From TicketSales TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + " Group By TS.DayCode, TS.UserType, BillYear, BillMonth, BillDay, TS.BillNo, TS.UserId, TS.UserName) As T) Order By DayCode Desc,UserType,UserName,BillNo Desc,SerialNo Limit @limitIndex,@limitSize",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@requestUserId", param.RequestUserId),
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                        }
                        else
                        {
                            ticketSReport = dataB.Database.SqlQuery<CRTicketSalesReport>("(Select TS.DayCode, TS.UserType, TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, TS.UserId, TS.UserName, TS.TicketName, TS.TicketNo, TS.Count, " + amountQ + " As Amount From TicketSales TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + ") Union All (Select TS.DayCode, TS.UserType, 0 As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, 0 As SerialNo, TS.UserId, TS.UserName, '' As TicketName, '' As TicketNo, Sum(TS.Count) As Count, Sum(" + amountQ + ") As Amount From TicketSales TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + " Group By TS.DayCode, TS.UserType, BillYear, BillMonth, BillDay, TS.BillNo, TS.UserId, TS.UserName) Order By DayCode Desc,UserType,UserName,BillNo Desc,SerialNo Limit @limitIndex,@limitSize",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@requestUserId", param.RequestUserId),
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = ticketSReport;
            return retObj;
        }

        public CRWinnersReportReply GetWinnersReport(CGetWinnersReportParam param)
        {
            CRWinnersReportReply retObj = new CRWinnersReportReply();
            retObj.Success = true;

            List<CRWinnersReport> ticketWReport = new List<CRWinnersReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = ticketWReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (W.CompanyId=@companyId) ";
                        //string userIdQuery = param.UserId == 0 ? si.UserType == CUser.SUPER_ADMIN ? "" : si.UserType == CUser.ADMIN ? " && (((W.UserType=" + CUser.DEALER + ") || (W.UserType=" + CUser.SUB_DEALER + ")) || (W.UserId=@requestUserId))" : " && ((W.UserId=@requestUserId)||(W.ParentUserId=@requestUserId))" : " && (W.UserId=@userId)";
                        string userIdQuery = param.UserId == 0 ? si.UserType == CUser.SUPER_ADMIN ? "" : si.UserType == CUser.ADMIN ? " && ((W.UserType=" + CUser.DEALER + ") || (W.UserType=" + CUser.SUB_DEALER + "))" : si.UserType == CUser.DEALER ? " && (W.ParentUserId=@requestUserId || W.UserId=@requestUserId)" : " && (W.UserId=@requestUserId)" : " && (W.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (W.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (W.NoOfDigits=@noOfDigits) ";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        string qSelect = "Select W.DayCode, W.PrizeId, W.NoOfDigits, W.Mask, Year(W.BillDate) As BillYear, Month(W.BillDate) As BillMonth, Day(W.BillDate) As BillDay, W.SerialNo, W.BillNo, W.UserName, W.TicketName, W.PrizeName, W.TicketNo, W.Count, W.PrizeAmount, W.Commission ";
                        string qFrom = "From Winners W ";
                        string qWhere = "Where (W.DayCode>=@startD && W.DayCode<=@endD) ";
                        string qOrderBy = " Order By W.DayCode Desc, W.UserName, W.PrizeId, W.NoOfDigits Desc Limit @limitIndex,@limitSize";

                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrderBy;

                        ticketWReport = dataB.Database.SqlQuery<CRWinnersReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@requestUserId", param.RequestUserId),
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = ticketWReport;
            return retObj;
        }

        public CRResultReportReply GetResultReport(CGetResultReportParam param)
        {

            CRResultReportReply retObj = new CRResultReportReply();
            retObj.Success = true;

            List<CRResultReport> resultReport = new List<CRResultReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = resultReport;
                return retObj;
            }


            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long date = long.Parse(string.Format("{0}{1:00}{2:00}", param.Year, param.Month, param.Day));

                        string companyIdQuery = " && (PW.CompanyId=@companyId) && (DPS.CompanyId=@companyId) && (T.CompanyId=@companyId) && (P.CompanyId=@companyId) ";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (T.Id=@ticketId) ";

                        string subQ = companyIdQuery + ticketIdQuery;

                        string qSelect = "Select T.Name As TicketName, P.Name As PrizeName, PW.Number, DPS.Mask ";
                        string qFrom = "From PrizeWins PW, TicketPrizeSettings DPS, Tickets T, Prizes P ";
                        string qWhere = "Where (P.Id=DPS.PrizeId) && (DPS.PrizeId=PW.PrizeId) && (PW.DayCode=@date) && (T.Id=DPS.TicketId)";
                        string qOrder = " Order By Length(DPS.Mask), T.Id, DPS.Mask, PW.PrizeId";

                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrder;

                        var resData = dataB.Database.SqlQuery<CRResultReport>(sQuery,
                            new MySqlParameter("@date", date),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@companyId", param.CompanyId));

                        foreach (var i in resData)
                        {
                            resultReport.Add(new CRResultReport() { Mask = i.Mask, Number = GetNumberForMask(i.Number, i.Mask), PrizeName = i.PrizeName, TicketName = i.TicketName });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = resultReport;
            return retObj;
        }

        public CRProfitnLossReportReply GetProfitnLossReport(CGetProfitnLossReportParam param)
        {
            CRProfitnLossReportReply retObj = new CRProfitnLossReportReply();
            retObj.Success = true;

            List<CRProfitnLossReport> pnlReport = new List<CRProfitnLossReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = pnlReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (PNL.ParentUserId = @userId)" : " && (PNL.UserId=@userId)";
                        //string userIdQuery = param.UserId == 0 ? si.UserType == CUser.SUPER_ADMIN ? "" : si.UserType == CUser.ADMIN ? " && (((PNL.UserType=" + CUser.DEALER + ") || (PNL.UserType=" + CUser.SUB_DEALER + ")) || (PNL.UserId=@parentUserId))" : " && ((PNL.UserId=@parentUserId)||(PNL.ParentUserId=@parentUserId))" : " && (PNL.UserId=@userId)";
                        string subQ = companyIdQuery + userIdQuery;
                        string amountQ = si.UserType == CUser.DEALER || si.UserType == CUser.SUB_DEALER ? "(PNL.DealerSalesAmount)" : "(PNL.CompanySalesAmount)";
                        string qSelect = "Select PNL.DayCode, PNL.UserType, PNL.UserName, " + amountQ + " As SalesAmount, PNL.WinningAmount ";
                        string qFrom = "From ProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";
                        string qOrder = " Order By PNL.DayCode Desc, PNL.UserType";

                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrder;

                        pnlReport = dataB.Database.SqlQuery<CRProfitnLossReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),                            
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = pnlReport;
            return retObj;
        }

        public CRProfitnLossReportReply GetProfitnLossBriefReport(CGetProfitnLossReportParam param)
        {
            CRProfitnLossReportReply retObj = new CRProfitnLossReportReply();
            retObj.Success = true;

            List<CRProfitnLossReport> pnlReport = new List<CRProfitnLossReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = pnlReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (PNL.ParentUserId = @userId)" : " && (PNL.UserId=@userId)";
                        string userNameQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "(U.Id=PNL.ParentUserId)" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? "(U.Id=PNL.ParentUserId)" : "(U.Id=PNL.UserId)";
                        string subQ = companyIdQuery + userIdQuery;
                        string amountQ = si.UserType == CUser.DEALER || si.UserType == CUser.SUB_DEALER ? "Sum(PNL.DealerSalesAmount)" : "Sum(PNL.CompanySalesAmount)";
                        string qSelect = "Select (Select U.Name From Users U Where"+ userNameQuery + ") As UserName, " + amountQ + " As SalesAmount, Sum(PNL.WinningAmount) As WinningAmount ";
                        string qFrom = "From ProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";
                        string qOrder = " Group by PNL.ParentUserId Order By PNL.UserType";

                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrder;

                        pnlReport = dataB.Database.SqlQuery<CRProfitnLossReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = pnlReport;
            return retObj;
        }

        public CRProfitnLossReportReply GetDealerNetPaymentReport(CGetProfitnLossReportParam param)
        {

            CRProfitnLossReportReply retObj = new CRProfitnLossReportReply();
            retObj.Success = true;

            List<CRProfitnLossReport> pnlReport = new List<CRProfitnLossReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = pnlReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (PNL.ParentUserId = @userId)" : " && (PNL.UserId=@userId)";
                        //string userIdQuery = param.UserId == 0 ? si.UserType == CUser.SUPER_ADMIN ? "" : si.UserType == CUser.ADMIN ? " && (((PNL.UserType=" + CUser.DEALER + ") || (PNL.UserType=" + CUser.SUB_DEALER + ")) || (PNL.UserId=@parentUserId))" : " && ((PNL.UserId=@parentUserId)||(PNL.ParentUserId=@parentUserId))" : " && (PNL.UserId=@userId)";
                        string subQ = companyIdQuery + userIdQuery;
                        string amountQ = "(PNL.CompanySalesAmount)";
                        string qSelect = "Select PNL.DayCode, PNL.UserType, PNL.UserName, " + amountQ + " As SalesAmount, PNL.WinningAmount ";
                        string qFrom = "From ProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";
                        string qOrder = " Order By PNL.DayCode Desc, PNL.UserType";

                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrder;

                        pnlReport = dataB.Database.SqlQuery<CRProfitnLossReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),                            
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = pnlReport;
            return retObj;
        }

        public CRProfitnLossReportReply GetDealerNetPaymentBriefReport(CGetProfitnLossReportParam param)
        {

            CRProfitnLossReportReply retObj = new CRProfitnLossReportReply();
            retObj.Success = true;

            List<CRProfitnLossReport> pnlReport = new List<CRProfitnLossReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = pnlReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (PNL.ParentUserId = @userId)" : " && (PNL.UserId=@userId)";
                        //string userIdQuery = param.UserId == 0 ? si.UserType == CUser.SUPER_ADMIN ? "" : si.UserType == CUser.ADMIN ? " && (((PNL.UserType=" + CUser.DEALER + ") || (PNL.UserType=" + CUser.SUB_DEALER + ")) || (PNL.UserId=@parentUserId))" : " && ((PNL.UserId=@parentUserId)||(PNL.ParentUserId=@parentUserId))" : " && (PNL.UserId=@userId)";
                        string subQ = companyIdQuery + userIdQuery;
                        string amountQ = "Sum(PNL.CompanySalesAmount)";
                        string qSelect = "Select PNL.UserType, PNL.UserName, " + amountQ + " As SalesAmount, Sum(PNL.WinningAmount) As WinningAmount ";
                        string qFrom = "From ProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";
                        string qOrder = " Order By PNL.UserType";

                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrder;

                        pnlReport = dataB.Database.SqlQuery<CRProfitnLossReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = pnlReport;
            return retObj;
        }

        public CRWinnersReportReply GetWinnersDetailedReport(CGetWinnersReportParam param)
        {
            CRWinnersReportReply retObj = new CRWinnersReportReply();
            retObj.Success = true;

            List<CRWinnersReport> ticketWReport = new List<CRWinnersReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = ticketWReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (W.CompanyId=@companyId) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (W.ParentUserId = @userId)" : " && (W.UserId=@userId)";
                        //string userIdQuery = param.UserId == 0 ? si.UserType == CUser.SUPER_ADMIN ? "" : si.UserType == CUser.ADMIN ? " && ((W.UserType=" + CUser.DEALER + ") || (W.UserType=" + CUser.SUB_DEALER + "))" : si.UserType == CUser.DEALER ? " && (W.ParentUserId=@requestUserId || W.UserId=@requestUserId)" : " && (W.UserId=@requestUserId)" : " && (W.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (W.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (W.NoOfDigits=@noOfDigits) ";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        string sQuery = "(Select W.DayCode, W.PrizeId, W.NoOfDigits, W.Mask, Year(W.BillDate) As BillYear, Month(W.BillDate) As BillMonth, Day(W.BillDate) As BillDay, W.SerialNo, W.BillNo, W.UserName, W.TicketName, W.PrizeName, W.TicketNo, W.Count, W.PrizeAmount, W.Commission From Winners W Where (W.DayCode>=@startD && W.DayCode<=@endD) " + subQ + ") Union All (Select W.DayCode, 0 As PrizeId, 0 As NoOfDigits, '' As Mask, Year(W.DayCode) As BillYear, Month(W.DayCode) As BillMonth, Day(W.DayCode) As BillDay, 0 As SerialNo, 0 As BillNo, W.UserName, '' As TicketName, W.PrizeName, '' As TicketNo, Sum(W.Count) Count, Sum(W.PrizeAmount) As PrizeAmount, Sum(W.Commission) As Commission From Winners W Where (W.DayCode>=@startD && W.DayCode<=@endD) " + subQ + " Group By W.DayCode, W.UserName) Order By DayCode Desc, UserName, PrizeId, NoOfDigits Desc Limit @limitIndex,@limitSize";
                        //string sQuery = "(Select W.DayCode, W.PrizeId, W.NoOfDigits, W.Mask, Year(W.BillDate) As BillYear, Month(W.BillDate) As BillMonth, Day(W.BillDate) As BillDay, W.SerialNo, W.BillNo, W.UserName, W.TicketName, W.PrizeName, W.TicketNo, W.Count, W.PrizeAmount, W.Commission From Winners W Where (W.DayCode>=@startD && W.DayCode<=@endD) " + subQ + ") Union All (Select W.DayCode, 0 As PrizeId, 0 As NoOfDigits, '' As Mask, Year(W.BillDate) As BillYear, Month(W.BillDate) As BillMonth, Day(W.BillDate) As BillDay, 0 As SerialNo, 0 As BillNo, W.UserName, '' As TicketName, W.PrizeName, '' As TicketNo, Sum(W.Count) Count, Sum(W.PrizeAmount) As PrizeAmount, Sum(W.Commission) As Commission From Winners W Where (W.DayCode>=@startD && W.DayCode<=@endD) " + subQ + " Group By W.DayCode, BillYear, BillMonth, BillDay, W.UserName) Order By DayCode Desc, UserName, PrizeId, NoOfDigits Desc Limit @limitIndex,@limitSize";

                        ticketWReport = dataB.Database.SqlQuery<CRWinnersReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),                            
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = ticketWReport;
            return retObj;
        }


        public CTicketSalesTotal GetSalesReportTotal(CGetSalesReportParam param)
        {
            CTicketSalesTotal ticketSTotal = new CTicketSalesTotal();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                ticketSTotal.Success = false;
                ticketSTotal.Message = "Not Allowed";
                return ticketSTotal;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        //DayCodes                    
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        int SelectTable = -1;
                        SettingsService ss = new SettingsService();
                        long dayCode = ss.GetDayCode(param.CompanyId,param.RequestUserId);
                        if (endD >= dayCode && startD >= dayCode && dayCode != -1)
                        {
                            SelectTable = 0;
                        }
                        else if (endD >= dayCode || dayCode == -1)
                        {
                            SelectTable = 1;
                        }
                        else
                        {
                            SelectTable = 2;
                        }

                        string companyIdQuery = " && (TS.CompanyId=@companyId)";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (TS.ParentUserId = @userId)" : " && (TS.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (TS.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (TS.NoOfDigits=@noOfDigits) ";

                        string amountQ = si.UserType == CUser.DEALER || si.UserType == CUser.SUB_DEALER ? "(TS.Count*(TS.Cost-TS.DealerCommission))" : "(TS.Count*(TS.Cost-TS.CompanyCommission))";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        if (SelectTable == 0)
                        {
                            ticketSTotal = dataB.Database.SqlQuery<CTicketSalesTotal>("Select Ifnull(Sum(TS.Count),0) As Count, Ifnull(Sum(" + amountQ + "),0) As Amount From TicketSalesCaches TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),                        
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId)).FirstOrDefault();
                        }
                        else if (SelectTable == 1)
                        {
                            ticketSTotal = dataB.Database.SqlQuery<CTicketSalesTotal>("Select Sum(Count) As Count, Sum(Amount) As Amount From ((Select Ifnull(Sum(TS.Count),0) As Count, Ifnull(Sum(" + amountQ + "),0) As Amount From TicketSalesCaches TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + ") Union All (Select Sum(TS.Count), Sum(" + amountQ + ") As Amount From TicketSales TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ + ")) As T",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),                            
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId)).FirstOrDefault();
                        }
                        else
                        {
                            ticketSTotal = dataB.Database.SqlQuery<CTicketSalesTotal>("Select Ifnull(Sum(TS.Count),0) As Count, Ifnull(Sum(" + amountQ + "),0) As Amount From TicketSales TS Where(TS.DayCode >= @startD && TS.DayCode <=@endD) " + subQ,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),                            
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId)).FirstOrDefault();
                        }

                        ticketSTotal.Success = true;

                    }
                }
            }
            catch (Exception e)
            {
                ticketSTotal.Success = false;
                ticketSTotal.Message = e.Message;
            }
            return ticketSTotal;
        }

        public CWinnersTotal GetWinnersReportTotal(CGetWinnersReportParam param)
        {
            CWinnersTotal ticketWTotal = new CWinnersTotal();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                ticketWTotal.Success = false;
                ticketWTotal.Message = "Not Allowed";
                return ticketWTotal;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (W.CompanyId=@companyId) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (W.ParentUserId = @userId)" : " && (W.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (W.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (W.NoOfDigits=@noOfDigits) ";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        string qSelect = "Select Ifnull(Sum(W.Count),0) As Count, Ifnull(Sum(W.PrizeAmount),0) As PrizeAmount, Ifnull(Sum(W.Commission),0) As Commission ";
                        string qFrom = "From Winners W ";
                        string qWhere = "Where (W.DayCode>=@startD && W.DayCode<=@endD) ";

                        string sQuery = qSelect + qFrom + qWhere + subQ;

                        ticketWTotal = dataB.Database.SqlQuery<CWinnersTotal>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),                            
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).FirstOrDefault();

                        ticketWTotal.Success = true;
                    }
                }
            }
            catch (Exception e)
            {
                ticketWTotal.Success = false;
                ticketWTotal.Message = e.Message;
            }
            return ticketWTotal;
        }

        public CProfitnLossTotal GetProfitnLossReportTotal(CGetProfitnLossReportParam param)
        {
            CProfitnLossTotal pnlTotal = new CProfitnLossTotal();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId)
            {
                pnlTotal.Success = false;
                pnlTotal.Message = "Not Allowed";
                return pnlTotal;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (PNL.ParentUserId = @userId)" : " && (PNL.UserId=@userId)";
                        string subQ = companyIdQuery + userIdQuery;
                        string amountQ = si.UserType == CUser.DEALER || si.UserType == CUser.SUB_DEALER ? "(PNL.DealerSalesAmount)" : "(PNL.CompanySalesAmount)";
                        string qSelect = "Select Ifnull(Sum(" + amountQ + "),0) As SalesAmount, Ifnull(Sum(PNL.WinningAmount),0) As WinningAmount ";
                        string qFrom = "From ProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";

                        string sQuery = qSelect + qFrom + qWhere + subQ;

                        pnlTotal = dataB.Database.SqlQuery<CProfitnLossTotal>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),                            
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).FirstOrDefault();

                        pnlTotal.Success = true;
                    }
                }
            }
            catch (Exception e)
            {
                pnlTotal.Success = false;
                pnlTotal.Message = e.Message;
            }
            return pnlTotal;
        }

        public CProfitnLossTotal GetDealerNetPaymentReportTotal(CGetProfitnLossReportParam param)
        {
            CProfitnLossTotal pnlTotal = new CProfitnLossTotal();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId || si.UserType != CUser.DEALER)
            {
                pnlTotal.Success = false;
                pnlTotal.Message = "Not Allowed";
                return pnlTotal;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (PNL.ParentUserId = @userId)" : " && (PNL.UserId=@userId)";
                        string subQ = companyIdQuery + userIdQuery;
                        string amountQ = "(PNL.CompanySalesAmount)";
                        string qSelect = "Select Ifnull(Sum(" + amountQ + "),0) As SalesAmount, Ifnull(Sum(PNL.WinningAmount),0) As WinningAmount ";
                        string qFrom = "From ProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";

                        string sQuery = qSelect + qFrom + qWhere + subQ;

                        pnlTotal = dataB.Database.SqlQuery<CProfitnLossTotal>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),                            
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).FirstOrDefault();

                        pnlTotal.Success = true;
                    }
                }
            }
            catch (Exception e)
            {
                pnlTotal.Success = false;
                pnlTotal.Message = e.Message;
            }
            return pnlTotal;
        }


        public CRTicketCountReportReply GetTicketCountReport(CTicketCountParam param)
        {

            CRTicketCountReportReply retObj = new CRTicketCountReportReply();
            retObj.Success = true;

            List<CRTicketCountReport> ticketSReport = new List<CRTicketCountReport>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN && si.UserType != CUser.DEALER))
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                retObj.Report = ticketSReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.Year, param.Month, param.Day));

                        int SelectTable = -1;
                        SettingsService ss = new SettingsService();
                        long dayCode = ss.GetDayCode(param.CompanyId);
                        if (startD == dayCode || dayCode == -1)
                        {
                            SelectTable = 0;
                        }

                        string ticketNoQuery = param.TicketNo.Equals("") ? "" : " && (TC.TicketNo=@ticketNo) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (TC.ParentUserId = @userId)" : " && (TC.UserId=@userId)";
                        string companyIdQuery = " && (TC.CompanyId=@companyId) ";
                        string ticketIdQuery = param.TicketId == -1 ? "" : " && (TC.TicketId=@ticketId) ";

                        string subQ = companyIdQuery + ticketIdQuery + ticketNoQuery + userIdQuery;

                        if (SelectTable == 0)
                        {
                            
                            string havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (Count=@Count) " : param.Sign == 1 ? "Having (Count<@Count) " : "Having (Count>@Count) " : "";
                            ticketSReport = dataB.Database.SqlQuery<CRTicketCountReport>("Select  If((Select Count(Id) From BlockedTickets Where(CompanyId=TC.CompanyId && TicketId=TC.TicketId && TicketNo=TC.TicketNo))>0,0,1) As BlockStatus,TC.TicketId,TC.TicketNo,TC.TicketName,Sum(TC.Count) As Count From TicketSalesCaches TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.TicketId " + havingQuery + "   Order By Count Desc, TC.TicketName",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@Count", param.Count),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@ticketNo", param.TicketNo),
                            new MySqlParameter("@ticketId", param.TicketId)).ToList();

                            if(ticketSReport.Count <= 0)
                            {
                                havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (Count=@Count) " : param.Sign == 1 ? "Having (Count<@Count) " : "Having (Count>@Count) " : "";
                                ticketSReport = dataB.Database.SqlQuery<CRTicketCountReport>("Select  TC.TicketId,TC.TicketNo,TC.TicketName,Sum(TC.Count) As Count From TicketSales TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.TicketId " + havingQuery + " Order By Count Desc, TicketName",
                                    new MySqlParameter("@startD", startD),
                                    new MySqlParameter("@Count", param.Count),
                                    new MySqlParameter("@companyId", param.CompanyId),
                                    new MySqlParameter("@userId", param.UserId),
                                    new MySqlParameter("@ticketNo", param.TicketNo),
                                    new MySqlParameter("@ticketId", param.TicketId)).ToList();
                            }
                            

                        }
                        else
                        {
                            string havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (Count=@Count) " : param.Sign == 1 ? "Having (Count<@Count) " : "Having (Count>@Count) " : "";
                            ticketSReport = dataB.Database.SqlQuery<CRTicketCountReport>("Select  TC.TicketId,TC.TicketNo,TC.TicketName,Sum(TC.Count) As Count From TicketSales TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.TicketId " + havingQuery + " Order By Count Desc, TicketName",
                                new MySqlParameter("@startD", startD),
                                new MySqlParameter("@Count", param.Count),
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@userId", param.UserId),
                                new MySqlParameter("@ticketNo", param.TicketNo),
                                new MySqlParameter("@ticketId", param.TicketId)).ToList();
                        }
                        for (int i = 0; i < ticketSReport.Count; ++i)
                        {
                            ticketSReport.ElementAt(i).SerialNo = i + 1;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            retObj.Report = ticketSReport;
            return retObj;
        }

        public CRTicketCountGroupReportReply GetTicketCountGroupReport(CTicketCountGroupParam param)
        {
            CRTicketCountGroupReportReply retObj = new CRTicketCountGroupReportReply();
            retObj.Success = true;

            List<CRTicketCountGroupReport> ticketSReport = new List<CRTicketCountGroupReport>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN && si.UserType != CUser.DEALER))
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                retObj.Report = ticketSReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.Year, param.Month, param.Day));

                        int SelectTable = -1;
                        SettingsService ss = new SettingsService();
                        long dayCode = ss.GetDayCode(param.CompanyId);
                        if (startD == dayCode || dayCode == -1)
                        {
                            SelectTable = 0;
                        }

                        string ticketNoQuery = param.TicketNo.Equals("") ? "" : " && (TC.TicketNo=@ticketNo) ";
                        string userIdQuery = param.Select == CUser.ALL && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN) ? "" : param.Select == CUser.GROUP && (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN || si.UserType == CUser.DEALER) ? " && (TC.ParentUserId = @userId)" : " && (TC.UserId=@userId)";
                        string companyIdQuery = " && (TC.CompanyId=@companyId) ";
                        string noOfDigitsQuery = " && (TC.NoOfDigits=@noOfDigits) ";

                        string subQ = companyIdQuery + noOfDigitsQuery + ticketNoQuery + userIdQuery;

                        if (SelectTable == 0)
                        {
                            string havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (Count=@Count) " : param.Sign == 1 ? "Having (Count<@Count) " : "Having (Count>@Count) " : "";
                            ticketSReport = dataB.Database.SqlQuery<CRTicketCountGroupReport>("Select TC.TicketNo, Sum(TC.Count) As Count From TicketSalesCaches TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.DayCode, TC.TicketNo, TC.NoOfDigits " + havingQuery + " Order By TC.DayCode Desc, Count Desc",
                                new MySqlParameter("@startD", startD),
                                new MySqlParameter("@Count", param.Count),
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@userId", param.UserId),
                                new MySqlParameter("@ticketNo", param.TicketNo),
                                new MySqlParameter("@noOfDigits", param.NoOfDigits)).ToList();

                            if (ticketSReport.Count <= 0)
                            {
                                havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (Count=@Count) " : param.Sign == 1 ? "Having (Count<@Count) " : "Having (Count>@Count) " : "";
                                ticketSReport = dataB.Database.SqlQuery<CRTicketCountGroupReport>("Select  TC.TicketNo,Sum(TC.Count) As Count From TicketSales TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.NoOfDigits " + havingQuery + " Order By Count Desc",
                                    new MySqlParameter("@startD", startD),
                                    new MySqlParameter("@Count", param.Count),
                                    new MySqlParameter("@companyId", param.CompanyId),
                                    new MySqlParameter("@userId", param.UserId),
                                    new MySqlParameter("@ticketNo", param.TicketNo),
                                    new MySqlParameter("@noOfDigits", param.NoOfDigits)).ToList();
                            }

                        }
                        else
                        {
                            string havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (Count=@Count) " : param.Sign == 1 ? "Having (Count<@Count) " : "Having (Count>@Count) " : "";
                            ticketSReport = dataB.Database.SqlQuery<CRTicketCountGroupReport>("Select  TC.TicketNo,Sum(TC.Count) As Count From TicketSales TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.NoOfDigits " + havingQuery + " Order By Count Desc",
                                new MySqlParameter("@startD", startD),
                                new MySqlParameter("@Count", param.Count),
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@userId", param.UserId),
                                new MySqlParameter("@ticketNo", param.TicketNo),
                                new MySqlParameter("@noOfDigits", param.NoOfDigits)).ToList();
                        }
                        for (int i = 0; i < ticketSReport.Count; ++i)
                        {
                            ticketSReport.ElementAt(i).SerialNo = i + 1;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            retObj.Report = ticketSReport;
            return retObj;
        }

        public CRTicketCountBalanceReportReply GetTicketCountBalanceReport(CTicketCountParam param)
        {

            CRTicketCountBalanceReportReply retObj = new CRTicketCountBalanceReportReply();
            retObj.Success = true;

            List<CRTicketCountBalanceReport> ticketSReport = new List<CRTicketCountBalanceReport>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                retObj.Report = ticketSReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.Year, param.Month, param.Day));

                        int SelectTable = -1;
                        SettingsService ss = new SettingsService();
                        long dayCode = ss.GetDayCode(param.CompanyId);
                        if (startD == dayCode || dayCode == -1)
                        {
                            SelectTable = 0;
                        }

                        string ticketNoQuery = param.TicketNo.Equals("") ? "" : " && (TC.TicketNo=@ticketNo) ";
                        string companyIdQuery = " && (TC.CompanyId=@companyId) ";
                        string ticketIdQuery = param.TicketId == -1 ? "" : " && (TC.TicketId=@ticketId) ";

                        string subQ = companyIdQuery + ticketIdQuery + ticketNoQuery;

                        if (SelectTable == 0)
                        {

                            string havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (TC.InCount=@Count) " : param.Sign == 1 ? "Having (TC.InCount<@Count) " : "Having (TC.InCount>@Count) " : "";
                            ticketSReport = dataB.Database.SqlQuery<CRTicketCountBalanceReport>("Select TC.TicketId, TC.TicketNo, TC.TicketName, TC.InCount, TC.OutCount, (TC.InCount-TC.OutCount) As BalanceCount,If((Select Count(Id) From BlockedTickets Where(CompanyId=TC.CompanyId && TicketId=TC.TicketId && TicketNo=TC.TicketNo))>0,0,1) As BlockStatus From TicketCounts TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.DayCode, TC.TicketNo, TC.TicketId " + havingQuery + " Order By TC.DayCode Desc, BalanceCount Desc, TC.TicketName",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@Count", param.Count),
                            new MySqlParameter("@companyId", param.CompanyId),                            
                            new MySqlParameter("@ticketNo", param.TicketNo),
                            new MySqlParameter("@ticketId", param.TicketId)).ToList();

                            if (ticketSReport.Count <= 0)
                            {
                                havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (Count=@Count) " : param.Sign == 1 ? "Having (Count<@Count) " : "Having (Count>@Count) " : "";                                
                                ticketSReport = dataB.Database.SqlQuery<CRTicketCountBalanceReport>("Select  TSales.TicketId,TSales.TicketNo,TSales.TicketName,TSales.Count As InCount,TOuts.Count As OutCount,(Ifnull(TSales.Count, 0) - Ifnull(TOuts.Count, 0)) As BalanceCount From (Select TC.TicketId,TC.TicketNo,TC.TicketName,Sum(TC.Count) As Count From TicketSales TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.TicketId " + havingQuery + " ) TSales Left Join ( Select TC.TicketId,TC.TicketNo,Sum(TC.Count) As Count From OutDealerSales TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.TicketId " + havingQuery + " ) TOuts On TOuts.TicketId = TSales.TicketId And TOuts.TicketNo = TSales.TicketNo Order By BalanceCount Desc, TicketName",
                                    new MySqlParameter("@startD", startD),
                                    new MySqlParameter("@Count", param.Count),
                                    new MySqlParameter("@companyId", param.CompanyId),
                                    new MySqlParameter("@ticketNo", param.TicketNo),
                                    new MySqlParameter("@ticketId", param.TicketId)).ToList();
                            }


                        }
                        else
                        {
                            string havingQuery = param.Count > 0 ? param.Sign <= 0 ? "Having (Count=@Count) " : param.Sign == 1 ? "Having (Count<@Count) " : "Having (Count>@Count) " : "";                           
                            ticketSReport = dataB.Database.SqlQuery<CRTicketCountBalanceReport>("Select  TSales.TicketId,TSales.TicketNo,TSales.TicketName,TSales.Count As InCount,TOuts.Count As OutCount,(Ifnull(TSales.Count, 0) - Ifnull(TOuts.Count, 0)) As BalanceCount From (Select TC.TicketId,TC.TicketNo,TC.TicketName,Sum(TC.Count) As Count From TicketSales TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.TicketId " + havingQuery + " ) TSales Left Join ( Select TC.TicketId,TC.TicketNo,Sum(TC.Count) As Count From OutDealerSales TC Where(TC.DayCode = @startD) " + subQ + " Group By TC.TicketNo, TC.TicketId " + havingQuery + " ) TOuts On TOuts.TicketId = TSales.TicketId And TOuts.TicketNo = TSales.TicketNo Order By BalanceCount Desc, TicketName",
                                new MySqlParameter("@startD", startD),
                                new MySqlParameter("@Count", param.Count),
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@ticketNo", param.TicketNo),
                                new MySqlParameter("@ticketId", param.TicketId)).ToList();
                        }
                        for (int i = 0; i < ticketSReport.Count; ++i)
                        {
                            ticketSReport.ElementAt(i).SerialNo = i + 1;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            retObj.Report = ticketSReport;
            return retObj;
        }


        public CRUserCreateReply CreateUser(CRCreateUserparam param)
        {
            CRUserCreateReply retObj = new CRUserCreateReply();
            retObj.Success = true;

            long userId = -1;
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null && (si.UserType!=CUser.DEALER && si.UserType != CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.UserId = -1;
                return retObj;
            }

            try
            {
                if (IsUsernameAlreadyUsed(param.Username, param.CompanyId))
                {                    
                    retObj.Success = false;
                    retObj.Message = "Username already Exist";
                    retObj.UserId = -1;
                    return retObj;
                }

                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {

                            Account u = dataB.Accounts.Create();
                            u.CompanyId = param.CompanyId;
                            u.Name = param.Name;
                            u.Username = param.Username;
                            u.Password = param.Password;
                            u.UserType = si.UserType == CUser.DEALER ? CUser.SUB_DEALER : param.ParentUserId == -1 ? CUser.DEALER : CUser.SUB_DEALER;
                            u.ParentUserId = param.ParentUserId==-1?si.UserId: param.ParentUserId;
                            u.Permission = CUser.NO_PERMISSION;
                            u.TicketHoldingStatus = CUser.DISABLED;
                            u.Status = CUser.ACTIVATED;
                            dataB.Accounts.Add(u);

                            dataB.SaveChanges();
                            //Success
                            retObj.Success = true;
                            userId = u.Id;


                            //Default Commission Creation
                            var cr = dataB.UserTicketRates.Select(c => c).Where(x => x.CompanyId == param.CompanyId && x.UserId == userId);
                            var r = dataB.UserTicketRates.RemoveRange(cr);
                            
                            TicketRegisterService trs = new TicketRegisterService();
                            List<CTicket> tkts = trs.ReadAllTickets(param.CompanyId);                                                     
                            int i = 0;
                            foreach (var t in tkts)
                            {
                                var dt = dataB.UserTicketRates.Create();
                                dt.SerialNo = ++i;
                                dt.Commission = 0;
                                dt.UserId = userId;
                                dt.TicketId = t.TicketId;
                                dt.CompanyId = param.CompanyId;

                                dt.Mask = t.Mask;
                                dt.NoOfDigits = t.NoOfDigits;
                                dt.TicketName = t.Name;
                                dt.TicketType = t.Type;
                                dt.UserName = u.Name;
                                dt.UserType = u.UserType;
                                dataB.UserTicketRates.Add(dt);
                            }

                            dataB.SaveChanges();
                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;                            

                            dataBTransaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.UserId = userId;
            return retObj;
        }

        public CRUserDeleteReply DeleteUser(CRDeleteUserparam param)
        {
            CRUserDeleteReply retObj = new CRUserDeleteReply();
            retObj.Success = true;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null && (si.UserType != CUser.DEALER && si.UserType != CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";                
                return retObj;
            }

            try
            {
                if (IsUserRemovable(param.UserId) != true)
                {
                    retObj.Success = false;
                    retObj.Message = "Permission Denied To Remove this User";
                    return retObj;
                }

                if (IsUserHasChild(param.UserId) == true)
                {
                    retObj.Success = false;
                    retObj.Message = "Please Remove the Sub Dealers Under this User, First";
                    return retObj;
                }

                if (IsUserUsedInTransaction(param.UserId) == true)
                {
                    retObj.Success = false;
                    retObj.Message = "Please Remove User from Transactions and other Registers, First";
                    return retObj;
                }

                
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            if (si.UserType == CUser.DEALER)
                            {
                                var cr = dataB.Accounts.Select(c => c).Where(x => x.Id == param.UserId && x.ParentUserId==si.UserId && x.CompanyId == param.CompanyId);
                                var r = dataB.Accounts.RemoveRange(cr);
                                if (r.Count() > 0)
                                {
                                    var crr = dataB.UserTicketRates.Select(c => c).Where(x => x.UserId == param.UserId && x.CompanyId == param.CompanyId);
                                    var rr = dataB.UserTicketRates.RemoveRange(crr);

                                    dataB.SaveChanges();
                                    dataBTransaction.Commit();

                                }
                                else
                                {
                                    retObj.Success = false;
                                    retObj.Message = "User Doesnt Exist";
                                }
                            }
                            else
                            {
                                var cr = dataB.Accounts.Select(c => c).Where(x => x.Id == param.UserId && x.CompanyId == param.CompanyId);
                                var r = dataB.Accounts.RemoveRange(cr);
                                if (r.Count() > 0)
                                {
                                    var crr = dataB.UserTicketRates.Select(c => c).Where(x => x.UserId == param.UserId && x.CompanyId == param.CompanyId);
                                    var rr = dataB.UserTicketRates.RemoveRange(crr);

                                    dataB.SaveChanges();
                                    dataBTransaction.Commit();

                                }
                                else
                                {
                                    retObj.Success = false;
                                    retObj.Message = "User Doesnt Exist";
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;
                            dataBTransaction.Rollback();
                        }

                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            
            return retObj;
        }

        public CRUserUpdateReply UpdateUser(CRUpdateUserparam param)
        {
            CRUserUpdateReply retObj = new CRUserUpdateReply();
            retObj.Success = true;
            
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null && (si.UserType != CUser.DEALER && si.UserType != CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";                
                return retObj;
            }

            try
            {
                if (IsUsernameAlreadyUsed(param.UserId, param.Username, param.CompanyId))
                {
                    retObj.Success = false;
                    retObj.Message = "Username already Exist";                    
                    return retObj;
                }

                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            Account u = null;
                            if (si.UserType == CUser.DEALER)
                            {
                                u=dataB.Accounts.Select(c => c).Where(x => x.Id == param.UserId && x.ParentUserId==si.UserId && x.CompanyId == param.CompanyId).FirstOrDefault();
                            }
                            else
                            {
                                u=dataB.Accounts.Select(c => c).Where(x => x.Id == param.UserId && x.CompanyId == param.CompanyId).FirstOrDefault();
                            }

                            u.Name = param.Name;
                            u.Username = param.Username;
                            u.Password = param.Password;
                            
                            dataB.SaveChanges();

                            dataB.Database.ExecuteSqlCommand("Update TicketSales Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));

                            /*
                             *TicketSalesCaches Trigger Happens to run,So should be careful with updating
                             *UPDATE:- updating trigger is removed as there is no updates for number or count is allowed
                             */ /*
                            dataB.Database.ExecuteSqlCommand("Update TicketSalesCaches Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));
                           
                            dataB.Database.ExecuteSqlCommand("Update ProfitnLosses Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));

                            dataB.Database.ExecuteSqlCommand("Update Winners Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));

                            dataB.Database.ExecuteSqlCommand("Update UserTicketSettings Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));

                            dataB.Database.ExecuteSqlCommand("Update AccountEntries Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));

                            dataB.Database.ExecuteSqlCommand("Update DealerHoldingLimits Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));

                            dataB.Database.ExecuteSqlCommand("Update DealerTicketCounts Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));

                            dataB.Database.ExecuteSqlCommand("Update DealerTicketLimits Set UserName=@userName Where (CompanyId=@companyId && UserId=@userId)",
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@userName", param.Name),
                            new MySqlParameter("@companyId", param.CompanyId));

                            //Success
                            retObj.Success = true;

                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;                            

                            dataBTransaction.Rollback();
                        }
                        finally
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }            
            return retObj;
        }

        public CRUserTicketCommissionReply GetUserTicketCommission(CRGetUserTicketCommissionparam param)
        {
            CRUserTicketCommissionReply retObj = new CRUserTicketCommissionReply();
            retObj.Success = true;

            List<CRUserTicketCommissionReport> report = new List<CRUserTicketCommissionReport>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN && si.UserType != CUser.DEALER))
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                retObj.Report = report;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {

                        if (si.UserType == CUser.SUPER_ADMIN || si.UserType == CUser.ADMIN)
                        {
                            report = dataB.Database.SqlQuery<CRUserTicketCommissionReport>("Select UT.TicketId, UT.TicketName, UT.NoOfDigits, UT.Commission From UserTicketSettings UT Where(UT.UserId=@userId && UT.CompanyId=@companyId) Order By UT.SerialNo ",
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@userId", param.UserId)).ToList();
                        }else if (si.UserType == CUser.DEALER)
                        {
                            report = dataB.Database.SqlQuery<CRUserTicketCommissionReport>("Select UT.TicketId, UT.TicketName, UT.NoOfDigits, UT.Commission From UserTicketSettings UT Where(UT.UserId=@userId && UT.UserType=@userType && UT.CompanyId=@companyId) Order By UT.SerialNo ",
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@userId", param.UserId),
                                new MySqlParameter("@userType", CUser.SUB_DEALER)).ToList();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            retObj.Report = report;
            return retObj;
        }

        public CRUpdateUserTicketCommissionReply UpdateUserTicketCommission(CUpdateUserTicketCommissionParam param)
        {
            CRUpdateUserTicketCommissionReply retObj = new CRUpdateUserTicketCommissionReply();
            retObj.Success = true;            

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN && si.UserType != CUser.DEALER))
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;                
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            var cr = dataB.UserTicketRates.Select(c => c).Where(x => x.CompanyId == param.CompanyId && x.UserId == param.UserId);
                            var r = dataB.UserTicketRates.RemoveRange(cr);

                            TicketRegisterService trs = new TicketRegisterService();                            
                            UserRegisterService urs = new UserRegisterService();
                            CUser u = null;
                            if (si.UserType == CUser.DEALER)
                            {
                                //For dealer check if the sub dealer commission is less than or equal to dealer commission
                                u=urs.ReadUser(param.UserId,si.UserId,param.CompanyId);

                                int i = 0;
                                foreach (var com in param.Commissions)
                                {
                                    CTicket t = trs.ReadTicket(com.TicketId, param.CompanyId);

                                    UserTicketSettings dealerCom= dataB.UserTicketRates.Where(e => e.CompanyId == param.CompanyId && e.UserId==si.UserId && e.TicketId==t.TicketId).FirstOrDefault();
                                    if (dealerCom == null)
                                    {
                                        retObj.Message = "Dealer Commission Need to be Set first";
                                        retObj.Success = false;
                                        return retObj;
                                    }
                                    
                                    var dt = dataB.UserTicketRates.Create();
                                    dt.SerialNo = ++i;
                                    dt.Commission = com.Commission>dealerCom.Commission?dealerCom.Commission:com.Commission;
                                    dt.UserId = param.UserId;
                                    dt.TicketId = t.TicketId;
                                    dt.CompanyId = param.CompanyId;

                                    dt.Mask = t.Mask;
                                    dt.NoOfDigits = t.NoOfDigits;
                                    dt.TicketName = t.Name;
                                    dt.TicketType = t.Type;
                                    dt.UserName = u.Name;
                                    dt.UserType = u.UserType;
                                    dataB.UserTicketRates.Add(dt);
                                }

                                dataB.SaveChanges();
                                dataBTransaction.Commit();
                            }
                            else
                            {
                                u=urs.ReadUser(param.UserId);

                                int i = 0;
                                foreach (var com in param.Commissions)
                                {
                                    CTicket t = trs.ReadTicket(com.TicketId, param.CompanyId);
                                    var dt = dataB.UserTicketRates.Create();
                                    dt.SerialNo = ++i;
                                    dt.Commission = com.Commission;
                                    dt.UserId = param.UserId;
                                    dt.TicketId = t.TicketId;
                                    dt.CompanyId = param.CompanyId;

                                    dt.Mask = t.Mask;
                                    dt.NoOfDigits = t.NoOfDigits;
                                    dt.TicketName = t.Name;
                                    dt.TicketType = t.Type;
                                    dt.UserName = u.Name;
                                    dt.UserType = u.UserType;
                                    dataB.UserTicketRates.Add(dt);
                                }

                                dataB.SaveChanges();
                                dataBTransaction.Commit();
                            }

                            
                        }catch(Exception ex)
                        {
                            dataBTransaction.Rollback();
                            retObj.Message = ex.Message;
                            retObj.Success = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }
            
            return retObj;
        }

        public CRGetResultsReply GetResults(CRGetResultsparam param)
        {
            CRGetResultsReply retObj = new CRGetResultsReply();
            retObj.Success = true;

            List<CRGetResultReport> report = new List<CRGetResultReport>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                retObj.Report = report;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    SettingsService ss = new SettingsService();
                    DateTime dDate = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day);
                    long dayCode = long.Parse(string.Format("{0}{1:00}{2:00}", dDate.Year, dDate.Month, dDate.Day));

                    using (var dataB = new AquaStorage())
                    {

                        report = dataB.Database.SqlQuery<CRGetResultReport>("Select PW.PrizeId, PW.Number As TicketNo, PW.SerialNo From PrizeWins PW Where(PW.DayCode=@dayCode && PW.CompanyId=@companyId) Order By PW.SerialNo ",
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@dayCode", dayCode)).ToList();
                        if (report.Count <= 0)
                        {
                            //Load Default
                            PrizeWinRegisterService pws = new PrizeWinRegisterService();
                            pws.LoadDefaultPrizeWins(dDate, param.CompanyId);

                            //Read
                            report = dataB.Database.SqlQuery<CRGetResultReport>("Select PW.PrizeId, PW.Number As TicketNo, PW.SerialNo From PrizeWins PW Where(PW.DayCode=@dayCode && PW.CompanyId=@companyId) Order By PW.SerialNo ",
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@dayCode", dayCode)).ToList();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            retObj.Report = report;
            return retObj;
        }

        public CRGetResultsReply GetSummaryResults(CRGetSummaryResultsparam param)
        {
            CRGetResultsReply retObj = new CRGetResultsReply();
            retObj.Success = true;

            List<CRGetResultReport> report = new List<CRGetResultReport>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null )
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                retObj.Report = report;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    long dayCode = long.Parse(string.Format("{0}{1:00}{2:00}", param.Year, param.Month, param.Day));

                    using (var dataB = new AquaStorage())
                    {

                        report = dataB.Database.SqlQuery<CRGetResultReport>("Select PW.PrizeId, PW.Number As TicketNo, PW.SerialNo From PrizeWins PW Where(PW.DayCode=@dayCode && PW.CompanyId=@companyId) Order By PW.SerialNo ",
                                new MySqlParameter("@companyId", param.CompanyId),
                                new MySqlParameter("@dayCode", dayCode)).ToList();                        
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            retObj.Report = report;
            return retObj;
        }

        public CRUpdateResultsReply UpdateResults(CUpdateResultsParam param)
        {
            CRUpdateResultsReply retObj = new CRUpdateResultsReply();
            retObj.Success = true;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    SettingsService ss = new SettingsService();
                    DateTime dDate = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day);
                    long dayCode = long.Parse(string.Format("{0}{1:00}{2:00}", dDate.Year, dDate.Month, dDate.Day));

                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {                            
                            foreach (var com in param.Results)
                            {
                                PrizeWin pw= dataB.PrizeWins.Select(c => c).Where(x => x.CompanyId == param.CompanyId && x.DayCode == dayCode && x.PrizeId==com.PrizeId).FirstOrDefault();
                                pw.Number = com.TicketNo;

                                dataB.SaveChanges();
                            }
                            
                            dataBTransaction.Commit();

                            //Convert To end Todays Sales
                            TicketSalesService tss = new TicketSalesService();                            
                            
                            tss.ConvertTicketSalesCacheToTicketSales(dDate, param.CompanyId, param.SessionId);                            
                            tss.RemoveTicketCounts(dDate, param.CompanyId, param.SessionId);
                            tss.RemoveBlockedTickets(dDate, param.CompanyId, param.SessionId);
                            tss.GenerateWinners(dDate, param.CompanyId, param.SessionId);
                            tss.GenerateProfitnLoss(dDate, param.CompanyId, param.SessionId);
                            tss.GenerateAccounts(dDate, param.CompanyId, param.SessionId);                            
                            tss.GenerateProfitReport(dDate, param.CompanyId, param.SessionId);


                        }
                        catch (Exception ex)
                        {
                            dataBTransaction.Rollback();
                            retObj.Message = ex.Message;
                            retObj.Success = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            return retObj;
        }

        public CRSendToOutDealerReply SendToOutDealer(CSendToOutDealerParam param)
        {
            CRSendToOutDealerReply retObj = new CRSendToOutDealerReply();
            retObj.Success = true;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    OutDealerSalesService odss = new OutDealerSalesService();
                    CSendToOutDealerMessage csm= odss.SendToOutDealer(param.OutDealerId, param.CompanyId);
                    if (!csm.IsSuccess)
                    {
                        retObj.Success = false;
                        retObj.Message = csm.Message + " : " + csm.OutDealerMessage + " : " + csm.OutDealerSalesMessage;
                    }
                    else
                    {
                        retObj.Message = csm.Message + " : " + csm.OutDealerMessage + " : " + csm.OutDealerSalesMessage;
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            return retObj;
        }


        public CRTicketSalesReportReply GetOutDealerSalesReport(CGetSalesReportParam param)
        {
            CRTicketSalesReportReply retObj = new CRTicketSalesReportReply();
            retObj.Success = true;

            List<CRTicketSalesReport> ticketSReport = new List<CRTicketSalesReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = ticketSReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        //DayCodes                    
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));
                                                
                        string companyIdQuery = " && (OS.CompanyId=@companyId)";
                        string userIdQuery = param.UserId == -1 ? "" : " && (OS.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (OS.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (OS.NoOfDigits=@noOfDigits) ";

                        string amountQ = "(OS.Count*OS.Amount)";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        ticketSReport = dataB.Database.SqlQuery<CRTicketSalesReport>("(Select OS.DayCode, OS.UserType, OS.Id As BillId, Year(OS.BillDate) As BillYear, Month(OS.BillDate) As BillMonth, Day(OS.BillDate) As BillDay, OS.BillNo, OS.SerialNo, OS.UserId, OS.UserName, OS.TicketName, OS.TicketNo, OS.Count, " + amountQ + " As Amount From OutDealerSales OS Where(OS.DayCode >= @startD && OS.DayCode <=@endD) " + subQ + ") Union All (Select OS.DayCode, OS.UserType, 0 As BillId, Year(OS.BillDate) As BillYear, Month(OS.BillDate) As BillMonth, Day(OS.BillDate) As BillDay, OS.BillNo, 0 As SerialNo, OS.UserId, OS.UserName, '' As TicketName, '' As TicketNo, Sum(OS.Count) As Count, Sum(" + amountQ + ") As Amount From OutDealerSales OS Where(OS.DayCode >= @startD && OS.DayCode <=@endD) " + subQ + " Group By OS.DayCode, OS.UserType, BillYear, BillMonth, BillDay, OS.BillNo, OS.UserId, OS.UserName) Order By DayCode Desc,UserType,UserName,BillNo Desc,SerialNo Limit @limitIndex,@limitSize",
                        new MySqlParameter("@startD", startD),
                        new MySqlParameter("@endD", endD),
                        new MySqlParameter("@companyId", param.CompanyId),
                        new MySqlParameter("@userId", param.UserId),
                        new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                        new MySqlParameter("@ticketId", param.TicketId),
                        new MySqlParameter("@limitIndex", param.LimitIndex),
                        new MySqlParameter("@limitSize", param.LimitSize)).ToList();

                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = ticketSReport;
            return retObj;
        }

        public CTicketSalesTotal GetOutDealerSalesReportTotal(CGetSalesReportParam param)
        {
            CTicketSalesTotal ticketSTotal = new CTicketSalesTotal();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                ticketSTotal.Success = false;
                ticketSTotal.Message = "Not Allowed";
                return ticketSTotal;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        //DayCodes                    
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (OS.CompanyId=@companyId)";
                        string userIdQuery = param.UserId == -1 ? "" : " && (OS.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (OS.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (OS.NoOfDigits=@noOfDigits) ";

                        string amountQ = "(OS.Count*OS.Amount)";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;


                        ticketSTotal = dataB.Database.SqlQuery<CTicketSalesTotal>("Select Ifnull(Sum(OS.Count),0) As Count, Ifnull(Sum(" + amountQ + "),0) As Amount From OutDealerSales OS Where(OS.DayCode >= @startD && OS.DayCode <=@endD) " + subQ,
                        new MySqlParameter("@startD", startD),
                        new MySqlParameter("@endD", endD),
                        new MySqlParameter("@companyId", param.CompanyId),
                        new MySqlParameter("@userId", param.UserId),
                        new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                        new MySqlParameter("@ticketId", param.TicketId)).FirstOrDefault();


                        ticketSTotal.Success = true;

                    }
                }
            }
            catch (Exception e)
            {
                ticketSTotal.Success = false;
                ticketSTotal.Message = e.Message;
            }
            return ticketSTotal;
        }

        public CRWinnersReportReply GetOutDealerWinnersReport(CGetWinnersReportParam param)
        {
            CRWinnersReportReply retObj = new CRWinnersReportReply();
            retObj.Success = true;

            List<CRWinnersReport> ticketWReport = new List<CRWinnersReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = ticketWReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (W.CompanyId=@companyId) ";
                        string userIdQuery = param.UserId == -1 ? "" : " && (W.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (W.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (W.NoOfDigits=@noOfDigits) ";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        string sQuery = "(Select W.DayCode, W.PrizeId, W.NoOfDigits, W.Mask, Year(W.BillDate) As BillYear, Month(W.BillDate) As BillMonth, Day(W.BillDate) As BillDay, W.SerialNo, W.BillNo, W.UserName, W.TicketName, W.PrizeName, W.TicketNo, W.Count, W.PrizeAmount, W.Commission From OutDealerWinners W Where (W.DayCode>=@startD && W.DayCode<=@endD) " + subQ + ") Union All (Select W.DayCode, 0 As PrizeId, 0 As NoOfDigits, '' As Mask, Year(W.BillDate) As BillYear, Month(W.BillDate) As BillMonth, Day(W.BillDate) As BillDay, 0 As SerialNo, 0 As BillNo, W.UserName, '' As TicketName, W.PrizeName, '' As TicketNo, Sum(W.Count) Count, Sum(W.PrizeAmount) As PrizeAmount, Sum(W.Commission) As Commission From OutDealerWinners W Where (W.DayCode>=@startD && W.DayCode<=@endD) " + subQ + " Group By W.DayCode, BillYear, BillMonth, BillDay, W.UserName) Order By DayCode Desc, UserName, PrizeId, NoOfDigits Desc Limit @limitIndex,@limitSize";

                        ticketWReport = dataB.Database.SqlQuery<CRWinnersReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = ticketWReport;
            return retObj;
        }

        public CWinnersTotal GetOutDealerWinnersReportTotal(CGetWinnersReportParam param)
        {
            CWinnersTotal ticketWTotal = new CWinnersTotal();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                ticketWTotal.Success = false;
                ticketWTotal.Message = "Not Allowed";
                return ticketWTotal;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (W.CompanyId=@companyId) ";
                        string userIdQuery = param.UserId == -1 ? "" : " && (W.UserId=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (W.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (W.NoOfDigits=@noOfDigits) ";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        string qSelect = "Select Ifnull(Sum(W.Count),0) As Count, Ifnull(Sum(W.PrizeAmount),0) As PrizeAmount, Ifnull(Sum(W.Commission),0) As Commission ";
                        string qFrom = "From OutDealerWinners W ";
                        string qWhere = "Where (W.DayCode>=@startD && W.DayCode<=@endD) ";

                        string sQuery = qSelect + qFrom + qWhere + subQ;

                        ticketWTotal = dataB.Database.SqlQuery<CWinnersTotal>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).FirstOrDefault();

                        ticketWTotal.Success = true;
                    }
                }
            }
            catch (Exception e)
            {
                ticketWTotal.Success = false;
                ticketWTotal.Message = e.Message;
            }
            return ticketWTotal;
        }

        public CRProfitnLossReportReply GetOutDealerProfitnLossReport(CGetProfitnLossReportParam param)
        {
            CRProfitnLossReportReply retObj = new CRProfitnLossReportReply();
            retObj.Success = true;

            List<CRProfitnLossReport> pnlReport = new List<CRProfitnLossReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = pnlReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.UserId == -1 ? "" : " && (PNL.UserId=@userId)";
                        string subQ = companyIdQuery + userIdQuery;                        
                        string qSelect = "Select PNL.DayCode, PNL.UserType, PNL.UserName, PNL.SalesAmount, PNL.WinningAmount ";
                        string qFrom = "From OutDealerProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";
                        string qOrder = " Group by PNL.UserId Order By PNL.DayCode Desc, PNL.UserType";                        
                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrder;

                        pnlReport = dataB.Database.SqlQuery<CRProfitnLossReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = pnlReport;
            return retObj;
        }

        public CRProfitnLossReportReply GetOutDealerProfitnLossBriefReport(CGetProfitnLossReportParam param)
        {
            CRProfitnLossReportReply retObj = new CRProfitnLossReportReply();
            retObj.Success = true;

            List<CRProfitnLossReport> pnlReport = new List<CRProfitnLossReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = pnlReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.UserId == -1 ? "" : " && (PNL.UserId=@userId)";
                        string subQ = companyIdQuery + userIdQuery;
                        string qSelect = "Select PNL.UserType, PNL.UserName, Sum(PNL.SalesAmount) As SalesAmount, Sum(PNL.WinningAmount) As WinningAmount ";
                        string qFrom = "From OutDealerProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";                        
                        string qOrder = " Group by PNL.UserId Order By PNL.UserType";

                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrder;

                        pnlReport = dataB.Database.SqlQuery<CRProfitnLossReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = pnlReport;
            return retObj;
        }

        public CProfitnLossTotal GetOutDealerProfitnLossReportTotal(CGetProfitnLossReportParam param)
        {
            CProfitnLossTotal pnlTotal = new CProfitnLossTotal();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                pnlTotal.Success = false;
                pnlTotal.Message = "Not Allowed";
                return pnlTotal;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PNL.CompanyId=@companyId) ";
                        string userIdQuery = param.UserId == -1 ? "" : " && (PNL.UserId=@userId)";
                        string subQ = companyIdQuery + userIdQuery;
                        string qSelect = "Select Ifnull(Sum(PNL.SalesAmount),0) As SalesAmount, Ifnull(Sum(PNL.WinningAmount),0) As WinningAmount ";
                        string qFrom = "From OutDealerProfitnLosses PNL ";
                        string qWhere = "Where  (PNL.DayCode>=@startD && PNL.DayCode<=@endD) ";

                        string sQuery = qSelect + qFrom + qWhere + subQ;

                        pnlTotal = dataB.Database.SqlQuery<CProfitnLossTotal>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@limitIndex", param.LimitIndex),
                            new MySqlParameter("@limitSize", param.LimitSize)).FirstOrDefault();

                        pnlTotal.Success = true;
                    }
                }
            }
            catch (Exception e)
            {
                pnlTotal.Success = false;
                pnlTotal.Message = e.Message;
            }
            return pnlTotal;
        }

        public CRProfitReportReply GetProfitReport(CGetProfitReportParam param)
        {
            CRProfitReportReply retObj = new CRProfitReportReply();
            retObj.Success = true;

            List<CRProfitReport> pReport = new List<CRProfitReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = pReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (PR.CompanyId=@companyId) ";
                        string subQ = companyIdQuery;
                        string qSelect = "Select PR.DayCode, PR.SalesAmount, PR.SalesWinningAmount, PR.OutDealerSalesAmount, PR.OutDealerWinningAmount ";
                        string qFrom = "From ProfitReports PR ";
                        string qWhere = "Where  (PR.DayCode>=@startD && PR.DayCode<=@endD) ";
                        string qOrder = " Order By PR.DayCode Desc";

                        string sQuery = qSelect + qFrom + qWhere + subQ + qOrder;

                        pReport = dataB.Database.SqlQuery<CRProfitReport>(sQuery,
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = pReport;
            return retObj;
        }


        public CRBlockReply BlockTicket(CBlockParam param)
        {
            CRBlockReply retObj = new CRBlockReply();
            retObj.Success = false;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Message = "Not Allowed";
                return retObj;
            }

            try
            {
                long sendDayCode = long.Parse(string.Format("{0}{1:00}{2:00}", param.Year, param.Month, param.Day));
                SettingsService ss = new SettingsService();
                long dayCode = ss.GetDayCode(param.CompanyId);
                if (dayCode == -1 || dayCode!= sendDayCode)
                {
                    retObj.Message = "Not Allowed";
                    return retObj;
                }

                //lock (Synchronizer.@lock)
                {

                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            BlockedTicket blno =dataB.BlockedTickets.Where(e => e.CompanyId == param.CompanyId && e.TicketId == param.TicketId && e.TicketNo == param.TicketNo).FirstOrDefault();
                            if (blno == null)
                            {
                                BlockedTicket bt = dataB.BlockedTickets.Create();
                                bt.CompanyId = param.CompanyId;
                                bt.TicketId = param.TicketId;
                                bt.TicketNo = param.TicketNo;
                                dataB.BlockedTickets.Add(bt);

                                dataB.SaveChanges();
                                dataBTransaction.Commit();
                            }
                            //Success
                            retObj.Success = true;
                                                        
                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;

                            dataBTransaction.Rollback();
                        }

                    }
                }
            }
            catch (Exception e) { retObj.Message = e.Message; }

            return retObj;
        }

        public CRBlockReply UnBlockTicket(CBlockParam param)
        {
            CRBlockReply retObj = new CRBlockReply();
            retObj.Success = false;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Message = "Not Allowed";
                return retObj;
            }

            try
            {
                long sendDayCode = long.Parse(string.Format("{0}{1:00}{2:00}", param.Year, param.Month, param.Day));
                SettingsService ss = new SettingsService();
                long dayCode = ss.GetDayCode(param.CompanyId);
                if (dayCode == -1 || dayCode != sendDayCode)
                {
                    retObj.Message = "Not Allowed";
                    return retObj;
                }

                //lock (Synchronizer.@lock)
                {

                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            var cr = dataB.BlockedTickets.Select(c => c).Where(x => x.CompanyId == param.CompanyId && x.TicketId == param.TicketId && x.TicketNo == param.TicketNo);
                            var r = dataB.BlockedTickets.RemoveRange(cr);

                            dataB.SaveChanges();

                            //Success
                            retObj.Success = true;

                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;

                            dataBTransaction.Rollback();
                        }

                    }
                }
            }
            catch (Exception e) { retObj.Message = e.Message; }

            return retObj;
        }


        public List<CTicketLimit> GetTicketlimits(CRGetLimitsParam param)
        {
            List<CTicketLimit> limits = new List<CTicketLimit>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType !=CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                return limits;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {

                            var datas = dataB.TicketLimits.Select(e => e).Where(e => ( e.CompanyId == param.CompanyId)).OrderBy(e=>e.SerialNo);

                            if (datas != null)
                            {
                                foreach (var i in datas)
                                {
                                    CTicketLimit ctl = new CTicketLimit();
                                    ctl.Id = i.Id;
                                    ctl.TicketId = i.TicketId;
                                    ctl.NoOfDigits = i.NoOfDigits;
                                    ctl.TicketName = i.TicketName;
                                    ctl.Number = i.Number;
                                    ctl.Count = i.Limit;
                                    ctl.SerialNo = i.SerialNo;
                                    limits.Add(ctl);
                                }
                            }

                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return limits;
        }

        public CRLimitCreateReply CreateTicketLimit(CRCreateLimitParam param)
        {
            CRLimitCreateReply retObj = new CRLimitCreateReply();
            retObj.Success = true;

            long Id = -1;
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null && (si.UserType != CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Id = -1;
                return retObj;
            }

            if (param.Number.Equals("All"))
            {
                retObj.Success = false;
                retObj.Message = "'All' Not Allowed to Create";
                retObj.Id = -1;
                return retObj;
            }

            try
            {
                TicketRegisterService trs = new TicketRegisterService();
                CTicket ticket=trs.ReadTicket(param.TicketId, param.CompanyId);
                
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            int slNo=dataB.TicketLimits.Count();
                            TicketLimit tl = dataB.TicketLimits.Create();
                            tl.CompanyId = param.CompanyId;
                            tl.TicketId = ticket.TicketId;
                            tl.TicketMask = ticket.Mask;
                            tl.TicketName = ticket.Name;
                            tl.NoOfDigits = ticket.NoOfDigits;
                            tl.TicketType = ticket.Type;
                            tl.Number = param.Number;
                            tl.Limit = param.Count;
                            tl.SerialNo = slNo+1;
                            dataB.TicketLimits.Add(tl);

                            dataB.SaveChanges();
                            //Success
                            retObj.Success = true;
                            Id = tl.Id;

                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;

                            dataBTransaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Id = Id;
            return retObj;
        }

        public CRLimitDeleteReply DeleteTicketLimit(CRDeleteLimitParam param)
        {
            CRLimitDeleteReply retObj = new CRLimitDeleteReply();
            retObj.Success = true;            

            try
            {
                TicketLimitRegisterService tlrs = new TicketLimitRegisterService();
                CBoolMessage cbm= tlrs.DeleteTicketLimit(param.Id, param.SerialNo, param.CompanyId, param.SessionId);
                retObj.Success = cbm.IsSuccess;
                retObj.Message = cbm.Message;
                
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }

            return retObj;
        }

        public CRLimitUpdateReply UpdateTicketLimit(CRUpdateLimitParam param)
        {
            CRLimitUpdateReply retObj = new CRLimitUpdateReply();
            retObj.Success = true;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Message = "Session Error";
                retObj.Success = false;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            TicketLimit u = dataB.TicketLimits.Select(c => c).Where(x => x.Id == param.Id).FirstOrDefault();
                            u.Limit = param.Count;
                            u.Number = param.Number;

                            dataB.SaveChanges();
                            //Success
                            retObj.Success = true;

                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;

                            dataBTransaction.Rollback();
                        }
                        finally
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            return retObj;
        }



        public List<COutDealerTicketFilter> GetOutDealerTicketFilters(CRGetOutDealerTicketFiltersParam param)
        {
            List<COutDealerTicketFilter> filters = new List<COutDealerTicketFilter>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                return filters;
            }
            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        try
                        {

                            filters = dataB.OutDealerTicketFilters.Select(c => new COutDealerTicketFilter() { Id = c.Id, CompanyId = c.CompanyId, SerialNo = c.SerialNo, Count = c.Count, Number = c.Number, UserId = c.UserId, Condition = c.Condition, InTicketIds = c.InTicketIds, InTicketNames = c.InTicketNames, NoOfDigits = c.NoOfDigits, OutTicketId = c.OutTicketId, OutTicketName = c.OutTicketName, UserName = c.UserName, UserType = c.UserType }).Where(e => e.CompanyId == param.CompanyId && e.UserId == param.OutdealerId).OrderBy(e => e.SerialNo).ToList();
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch
            {

            }

            return filters;
        }

        public CRFilterCreateReply CreateOutDealerTicketFilter(CRCreateOutDealerTicketFilterParam param)
        {
            CRFilterCreateReply retObj = new CRFilterCreateReply();
            retObj.Success = true;

            long Id = -1;
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null && (si.UserType != CUser.ADMIN && si.UserType != CUser.SUPER_ADMIN))
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Id = -1;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {

                            int slNo = dataB.OutDealerTicketFilters.Where(e => e.UserId == param.UserId).Count();
                            OutDealerTicketFilter odtf = dataB.OutDealerTicketFilters.Create();
                            odtf.SerialNo = slNo+1;
                            odtf.NoOfDigits = param.NoOfDigits;
                            odtf.InTicketIds = param.InTicketIds;
                            odtf.OutTicketId = param.OutTicketId;
                            odtf.InTicketNames = param.InTicketNames;
                            odtf.OutTicketName = param.OutTicketName;
                            odtf.Number = param.Number.ToUpper().Trim();
                            odtf.Condition = param.Condition;
                            odtf.Count = param.Count;

                            odtf.UserId = param.UserId;
                            odtf.UserName = param.UserName;
                            odtf.UserType = param.UserType;

                            odtf.CompanyId = param.CompanyId;

                            dataB.OutDealerTicketFilters.Add(odtf);

                            dataB.SaveChanges();
                            //Success                            
                            retObj.Success = true;
                            retObj.Id = odtf.Id;

                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;

                            dataBTransaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Id = Id;
            return retObj;
        }

        public CRFilterDeleteReply DeleteOutDealerTicketFilter(CRDeleteOutDealerTicketFilterParam param)
        {
            CRFilterDeleteReply retObj = new CRFilterDeleteReply();
            retObj.Success = true;

            try
            {
                
                using (var dataB = new AquaStorage())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {

                        var cr = dataB.OutDealerTicketFilters.Select(c => c).Where(x => x.Id == param.Id);
                        var r = dataB.OutDealerTicketFilters.RemoveRange(cr);
                        if (r.Count() > 0)
                        {
                            //Decrement Serial no of all items in the bill that follows this item

                            var bupdate = dataB.OutDealerTicketFilters.Select(c => c).Where(x => x.CompanyId == param.CompanyId && x.SerialNo > param.SerialNo && x.UserId == param.UserId);
                            foreach (var i in bupdate)
                            {
                                i.SerialNo = i.SerialNo > param.SerialNo ? --i.SerialNo : i.SerialNo;
                            }

                            dataB.SaveChanges();
                            dataBTransaction.Commit();
                            retObj.Success = true;
                        }
                        else
                        {
                            retObj.Success = false;
                            retObj.Message = "Item Doesnt Exist";
                        }

                    }
                    catch (Exception e)
                    {
                        retObj.Success = false;
                        retObj.Message = e.Message;
                        dataBTransaction.Rollback();
                    }
                    finally
                    {

                    }
                }

            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }

            return retObj;
        }

        public CRFilterUpdateReply UpdateOutDealerTicketFilter(CRUpdateOutDealerTicketFilterParam param)
        {
            CRFilterUpdateReply retObj = new CRFilterUpdateReply();
            retObj.Success = true;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                retObj.Message = "Session Error";
                retObj.Success = false;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            OutDealerTicketFilter u = dataB.OutDealerTicketFilters.Select(c => c).Where(x => x.Id == param.Id).FirstOrDefault();
                            u.NoOfDigits = param.NoOfDigits;
                            u.InTicketIds = param.InTicketIds;
                            u.OutTicketId = param.OutTicketId;
                            u.InTicketNames = param.InTicketNames;
                            u.OutTicketName = param.OutTicketName;
                            u.Number = param.Number;
                            u.Condition = param.Condition;
                            u.Count = param.Count;

                            dataB.SaveChanges();
                            //Success
                            retObj.Success = true;

                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            retObj.Success = false;
                            retObj.Message = e.Message;

                            dataBTransaction.Rollback();
                        }
                        finally
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            return retObj;
        }


        public List<CRSessionStatus> GetSessions(CGetSessionsParam param)
        {
            List<CRSessionStatus> sessions = null;
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                return sessions;
            }
            try
            {
                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        string sQuery = "Select U.Id As UserId, U.ParentUserId, U.UserType, U.Name, (Select Count(*) From Sessions Where Sessions.UserId=U.Id && Sessions.DeviceType=" + CSession.DEVICE_MOBILE + ")>0 As IsActive From Users U Where (U.CompanyId=@companyId && U.UserType In (" + CUser.DEALER + "," + CUser.SUB_DEALER + ")) Order By U.UserType, U.Name ";
                        sessions = dataB.Database.SqlQuery<CRSessionStatus>(sQuery,
                            new MySqlParameter("@companyId", param.CompanyId)).ToList();
                    }
                    catch { }
                }
            }
            catch { }
            return sessions;
        }

        public CRRemoveSessionReply RemoveSession(CRemoveSessionParam param)
        {
            CRRemoveSessionReply reply = new CRRemoveSessionReply();
            reply.Success = false;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || (si.UserType != CUser.SUPER_ADMIN && si.UserType != CUser.ADMIN))
            {
                reply.Message = "Session Error";
                return reply;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        string sQuery = "Delete From Sessions Where (Sessions.UserId=@userId && Sessions.DeviceType=" + CSession.DEVICE_MOBILE + ")";
                        long rows = dataB.Database.ExecuteSqlCommand(sQuery,
                            new MySqlParameter("@userId", param.UserId));
                        if (rows > 0)
                        {
                            reply.Message = "Successfully Removed Session";
                            reply.Success = true;
                        }
                        else
                        {
                            reply.Message = "Could Not Delete";
                            reply.Success = false;
                        }
                    }
                    catch (Exception e)
                    {
                        reply.Message = e.Message;
                        reply.Success = false;
                    }
                }
            }
            catch (Exception e)
            {
                reply.Message = e.Message;
                reply.Success = false;
            }

            return reply;
        }


        private bool IsUserRemovable(long userCode)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Accounts.Select(e => e).Where(e => e.Id == userCode).FirstOrDefault();
                if (data.UserType == CUser.SUPER_ADMIN)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsUserHasChild(long userCode)
        {
            bool hasChild = false;
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Accounts.Select(e => e).Where(e => e.ParentUserId == userCode);
                if (data.Count() > 0)
                {
                    hasChild = true;
                }
            }
            return hasChild;
        }

        private bool IsUserUsedInTransaction(long userId)
        {
            bool isUsed = false;
            using (var dataB = new AquaStorage())
            {
                var dTS = dataB.TicketSales.Select(e => e).Where(e => e.UserId == userId);
                if (dTS.Count() > 0)
                {
                    isUsed = true;
                }
                else
                {
                    var dAE = dataB.AccountEntries.Select(e => e).Where(e => e.UserId == userId);
                    if (dAE.Count() > 0)
                    {
                        isUsed = true;
                    }

                    else
                    {
                        var dDTS = dataB.DealerHoldingLimits.Select(e => e).Where(e => e.UserId == userId);
                        if (dDTS.Count() > 0)
                        {
                            isUsed = true;
                        }
                        else
                        {
                            var dt1 = dataB.TicketSalesCache.Select(e => e).Where(e => e.UserId == userId);
                            if (dt1.Count() > 0)
                            {
                                isUsed = true;
                            }
                            else
                            {
                                var dt2 = dataB.Winners.Select(e => e).Where(e => e.UserId == userId);
                                if (dt2.Count() > 0)
                                {
                                    isUsed = true;
                                }
                                else
                                {
                                    var dt3 = dataB.ProfitnLosses.Select(e => e).Where(e => e.UserId == userId);
                                    if (dt3.Count() > 0)
                                    {
                                        isUsed = true;
                                    }
                                    else
                                    {
                                        var dt4 = dataB.DealerTicketCounts.Select(e => e).Where(e => e.UserId == userId);
                                        if (dt4.Count() > 0)
                                        {
                                            isUsed = true;
                                        }
                                        else
                                        {
                                            var dt5 = dataB.DealerTicketLimits.Select(e => e).Where(e => e.UserId == userId);
                                            if (dt5.Count() > 0)
                                            {
                                                isUsed = true;
                                            }
                                            else
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return isUsed;
        }

        private string GetNumberForMask(string number, string mask)
        {
            string tNo = "";
            try
            {
                for (int i = 0; i < mask.Length; i++)
                {
                    if (mask.ElementAt(i).Equals('A'))
                    {
                        tNo += number.ElementAt(0);
                    }
                    else if (mask.ElementAt(i).Equals('B'))
                    {
                        tNo += number.ElementAt(1);
                    }
                    else if (mask.ElementAt(i).Equals('C'))
                    {
                        tNo += number.ElementAt(2);
                    }
                }
            }
            catch { }
            return tNo;
        }

        private bool IsUsernameAlreadyUsed(string username, long companyId)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Accounts.Select(e => e).Where(e => e.Username == username && e.CompanyId == companyId);
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
                var data = dataB.Accounts.Select(e => e).Where(e => e.Username == username && e.CompanyId == companyId && e.Id != userId);
                if (data.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        //Unused
        
        public CRTicketSalesReportReply GetDetailedSalesReport(CGetSalesReportParam param)
        {
            CRTicketSalesReportReply retObj = new CRTicketSalesReportReply();
            retObj.Success = true;

            List<CRTicketSalesReport> ticketSReport = new List<CRTicketSalesReport>();
            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(param.SessionId);
            if (si == null || si.UserId != param.RequestUserId || si.UserType!=CUser.SUPER_ADMIN)
            {
                retObj.Success = false;
                retObj.Message = "Not Allowed";
                retObj.Report = ticketSReport;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        //DayCodes                    
                        long startD = long.Parse(string.Format("{0}{1:00}{2:00}", param.FromYear, param.FromMonth, param.FromDay));
                        long endD = long.Parse(string.Format("{0}{1:00}{2:00}", param.ToYear, param.ToMonth, param.ToDay));

                        string companyIdQuery = " && (TS.CompanyId=@companyId) && (UR.CompanyId=@companyId)";
                        //string userIdQuery = param.UserId == 0 ? " && ((UR.Id=@ownerId)||(UR.OwnerId=@ownerId))" : " && (UR.Id=@userId)";
                        string userIdQuery = param.UserId == 0 ? si.UserType == CUser.SUPER_ADMIN ? "" : si.UserType == CUser.ADMIN ? " && (((UR.UserType=" + CUser.DEALER + ") || (UR.UserType=" + CUser.SUB_DEALER + ")) || (UR.Id=@ownerId))" : " && ((UR.Id=@ownerId)||(UR.OwnerId=@ownerId))" : " && (UR.Id=@userId)";
                        string ticketIdQuery = param.TicketId == 0 ? "" : " && (TS.TicketId=@ticketId) ";
                        string noOfDigitsQuery = param.NoOfDigits == 0 ? "" : " && (TR.NoOfDigits=@noOfDigits) ";

                        string subQ = companyIdQuery + userIdQuery + ticketIdQuery + noOfDigitsQuery;

                        var dataSummary = dataB.Database.SqlQuery<CRTicketSalesReport>("Select Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, UR.Id As UserId, UR.Name As UserName, Sum(TS.Count) As Count, Sum(TS.Count*(TS.TicketRate-TS.TicketCommission)) As Amount From TicketSales TS, Tickets TR, Users UR Where(TS.DayCode >= @startD && TS.DayCode <=@endD) && (TS.UserId=UR.Id) && (TR.Id=TS.TicketId) " + subQ + " Group By BillYear, BillMonth, BillDay, TS.BillNo, UserId, UserName Order By TS.DayCode Desc,UR.UserType,UR.Name,TS.BillNo Desc",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", param.UserId),
                            new MySqlParameter("@ownerId", param.RequestUserId),
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId)).ToList();

                        if (dataSummary != null)
                        {
                            foreach (var i in dataSummary)
                            {
                                CRTicketSalesReport crt = new CRTicketSalesReport { Amount = i.Amount, BillDay = i.BillDay, BillId = i.BillId, BillMonth = i.BillMonth, BillNo = i.BillNo, BillYear = i.BillYear, Count = i.Count, SerialNo = i.SerialNo, TicketName = i.TicketName, TicketNo = i.TicketNo, UserId = i.UserId, UserName = i.UserName };
                                ticketSReport.Add(crt);

                                string subQ1 = " && (TS.CompanyId=@companyId) && (TS.UserId=@userId) && (TS.BillNo=@billNo)" + ticketIdQuery + noOfDigitsQuery;
                                var dataItems = dataB.Database.SqlQuery<CRTicketSalesReport>("Select TS.Id As BillId, Year(TS.BillDate) As BillYear, Month(TS.BillDate) As BillMonth, Day(TS.BillDate) As BillDay, TS.BillNo, TS.SerialNo, UR.Id As UserId, UR.Name As UserName, TR.Name As TicketName, TS.TicketNo, TS.Count, TS.Count*(TS.TicketRate-TS.TicketCommission) As Amount From TicketSales TS,Tickets TR,Users UR Where(TS.DayCode >= @startD && TS.DayCode <=@endD) && (TR.Id=TS.TicketId)  && (UR.Id=TS.UserId) " + subQ1 + " Order By TS.DayCode Desc,UR.UserType,UR.Name,TS.BillNo Desc,TS.SerialNo",
                            new MySqlParameter("@startD", startD),
                            new MySqlParameter("@endD", endD),
                            new MySqlParameter("@companyId", param.CompanyId),
                            new MySqlParameter("@userId", crt.UserId),
                            new MySqlParameter("@billNo", crt.BillNo),
                            new MySqlParameter("@noOfDigits", param.NoOfDigits - 1),
                            new MySqlParameter("@ticketId", param.TicketId));
                                if (dataItems != null)
                                {
                                    ticketSReport.AddRange(dataItems);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Success = false;
                retObj.Message = e.Message;
            }
            retObj.Report = ticketSReport;
            return retObj;
        }

    */
    }

}
