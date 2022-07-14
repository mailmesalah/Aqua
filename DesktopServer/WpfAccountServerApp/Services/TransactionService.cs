using MySql.Data.MySqlClient;
using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using AquaServer.General;
using AquaServer.Local;
using AquaServer.StorageModel;

namespace AquaServer.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    public class TransactionService : ITransaction
    {
        
        public long ReadNextBillNo(int billType, long companyId)
        {
            try
            {
                BillNoGeneratorService bns = new BillNoGeneratorService();
                return bns.GetNextBillNo(companyId, billType);
            }
            catch { return -1; }
        }

        public CBoolMessage DeleteBill(long billNo, int billType, long financialCode, long companyId, string sessionId)
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
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {

                            var cr = dataB.Transactions.Select(c => c).Where(x => x.CompanyId == companyId && x.FinancialCode == financialCode && x.BillType == billType && x.BillNo == billNo);
                            var r = dataB.Transactions.RemoveRange(cr);
                            if (r.Count() > 0)
                            {
                                dataB.SaveChanges();

                                dataBTransaction.Commit();
                                cbm.IsSuccess = true;
                            }
                            else
                            {
                                cbm.IsSuccess = false;
                                cbm.Message = "Bill Doesnt Exist Or not available to Delete";
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

        public CACTransactionParam ReadBill(long billNo, int billType, long companyId, string sessionId)
        {
            CACTransactionParam bill = new CACTransactionParam();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                return bill;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    List<Transaction> trans = dataB.Transactions.Where(e => e.CompanyId == companyId && e.BillType == billType && e.BillNo == billNo).OrderBy(e => e.SubBillType).ThenBy(e=>e.SerialNo).ToList();
                    if (trans != null && trans.Count > 0)
                    {
                        bill.Items = trans.Select(d => new CACTransactionItem { Id = d.Id, SubBillType = d.SubBillType, SerialNo = d.SerialNo, AccountId = d.AccountId, AccountName = d.AccountName, AccountType = d.AccountType, MainGroup = d.MainGroup, ParentGroupId = d.ParentGroupId, Credit = d.Credit, Debit = d.Debit }).ToList<CACTransactionItem>();
                        bill.BillNo = trans.ElementAt(0).BillNo;
                        bill.BillType = trans.ElementAt(0).BillType;
                        bill.BillDate = trans.ElementAt(0).BillDate;
                        bill.AddedUserId= trans.ElementAt(0).AddedUserId;
                        bill.CompanyId= trans.ElementAt(0).CompanyId;
                        bill.Narration= trans.ElementAt(0).Narration;
                        bill.FinancialCode= trans.ElementAt(0).FinancialCode;
                    }
                }
            }
            catch { }
            return bill;
        }

        public CTransactionMessage AddBill(CACTransactionParam tBill, string sessionId)
        {
            CTransactionMessage cbm = new CTransactionMessage();
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
                SettingsService ss = new SettingsService();
                long financialCode = ss.GetFinancialCode(tBill.BillDate,tBill.CompanyId);
                if (financialCode == -1)
                {
                    cbm.Message = "Not Allowed to Enter Item at this Time";
                    return cbm;
                }

                lock (Synchronizer.@lock)
                {

                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            BillNoGeneratorService bns = new BillNoGeneratorService();
                            long billNo = 0;
                            billNo = bns.GetNextBillNo(tBill.CompanyId, tBill.BillType);
                            foreach (var it in tBill.Items)                            
                            {
                                Transaction u = dataB.Transactions.Create();
                                u.CompanyId = tBill.CompanyId;
                                u.BillType = tBill.BillType;
                                u.AddedUserId = tBill.AddedUserId;
                                u.FinancialCode = financialCode;
                                u.Narration = tBill.Narration;
                                u.BillNo = billNo;
                                u.BillDate = tBill.BillDate;
                                u.SerialNo = it.SerialNo;
                                u.SubBillType = it.SubBillType;
                                u.MainGroup = it.MainGroup;
                                u.ParentGroupId = it.ParentGroupId;
                                u.AccountId = it.AccountId;
                                u.AccountName = it.AccountName;
                                u.AccountType = it.AccountType;
                                u.Credit = it.Credit;
                                u.Debit = it.Debit;                                
                                dataB.Transactions.Add(u);
                            }
                            //Update Bill No
                            bns.SetBillNo(tBill.CompanyId, tBill.BillType, billNo + 1);

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
            }
            catch (Exception e) { cbm.Message = e.Message; }
            return cbm;
        }

        public CTransactionMessage UpdateBill(CACTransactionParam tBill, string sessionId)
        {
            CTransactionMessage cbm = new CTransactionMessage();
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
                SettingsService ss = new SettingsService();
                long financialCode = ss.GetFinancialCode(tBill.CompanyId);
                if (financialCode == -1)
                {
                    cbm.Message = "Not Allowed to Enter Item at this Time";
                    return cbm;
                }

                //lock (Synchronizer.@lock)
                {

                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            String sQuery = "Delete From Transactions Where(FinancialCode = @financialCode && BillType = @billType && BillNo = @billNo && CompanyId = @companyId)";
                            int count = dataB.Database.ExecuteSqlCommand(sQuery,
                                new MySqlParameter("@financialCode", tBill.FinancialCode),
                                new MySqlParameter("@billType", tBill.BillType),
                                new MySqlParameter("@billNo", tBill.BillNo),
                                new MySqlParameter("@companyId", tBill.CompanyId));

                            foreach (var it in tBill.Items)
                            {
                                Transaction u = dataB.Transactions.Create();
                                u.CompanyId = tBill.CompanyId;
                                u.BillType = tBill.BillType;
                                u.AddedUserId = tBill.AddedUserId;
                                u.FinancialCode = tBill.FinancialCode;
                                u.Narration = tBill.Narration;
                                u.BillNo = tBill.BillNo;
                                u.BillDate = tBill.BillDate;
                                u.SerialNo = it.SerialNo;
                                u.SubBillType = it.SubBillType;
                                u.MainGroup = it.MainGroup;
                                u.ParentGroupId = it.ParentGroupId;
                                u.AccountId = it.AccountId;
                                u.AccountName = it.AccountName;
                                u.AccountType = it.AccountType;
                                u.Credit = it.Credit;
                                u.Debit = it.Debit;
                                dataB.Transactions.Add(u);
                            }


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
            }
            catch (Exception e) { cbm.Message = e.Message; }
            return cbm;
        }


        public CDayBookReportRE ReadDayBook(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, int limitIndex, int limitSize, long companyId, string sessionId)
        {
            CDayBookReportRE retObj = new CDayBookReportRE();
            retObj.Success = true;

            List<CDayBookReportItem> report = new List<CDayBookReportItem>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
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
                        string billTypeQuery = billType==-1 ? "" : " && (TS.BillType=@billType) ";
                        string billNoQuery = billNo == -1 ? "" : " && (TS.BillNo=@billNo) ";
                        string companyIdQuery = " && (TS.CompanyId=@companyId) ";
                        string mainGroupQuery = mainGroup == -1 ? "" : " && (TS.MainGroup=@mainGroup) ";
                        string parentGroupIdQuery = parentGroupId == -1 ? "" : " && (TS.ParentGroupId=@parentGroupId) ";
                        string accountIdQuery = accountId==-1 ? "" : " && (TS.AccountId=@accountId) ";

                        string subQ = companyIdQuery + parentGroupIdQuery + mainGroupQuery + billNoQuery + billTypeQuery + accountIdQuery;


                        report = dataB.Database.SqlQuery<CDayBookReportItem>("Select TS.BillDate, TS.BillNo, TS.BillType, TS.AccountName, TS.MainGroup, TS.ParentGroupId, TS.Debit, TS.Credit From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) " + subQ + " Order By TS.BillDate Desc, TS.BillType,TS.SubBillType,TS.BillNo Desc,TS.SerialNo Limit @limitIndex,@limitSize",
                            new MySqlParameter("@startD", fromDate),
                            new MySqlParameter("@endD", toDate),
                            new MySqlParameter("@billNo", billNo),
                            new MySqlParameter("@companyId", companyId),
                            new MySqlParameter("@mainGroup", mainGroup),
                            new MySqlParameter("@parentGroupId", parentGroupId),
                            new MySqlParameter("@billType", billType),
                            new MySqlParameter("@accountId", accountId),
                            new MySqlParameter("@limitIndex", limitIndex),
                            new MySqlParameter("@limitSize", limitSize)).ToList();

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

        public CDayBookReportTotalRE ReadDayBookTotal(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, long companyId, string sessionId)
        {
            CDayBookReportTotalRE total = new CDayBookReportTotalRE();
            total.Success = true;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                total.Success = false;
                total.Message = "Not Allowed";
                return total;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        string billTypeQuery = billType == -1 ? "" : " && (TS.BillType=@billType) ";
                        string billNoQuery = billNo == -1 ? "" : " && (TS.BillNo=@billNo) ";
                        string companyIdQuery = " && (TS.CompanyId=@companyId) ";
                        string mainGroupQuery = mainGroup == -1 ? "" : " && (TS.MainGroup=@mainGroup) ";
                        string parentGroupIdQuery = parentGroupId == -1 ? "" : " && (TS.ParentGroupId=@parentGroupId) ";
                        string accountIdQuery = accountId == -1 ? "" : " && (TS.AccountId=@accountId) ";

                        string subQ = companyIdQuery + parentGroupIdQuery + mainGroupQuery + billNoQuery + billTypeQuery + accountIdQuery;

                        try
                        {
                            total.Total = dataB.Database.SqlQuery<CTotalDebitCreditRE>("Select Sum(TS.Debit) As Debit, Sum(TS.Credit) As Credit From Transactions TS Where(TS.BillDate >= @startD && TS.BillDate <=@endD) " + subQ,
                                new MySqlParameter("@startD", fromDate),
                                new MySqlParameter("@endD", toDate),
                                new MySqlParameter("@billNo", billNo),
                                new MySqlParameter("@companyId", companyId),
                                new MySqlParameter("@mainGroup", mainGroup),
                                new MySqlParameter("@parentGroupId", parentGroupId),
                                new MySqlParameter("@billType", billType),
                                new MySqlParameter("@accountId", accountId)).FirstOrDefault();
                        }catch
                        {
                            total.Total = new CTotalDebitCreditRE { Debit = 0, Credit = 0 };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                total.Message = e.Message;
                total.Success = false;
            }
            
            return total;
        }


        public CTrialBalanceReportRE ReadTrialBalance(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, long companyId, string sessionId)
        {
            CTrialBalanceReportRE retObj = new CTrialBalanceReportRE();
            retObj.Success = true;

            List<CTrialBalanceReportItem> report = new List<CTrialBalanceReportItem>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
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
                        string billTypeQuery = billType == -1 ? "" : " && (TS.BillType=@billType) ";
                        string billNoQuery = billNo == -1 ? "" : " && (TS.BillNo=@billNo) ";
                        string companyIdQuery = " && (TS.CompanyId=@companyId) ";
                        string mainGroupQuery = mainGroup == -1 ? "" : " && (TS.MainGroup=@mainGroup) ";
                        string parentGroupIdQuery = parentGroupId == -1 ? "" : " && (TS.ParentGroupId=@parentGroupId) ";
                        string accountIdQuery = accountId == -1 ? "" : " && (TS.AccountId=@accountId) ";

                        string subQ = companyIdQuery + parentGroupIdQuery + mainGroupQuery + billNoQuery + billTypeQuery + accountIdQuery;

                        //try
                        //{
                            report = dataB.Database.SqlQuery<CTrialBalanceReportItem>("Select TS.AccountName, TS.MainGroup, TS.ParentGroupId, If(Sum(TS.Debit)>Sum(TS.Credit), Sum(TS.Debit)-Sum(TS.Credit), 0)As Debit, If(Sum(TS.Credit)>Sum(TS.Debit),Sum(TS.Credit)-Sum(TS.Debit),0) As Credit From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) " + subQ + " Group By TS.AccountId, TS.AccountName, TS.MainGroup, TS.ParentGroupId Order By TS.MainGroup, TS.AccountName",
                                new MySqlParameter("@startD", fromDate),
                                new MySqlParameter("@endD", toDate),
                                new MySqlParameter("@billNo", billNo),
                                new MySqlParameter("@companyId", companyId),
                                new MySqlParameter("@mainGroup", mainGroup),
                                new MySqlParameter("@parentGroupId", parentGroupId),
                                new MySqlParameter("@billType", billType),
                                new MySqlParameter("@accountId", accountId)).ToList();
                        //}
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


        public CAssetnLiabilityReportRE ReadAssetsnLiabilities(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, long companyId, string sessionId)
        {
            CAssetnLiabilityReportRE retObj = new CAssetnLiabilityReportRE();
            retObj.Success = true;

            List<CAnLReportItem> assets = new List<CAnLReportItem>();
            List<CAnLReportItem> liabilities = new List<CAnLReportItem>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                retObj.Assets = assets;
                retObj.Liabilities = liabilities;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        string billTypeQuery = billType == -1 ? "" : " && (TS.BillType=@billType) ";
                        string billNoQuery = billNo == -1 ? "" : " && (TS.BillNo=@billNo) ";
                        string companyIdQuery = " && (TS.CompanyId=@companyId) ";
                        //string mainGroupQuery = mainGroup == -1 ? "" : " && (TS.MainGroup=@mainGroup) ";
                        string parentGroupIdQuery = parentGroupId == -1 ? "" : " && (TS.ParentGroupId=@parentGroupId) ";
                        string accountIdQuery = accountId == -1 ? "" : " && (TS.AccountId=@accountId) ";

                        string subQ = companyIdQuery + parentGroupIdQuery + billNoQuery + billTypeQuery + accountIdQuery;

                        //try
                        //{
                        //Monetary Accounts
                        assets = dataB.Database.SqlQuery<CAnLReportItem>("Select TS.AccountName, TS.MainGroup, TS.ParentGroupId, (Sum(TS.Debit)-Sum(TS.Credit))As Amount From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) && (TS.MainGroup="+CAccount.MONETARY_ACCOUNT+") " + subQ + " Group By TS.AccountId, TS.AccountName, TS.MainGroup, TS.ParentGroupId Order By TS.MainGroup, TS.AccountName",
                            new MySqlParameter("@startD", fromDate),
                            new MySqlParameter("@endD", toDate),
                            new MySqlParameter("@billNo", billNo),
                            new MySqlParameter("@companyId", companyId),
                            //new MySqlParameter("@mainGroup", mainGroup),
                            new MySqlParameter("@parentGroupId", parentGroupId),
                            new MySqlParameter("@billType", billType),
                            new MySqlParameter("@accountId", accountId)).Where(e=>e.Amount>0).ToList();

                        //Personal Accounts
                        assets.AddRange(dataB.Database.SqlQuery<CAnLReportItem>("Select TS.AccountName, TS.MainGroup, TS.ParentGroupId, (Sum(TS.Credit)-Sum(TS.Debit))As Amount From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) && (TS.MainGroup=" + CAccount.PERSONAL_ACCOUNT + ") " + subQ + " Group By TS.AccountId, TS.AccountName, TS.MainGroup, TS.ParentGroupId Order By TS.MainGroup, TS.AccountName",
                            new MySqlParameter("@startD", fromDate),
                            new MySqlParameter("@endD", toDate),
                            new MySqlParameter("@billNo", billNo),
                            new MySqlParameter("@companyId", companyId),
                            //new MySqlParameter("@mainGroup", mainGroup),
                            new MySqlParameter("@parentGroupId", parentGroupId),
                            new MySqlParameter("@billType", billType),
                            new MySqlParameter("@accountId", accountId)).Where(e => e.Amount > 0).ToList());

                        //Stock Accounts
                        assets.AddRange(dataB.Database.SqlQuery<CAnLReportItem>("Select TS.AccountName, TS.MainGroup, TS.ParentGroupId, (Sum(TS.Debit)-Sum(TS.Credit))As Amount From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) && (TS.MainGroup=" + CAccount.STOCK_ACCOUNT + ") " + subQ + " Group By TS.AccountId, TS.AccountName, TS.MainGroup, TS.ParentGroupId Order By TS.MainGroup, TS.AccountName",
                            new MySqlParameter("@startD", fromDate),
                            new MySqlParameter("@endD", toDate),
                            new MySqlParameter("@billNo", billNo),
                            new MySqlParameter("@companyId", companyId),
                            //new MySqlParameter("@mainGroup", mainGroup),
                            new MySqlParameter("@parentGroupId", parentGroupId),
                            new MySqlParameter("@billType", billType),
                            new MySqlParameter("@accountId", accountId)).ToList());

                        //Monetary Accounts
                        liabilities = dataB.Database.SqlQuery<CAnLReportItem>("Select TS.AccountName, TS.MainGroup, TS.ParentGroupId, (Sum(TS.Credit)-Sum(TS.Debit))As Amount From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) && (TS.MainGroup=" + CAccount.MONETARY_ACCOUNT + ") " + subQ + " Group By TS.AccountId, TS.AccountName, TS.MainGroup, TS.ParentGroupId Order By TS.MainGroup, TS.AccountName",
                            new MySqlParameter("@startD", fromDate),
                            new MySqlParameter("@endD", toDate),
                            new MySqlParameter("@billNo", billNo),
                            new MySqlParameter("@companyId", companyId),
                            //new MySqlParameter("@mainGroup", mainGroup),
                            new MySqlParameter("@parentGroupId", parentGroupId),
                            new MySqlParameter("@billType", billType),
                            new MySqlParameter("@accountId", accountId)).Where(e => e.Amount > 0).ToList();

                        //Personal Accounts
                        liabilities.AddRange(dataB.Database.SqlQuery<CAnLReportItem>("Select TS.AccountName, TS.MainGroup, TS.ParentGroupId, (Sum(TS.Debit)-Sum(TS.Credit))As Amount From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) && (TS.MainGroup=" + CAccount.PERSONAL_ACCOUNT + ") " + subQ + " Group By TS.AccountId, TS.AccountName, TS.MainGroup, TS.ParentGroupId Order By TS.MainGroup, TS.AccountName",
                            new MySqlParameter("@startD", fromDate),
                            new MySqlParameter("@endD", toDate),
                            new MySqlParameter("@billNo", billNo),
                            new MySqlParameter("@companyId", companyId),
                            //new MySqlParameter("@mainGroup", mainGroup),
                            new MySqlParameter("@parentGroupId", parentGroupId),
                            new MySqlParameter("@billType", billType),
                            new MySqlParameter("@accountId", accountId)).Where(e => e.Amount > 0).ToList());


                        //}
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            retObj.Assets = assets;
            retObj.Liabilities = liabilities;
            return retObj;
        }

        public CIncomenExpenseReportRE ReadIncomenExpense(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, long companyId, string sessionId)
        {
            CIncomenExpenseReportRE retObj = new CIncomenExpenseReportRE();
            retObj.Success = true;

            List<CInEReportItem> income = new List<CInEReportItem>();
            List<CInEReportItem> expense = new List<CInEReportItem>();

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null)
            {
                retObj.Message = "Not Allowed";
                retObj.Success = false;
                retObj.Income = income;
                retObj.Expense = expense;
                return retObj;
            }

            try
            {
                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        string billTypeQuery = billType == -1 ? "" : " && (TS.BillType=@billType) ";
                        string billNoQuery = billNo == -1 ? "" : " && (TS.BillNo=@billNo) ";
                        string companyIdQuery = " && (TS.CompanyId=@companyId) ";
                        //string mainGroupQuery = mainGroup == -1 ? "" : " && (TS.MainGroup=@mainGroup) ";
                        string parentGroupIdQuery = parentGroupId == -1 ? "" : " && (TS.ParentGroupId=@parentGroupId) ";
                        string accountIdQuery = accountId == -1 ? "" : " && (TS.AccountId=@accountId) ";

                        string subQ = companyIdQuery + parentGroupIdQuery + billNoQuery + billTypeQuery + accountIdQuery;

                        //Income Accounts
                        income = dataB.Database.SqlQuery<CInEReportItem>("Select TS.AccountName, TS.MainGroup, TS.ParentGroupId, (Sum(TS.Credit)-Sum(TS.Debit))As Amount From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) && (TS.MainGroup=" + CAccount.CREDIT_ACCOUNT + ") " + subQ + " Group By TS.AccountId, TS.AccountName, TS.MainGroup, TS.ParentGroupId Order By TS.MainGroup, TS.AccountName",
                            new MySqlParameter("@startD", fromDate),
                            new MySqlParameter("@endD", toDate),
                            new MySqlParameter("@billNo", billNo),
                            new MySqlParameter("@companyId", companyId),
                            //new MySqlParameter("@mainGroup", mainGroup),
                            new MySqlParameter("@parentGroupId", parentGroupId),
                            new MySqlParameter("@billType", billType),
                            new MySqlParameter("@accountId", accountId)).Where(e => e.Amount > 0).ToList();

                        //Expense Accounts
                        expense=dataB.Database.SqlQuery<CInEReportItem>("Select TS.AccountName, TS.MainGroup, TS.ParentGroupId, (Sum(TS.Debit)-Sum(TS.Credit))As Amount From Transactions TS Where(Date(TS.BillDate) >= @startD && Date(TS.BillDate) <=@endD) && (TS.MainGroup=" + CAccount.CREDIT_ACCOUNT + ") " + subQ + " Group By TS.AccountId, TS.AccountName, TS.MainGroup, TS.ParentGroupId Order By TS.MainGroup, TS.AccountName",
                            new MySqlParameter("@startD", fromDate),
                            new MySqlParameter("@endD", toDate),
                            new MySqlParameter("@billNo", billNo),
                            new MySqlParameter("@companyId", companyId),
                            //new MySqlParameter("@mainGroup", mainGroup),
                            new MySqlParameter("@parentGroupId", parentGroupId),
                            new MySqlParameter("@billType", billType),
                            new MySqlParameter("@accountId", accountId)).Where(e => e.Amount > 0).ToList();
                 
                    }
                }
            }
            catch (Exception e)
            {
                retObj.Message = e.Message;
                retObj.Success = false;
            }

            retObj.Income = income;
            retObj.Expense = expense;
            return retObj;
        }
    }

}
