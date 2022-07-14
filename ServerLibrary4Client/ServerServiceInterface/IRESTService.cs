using ServerServiceInterface.RestClasses;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ServerServiceInterface
{
    [ServiceContract]
    public interface IRESTService
    {
        /*
        [OperationContract]
        [WebInvoke(UriTemplate = "/GetLoginDetails",Method ="POST", RequestFormat =WebMessageFormat.Json,ResponseFormat =WebMessageFormat.Json)]
        CServerMessage GetLoginDetails(CClientLocalInfo culi);
        //[OperationContract]

        [WebInvoke(UriTemplate = "/GetTickets", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<CTicket> GetTickets(CGetTicketsParam param);

        [WebInvoke(UriTemplate = "/GetUserNSubUsers", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<CUser> GetUserNSubUsers(CGetUserNSubUsersParam param);

        [WebInvoke(UriTemplate = "/GetSubUsersWithStatus", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<CUser> GetSubUsersWithStatus(CGetSubUsersParam param);

        [WebInvoke(UriTemplate = "/ToggleUserStatus", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CBool ToggleUserStatus(CToggleUserStatusParam param);

        [WebInvoke(UriTemplate = "/GetTicketsByNoOfDigits", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<CTicket> GetTicketsByNoOfDigits(CGetTicketsByNoOfDigitsParam param);


        [WebInvoke(UriTemplate = "/AddSalesBillItems", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CBulkTicketSalesMessage AddSalesBillItems(List<CTicketSalesParam> oSales);

        [WebInvoke(UriTemplate = "/DeleteSalesBillItems", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CBoolMessage DeleteSalesBillItems(List<CDeleteSalesBillItemsParam> oSales);


        [WebInvoke(UriTemplate = "/ReadNextBillNo", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CLong ReadNextBillNo(CReadNextBillNoParam param);
        
        
        //[WebInvoke(UriTemplate = "/GetUsersDealers", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        //List<CUser> GetUsersDealers(CGetUsersDealersParam param);
        [WebInvoke(UriTemplate = "/GetSalesReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRTicketSalesReportReply GetSalesReport(CGetSalesReportParam param);

        [WebInvoke(UriTemplate = "/GetDetailedSalesReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRTicketSalesReportReply GetDetailedSalesReport(CGetSalesReportParam param);

        [WebInvoke(UriTemplate = "/GetWinnersReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRWinnersReportReply GetWinnersReport(CGetWinnersReportParam param);

        [WebInvoke(UriTemplate = "/GetResultReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRResultReportReply GetResultReport(CGetResultReportParam param);

        [WebInvoke(UriTemplate = "/GetProfitnLossReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRProfitnLossReportReply GetProfitnLossReport(CGetProfitnLossReportParam param);

        [WebInvoke(UriTemplate = "/GetProfitnLossBriefReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRProfitnLossReportReply GetProfitnLossBriefReport(CGetProfitnLossReportParam param);

        [WebInvoke(UriTemplate = "/GetDealerNetPaymentReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRProfitnLossReportReply GetDealerNetPaymentReport(CGetProfitnLossReportParam param);

        [WebInvoke(UriTemplate = "/GetDealerNetPaymentBriefReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRProfitnLossReportReply GetDealerNetPaymentBriefReport(CGetProfitnLossReportParam param);


        [WebInvoke(UriTemplate = "/GetSalesDetailedReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRTicketSalesReportReply GetSalesDetailedReport(CGetSalesReportParam param);

        [WebInvoke(UriTemplate = "/GetWinnersDetailedReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRWinnersReportReply GetWinnersDetailedReport(CGetWinnersReportParam param);


        [WebInvoke(UriTemplate = "/GetSalesReportTotal", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CTicketSalesTotal GetSalesReportTotal(CGetSalesReportParam param);

        [WebInvoke(UriTemplate = "/GetWinnersReportTotal", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CWinnersTotal GetWinnersReportTotal(CGetWinnersReportParam param);

        [WebInvoke(UriTemplate = "/GetProfitnLossReportTotal", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CProfitnLossTotal GetProfitnLossReportTotal(CGetProfitnLossReportParam param);

        [WebInvoke(UriTemplate = "/GetDealerNetPaymentReportTotal", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CProfitnLossTotal GetDealerNetPaymentReportTotal(CGetProfitnLossReportParam param);


        [WebInvoke(UriTemplate = "/GetTicketCountReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRTicketCountReportReply GetTicketCountReport(CTicketCountParam param);

        [WebInvoke(UriTemplate = "/GetTicketCountGroupReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRTicketCountGroupReportReply GetTicketCountGroupReport(CTicketCountGroupParam param);


        [WebInvoke(UriTemplate = "/CreateUser", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRUserCreateReply CreateUser(CRCreateUserparam param);

        [WebInvoke(UriTemplate = "/GetUserTicketCommission", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRUserTicketCommissionReply GetUserTicketCommission(CRGetUserTicketCommissionparam param);

        [WebInvoke(UriTemplate = "/DeleteUser", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRUserDeleteReply DeleteUser(CRDeleteUserparam param);

        [WebInvoke(UriTemplate = "/UpdateUser", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRUserUpdateReply UpdateUser(CRUpdateUserparam param);

        [WebInvoke(UriTemplate = "/UpdateUserTicketCommission", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRUpdateUserTicketCommissionReply UpdateUserTicketCommission(CUpdateUserTicketCommissionParam param);

        [WebInvoke(UriTemplate = "/GetDealers", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<CUser> GetDealers(CGetDealersParam param);

        [WebInvoke(UriTemplate = "/GetResults", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRGetResultsReply GetResults(CRGetResultsparam param);

        [WebInvoke(UriTemplate = "/UpdateResults", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRUpdateResultsReply UpdateResults(CUpdateResultsParam param);

        [WebInvoke(UriTemplate = "/GetOutDealers", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<CUser> GetOutDealers(CGetDealersParam param);

        [WebInvoke(UriTemplate = "/SendToOutDealer", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRSendToOutDealerReply SendToOutDealer(CSendToOutDealerParam param);

        [WebInvoke(UriTemplate = "/GetOutDealerSalesReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRTicketSalesReportReply GetOutDealerSalesReport(CGetSalesReportParam param);

        [WebInvoke(UriTemplate = "/GetOutDealerSalesReportTotal", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CTicketSalesTotal GetOutDealerSalesReportTotal(CGetSalesReportParam param);

        [WebInvoke(UriTemplate = "/GetOutDealerWinnersReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRWinnersReportReply GetOutDealerWinnersReport(CGetWinnersReportParam param);

        [WebInvoke(UriTemplate = "/GetOutDealerWinnersReportTotal", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CWinnersTotal GetOutDealerWinnersReportTotal(CGetWinnersReportParam param);

        [WebInvoke(UriTemplate = "/GetOutDealerProfitnLossReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRProfitnLossReportReply GetOutDealerProfitnLossReport(CGetProfitnLossReportParam param);

        [WebInvoke(UriTemplate = "/GetOutDealerProfitnLossBriefReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRProfitnLossReportReply GetOutDealerProfitnLossBriefReport(CGetProfitnLossReportParam param);

        [WebInvoke(UriTemplate = "/GetOutDealerProfitnLossReportTotal", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CProfitnLossTotal GetOutDealerProfitnLossReportTotal(CGetProfitnLossReportParam param);

        [WebInvoke(UriTemplate = "/GetProfitReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRProfitReportReply GetProfitReport(CGetProfitReportParam param);

        [WebInvoke(UriTemplate = "/BlockTicket", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRBlockReply BlockTicket(CBlockParam param);

        [WebInvoke(UriTemplate = "/UnBlockTicket", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRBlockReply UnBlockTicket(CBlockParam param);

        [WebInvoke(UriTemplate = "/GetTicketCountBalanceReport", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRTicketCountBalanceReportReply GetTicketCountBalanceReport(CTicketCountParam param);


        [WebInvoke(UriTemplate = "/GetTicketlimits", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<CTicketLimit> GetTicketlimits(CRGetLimitsParam param);

        [WebInvoke(UriTemplate = "/CreateTicketLimit", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRLimitCreateReply CreateTicketLimit(CRCreateLimitParam param);

        [WebInvoke(UriTemplate = "/DeleteTicketLimit", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRLimitDeleteReply DeleteTicketLimit(CRDeleteLimitParam param);

        [WebInvoke(UriTemplate = "/UpdateTicketLimit", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRLimitUpdateReply UpdateTicketLimit(CRUpdateLimitParam param);



        [WebInvoke(UriTemplate = "/GetOutDealerTicketFilters", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<COutDealerTicketFilter> GetOutDealerTicketFilters(CRGetOutDealerTicketFiltersParam param);

        [WebInvoke(UriTemplate = "/CreateOutDealerTicketFilter", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRFilterCreateReply CreateOutDealerTicketFilter(CRCreateOutDealerTicketFilterParam param);

        [WebInvoke(UriTemplate = "/DeleteOutDealerTicketFilter", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRFilterDeleteReply DeleteOutDealerTicketFilter(CRDeleteOutDealerTicketFilterParam param);

        [WebInvoke(UriTemplate = "/UpdateOutDealerTicketFilter", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRFilterUpdateReply UpdateOutDealerTicketFilter(CRUpdateOutDealerTicketFilterParam param);


        [WebInvoke(UriTemplate = "/GetSessions", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        List<CRSessionStatus> GetSessions(CGetSessionsParam param);

        [WebInvoke(UriTemplate = "/RemoveSession", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRRemoveSessionReply RemoveSession(CRemoveSessionParam param);


        [WebInvoke(UriTemplate = "/DeleteOutDealerSalesBillItems", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CBoolMessage DeleteOutDealerSalesBillItems(List<CDeleteSalesBillItemsParam> oSales);


        [WebInvoke(UriTemplate = "/GetSummaryResults", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        CRGetResultsReply GetSummaryResults(CRGetSummaryResultsparam param);*/
    }

    [DataContract]
    public class CTicketLimit
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public string TicketName { get; set; }
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public int SerialNo { get; set; }

    }

    [DataContract]
    public class COutDealerTicketFilter
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public string InTicketIds { get; set; }
        [DataMember]
        public string InTicketNames { get; set; }
        [DataMember]
        public long OutTicketId { get; set; }
        [DataMember]
        public string OutTicketName { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public string Condition { get; set; }
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public long CompanyId { get; set; }

    }

    [DataContract]
    public class CRSessionStatus
    {
        public long SerialNo { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long ParentUserId { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsActive { get; set; }        
    }

    [DataContract]
    public class CRDeleteUserparam
    {
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRDeleteLimitParam
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRDeleteOutDealerTicketFilterParam
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRDeleteOutDealerTicketFilterparam
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRUpdateUserparam
    {
        [DataMember]
        public long UserId { get; set; }        
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRUpdateLimitParam
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRUpdateOutDealerTicketFilterParam
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public string InTicketIds { get; set; }
        [DataMember]
        public string InTicketNames { get; set; }
        [DataMember]
        public long OutTicketId { get; set; }
        [DataMember]
        public string OutTicketName { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public string Condition { get; set; }
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRSendToOutDealerReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class CRUpdateResultsReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class CRUpdateUserTicketCommissionReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    public class CUpdateResultParam
    {
        [DataMember]
        public long PrizeId { get; set; }
        [DataMember]
        public string TicketNo { get; set; }

    }

    public class CCommissionParam
    {
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public double Commission { get; set; }        

    }

    public class CUpdateUserTicketCommissionParam
    {
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public List<CCommissionParam> Commissions { get; set; }

    }

    public class CSendToOutDealerParam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public long OutDealerId { get; set; }

    }

    public class CUpdateResultsParam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public List<CUpdateResultParam> Results { get; set; }

    }

    [DataContract]
    public class CRGetResultReport
    {
        [DataMember]
        public long PrizeId { get; set; }
        [DataMember]
        public string TicketNo { get; set; }
        [DataMember]
        public int SerialNo { get; set; }        

    }

    [DataContract]
    public class CRUserTicketCommissionReport
    {
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public string TicketName { get; set; }
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public double Commission { get; set; }

    }

    [DataContract]
    public class CRGetResultsReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public List<CRGetResultReport> Report { get; set; }
    }

    [DataContract]
    public class CRUserTicketCommissionReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public List<CRUserTicketCommissionReport> Report { get; set; }
    }

    [DataContract]
    public class CRGetResultsparam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }


    [DataContract]
    public class CRGetSummaryResultsparam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

        [DataMember]
        public int Day { get; set; }
        [DataMember]
        public int Month { get; set; }
        [DataMember]
        public int Year { get; set; }
    }
    [DataContract]
    public class CRGetUserTicketCommissionparam
    {
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRCreateUserparam
    {
        [DataMember]
        public long ParentUserId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRCreateLimitParam
    {
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public int SerialNo { get; set; }

    }

    [DataContract]
    public class CRCreateOutDealerTicketFilterParam
    {
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public string InTicketIds { get; set; }
        [DataMember]
        public string InTicketNames { get; set; }
        [DataMember]
        public long OutTicketId { get; set; }
        [DataMember]
        public string OutTicketName { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public string Condition { get; set; }
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }

    }

    [DataContract]
    public class CRTicketCountReport
    {
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public string TicketName { get; set; }
        [DataMember]
        public string TicketNo { get; set; }
        [DataMember]
        public long? Count { get; set; }
        [DataMember]
        public bool BlockStatus { get; set; }

    }

    [DataContract]
    public class CRTicketCountBalanceReport
    {
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public string TicketName { get; set; }
        [DataMember]
        public string TicketNo { get; set; }
        [DataMember]
        public long? InCount { get; set; }
        [DataMember]
        public long? OutCount { get; set; }
        [DataMember]
        public long? BalanceCount { get; set; }
        [DataMember]
        public bool BlockStatus { get; set; }

    }


    [DataContract]
    public class CRTicketCountGroupReport
    {
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public string TicketNo { get; set; }
        [DataMember]
        public long? Count { get; set; }
    }

    [DataContract]
    public class CRTicketCountReportReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public List<CRTicketCountReport> Report { get; set; }
    }

    [DataContract]
    public class CRTicketCountBalanceReportReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public List<CRTicketCountBalanceReport> Report { get; set; }
    }

    [DataContract]
    public class CRTicketCountGroupReportReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public List<CRTicketCountGroupReport> Report { get; set; }
    }

    public class CTicketCountParam
    {
        [DataMember]
        public int Day { get; set; }
        [DataMember]
        public int Month { get; set; }
        [DataMember]
        public int Year { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public int Sign { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public string TicketNo { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public int Select { get; set; }
        [DataMember]
        public long UserId { get; set; }
    }

    public class CTicketCountGroupParam
    {
        [DataMember]
        public int Day { get; set; }
        [DataMember]
        public int Month { get; set; }
        [DataMember]
        public int Year { get; set; }
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public int Sign { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public string TicketNo { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public int Select { get; set; }
        [DataMember]
        public long UserId { get; set; }
    }

    [DataContract]
    public class CReadNextBillNoParam
    {
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
    }

    [DataContract]
    public class CGetTicketsByNoOfDigitsParam
    {        
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CGetTicketsParam
    {        
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CGetUserNSubUsersParam
    {
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        //[DataMember]
        //public int UserType { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CGetDealersParam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CGetSubUsersParam
    {
        [DataMember]
        public long RequestUserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CGetSessionsParam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CRemoveSessionParam
    {
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CRGetLimitsParam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CRGetOutDealerTicketFiltersParam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public long OutdealerId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CToggleUserStatusParam
    {
        [DataMember]
        public long RequestUserId { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public long CompanyId { get; set; }        
        [DataMember]
        public string SessionId { get; set; }
    }

    public class CBlockParam
    {
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public string TicketNo { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public int Day { get; set; }
        [DataMember]
        public int Month { get; set; }
        [DataMember]
        public int Year { get; set; }
    }

    public class CGetProfitReportParam
    {
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public int FromDay { get; set; }
        [DataMember]
        public int FromMonth { get; set; }
        [DataMember]
        public int FromYear { get; set; }
        [DataMember]
        public int ToDay { get; set; }
        [DataMember]
        public int ToMonth { get; set; }
        [DataMember]
        public int ToYear { get; set; }
        [DataMember]
        public string SessionId { get; set; }        
    }

    [DataContract]
    public class CGetProfitnLossReportParam
    {        
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public int FromDay { get; set; }
        [DataMember]
        public int FromMonth { get; set; }
        [DataMember]
        public int FromYear { get; set; }
        [DataMember]
        public int ToDay { get; set; }
        [DataMember]
        public int ToMonth { get; set; }
        [DataMember]
        public int ToYear { get; set; }
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public long RequestUserId { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public int LimitIndex { get; set; }
        [DataMember]
        public int LimitSize { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public int Select { get; set; }
    }

    [DataContract]
    public class CGetResultReportParam
    {        
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public int Day { get; set; }
        [DataMember]
        public int Month { get; set; }
        [DataMember]
        public int Year { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public string SessionId { get; set; }
    }

    [DataContract]
    public class CDeleteSalesBillItemsParam
    {        
        [DataMember]
        public long BillId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public long BillNo { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public long RequestUserId { get; set; }
    }    

    [DataContract]
    public class CGetUsersDealersParam
    {
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
    }

    [DataContract]
    public class CGetSalesReportParam
    {        

        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public long ParentUserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public int FromDay { get; set; }
        [DataMember]
        public int FromMonth { get; set; }
        [DataMember]
        public int FromYear { get; set; }
        [DataMember]
        public int ToDay { get; set; }
        [DataMember]
        public int ToMonth { get; set; }
        [DataMember]
        public int ToYear { get; set; }
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public long RequestUserId { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public int LimitIndex { get; set; }
        [DataMember]
        public int LimitSize { get; set; }
        [DataMember]
        public string SessionId { get; set; }

        [DataMember]
        public int Select { get; set; }
    }

    [DataContract]
    public class CGetWinnersReportParam
    {        
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public int FromDay { get; set; }
        [DataMember]
        public int FromMonth { get; set; }
        [DataMember]
        public int FromYear { get; set; }
        [DataMember]
        public int ToDay { get; set; }
        [DataMember]
        public int ToMonth { get; set; }
        [DataMember]
        public int ToYear { get; set; }
        [DataMember]
        public int NoOfDigits { get; set; }
        [DataMember]
        public long TicketId { get; set; }
        [DataMember]
        public long RequestUserId { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public int LimitIndex { get; set; }
        [DataMember]
        public int LimitSize { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public int Select { get; set; }

    }
}
