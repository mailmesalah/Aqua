using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServerServiceInterface
{    
    [ServiceContract]
    public interface ITransaction
    {        
        [OperationContract]
        long ReadNextBillNo(int billType, long companyId);
        [OperationContract]
        CBoolMessage DeleteBill(long billNo, int billType,long financialCode, long companyId, string sessionId);
        [OperationContract]
        CACTransactionParam ReadBill(long billNo, int billType, long companyId, string sessionId);
        [OperationContract]
        CTransactionMessage AddBill(CACTransactionParam tBill, string sessionId);
        [OperationContract]
        CTransactionMessage UpdateBill(CACTransactionParam tBill, string sessionId);

        [OperationContract]
        CDayBookReportRE ReadDayBook(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, int limitIndex, int limitSize, long companyId, string sessionId);
        [OperationContract]
        CDayBookReportTotalRE ReadDayBookTotal(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, long companyId, string sessionId);

        [OperationContract]
        CTrialBalanceReportRE ReadTrialBalance(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, long companyId, string sessionId);

        [OperationContract]
        CAssetnLiabilityReportRE ReadAssetsnLiabilities(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, long companyId, string sessionId);
        [OperationContract]
        CIncomenExpenseReportRE ReadIncomenExpense(DateTime fromDate, DateTime toDate, int billType, int mainGroup, long parentGroupId, long accountId, long billNo, long companyId, string sessionId);

    }

    
    public class CBillTypes
    {        
        public const int PERSONAL_PAYMENT = 123;
        public const int PERSONAL_RECEIPT = 321;
        public const int CREDIT_PAYMENT = 223;
        public const int CREDIT_RECEIPT = 221;
        public const int RECEIVABLE = 322;
        public const int PAYABLE = 124;
        public const int ACCOUNT_TRANSFER = 200;
        public const int PURCHASE = 400;
        public const int SALES = 500;
        public const int APPRECIATION = 600;
        public const int DEPRECIATION = 700;

    }

    public class CBillSubTypes
    {
        public const int PERSONAL_PAYMENT_CREDIT_PERSONAL = 1231;
        public const int PERSONAL_PAYMENT_PERSONAL_CREDIT = 1232;
        public const int PERSONAL_PAYMENT_MONETARY_PERSONAL = 1233;
        public const int PERSONAL_PAYMENT_PERSONAL_MONETARY = 1234;

        public const int PERSONAL_RECEIPT_CREDIT_PERSONAL = 3211;
        public const int PERSONAL_RECEIPT_PERSONAL_CREDIT = 3212;
        public const int PERSONAL_RECEIPT_MONETARY_PERSONAL = 3213;
        public const int PERSONAL_RECEIPT_PERSONAL_MONETARY = 3214;

        public const int CREDIT_PAYMENT_MONETARY_CREDIT = 2233;
        public const int CREDIT_PAYMENT_CREDIT_MONETARY = 2234;

        public const int CREDIT_RECEIPT_MONETARY_CREDIT = 2213;
        public const int CREDIT_RECEIPT_CREDIT_MONETARY = 2214;

        public const int PAYABLE_CREDIT_PERSONAL = 1241;
        public const int PAYABLE_PERSONAL_CREDIT = 1242;
     
        public const int RECEIVABLE_CREDIT_PERSONAL = 3221;
        public const int RECEIVABLE_PERSONAL_CREDIT = 3222;

        public const int ACCOUNT_TRANSFER_CREDIT_PERSONAL = 2001;
        public const int ACCOUNT_TRANSFER_DEBIT_PERSONAL = 2002;

        public const int PURCHASE_STOCK_PERSONAL = 4001;
        public const int PURCHASE_PERSONAL_STOCK = 4002;
        public const int PURCHASE_MONETARY_PERSONAL = 4003;
        public const int PURCHASE_PERSONAL_MONETARY = 4004;

        public const int SALES_STOCK_PERSONAL = 5001;
        public const int SALES_PERSONAL_STOCK = 5002;
        public const int SALES_MONETARY_PERSONAL = 5003;
        public const int SALES_PERSONAL_MONETARY = 5004;

        public const int APPRECIATION_CREDIT_STOCK = 6001;
        public const int APPRECIATION_STOCK_CREDIT = 6002;

        public const int DEPRECIATION_CREDIT_STOCK = 7001;
        public const int DEPRECIATION_STOCK_CREDIT = 7002;
    }

    
    [DataContract]
    public class CACTransactionParam
    {        
        [DataMember]
        public long BillNo { get; set; }
        [DataMember]
        public int BillType { get; set; }
        [DataMember]
        public DateTime BillDate { get; set; }
        [DataMember]
        public List<CACTransactionItem> Items { get; set; }
        [DataMember]
        public string Narration { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public long AddedUserId { get; set; }        
        [DataMember]
        public long FinancialCode { get; set; }
    }

    [DataContract]
    public class CACTransactionItem
    {
        [DataMember]
        public long Id { get; set; }        
        [DataMember]
        public int SubBillType { get; set; }
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public long AccountId { get; set; }
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public int AccountType { get; set; }
        [DataMember]
        public int MainGroup { get; set; }
        [DataMember]
        public long ParentGroupId { get; set; }
        [DataMember]
        public double Debit { get; set; }
        [DataMember]
        public double Credit { get; set; }        
    }


    [DataContract]
    public class CDayBookReportRE
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CDayBookReportItem> Report { get; set; }

    }

    [DataContract]
    public class CDayBookReportItem
    {
        [DataMember]
        public long BillNo { get; set; }
        [DataMember]
        public int BillType { get; set; }
        [DataMember]
        public DateTime BillDate { get; set; }
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public string SBillType { get; set; }
        [DataMember]
        public int MainGroup { get; set; }
        [DataMember]
        public long ParentGroupId { get; set; }
        [DataMember]
        public double Debit { get; set; }
        [DataMember]
        public double Credit { get; set; }
        [DataMember]
        public string Narration { get; set; }               
        [DataMember]
        public long FinancialCode { get; set; }
    }

    [DataContract]
    public class CDayBookReportTotalRE
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public CTotalDebitCreditRE Total { get; set; }        

    }

    [DataContract]
    public class CTotalDebitCreditRE
    {
        [DataMember]
        public long Debit { get; set; }
        [DataMember]
        public long Credit { get; set; }

    }

    [DataContract]
    public class CTrialBalanceReportRE
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CTrialBalanceReportItem> Report { get; set; }

    }

    [DataContract]
    public class CTrialBalanceReportItem
    {
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public int MainGroup { get; set; }
        [DataMember]
        public long ParentGroupId { get; set; }
        [DataMember]
        public double Debit { get; set; }
        [DataMember]
        public double Credit { get; set; }
        [DataMember]
        public string SMainGroup { get; set; }
    }


    [DataContract]
    public class CAssetnLiabilityReportRE
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CAnLReportItem> Assets { get; set; }
        [DataMember]
        public List<CAnLReportItem> Liabilities { get; set; }

    }

    [DataContract]
    public class CAnLReportItem
    {
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public int MainGroup { get; set; }
        [DataMember]
        public long ParentGroupId { get; set; }
        [DataMember]
        public double Amount { get; set; }
        [DataMember]
        public string SMainGroup { get; set; }
    }

    [DataContract]
    public class CIncomenExpenseReportRE
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CInEReportItem> Income { get; set; }
        [DataMember]
        public List<CInEReportItem> Expense { get; set; }

    }

    [DataContract]
    public class CInEReportItem
    {
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public int MainGroup { get; set; }
        [DataMember]
        public long ParentGroupId { get; set; }
        [DataMember]
        public double Amount { get; set; }
        [DataMember]
        public string SMainGroup { get; set; }
    }

}
