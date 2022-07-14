using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerServiceInterface
{
    [DataContract]
    [KnownType(typeof(CClientLocalInfo))]
    [KnownType(typeof(CBoolMessage))]
    [KnownType(typeof(CBulkTicketSalesMessage))]
    public abstract class CServerMessage
    {
        public const int CNull = 0;
        public const int CClientLocalInfo = 1;
        public const int CBoolMessage = 2;

        [DataMember]
        public string Message{get;set;}
        [DataMember]
        public bool IsSuccess { get; set; }
        [DataMember]
        public int ReturnObjectHint { get; set; }
    }

    [DataContract]
    public class CSendToOutDealerMessage : CServerMessage
    {
        [DataMember]
        public string BlockedTicketMessage { get; set; }
        [DataMember]
        public string OutDealerMessage { get; set; }
        [DataMember]
        public string OutDealerSalesMessage { get; set; }
    }

    [DataContract]
    public class CClientLocalInfo:CServerMessage
    {
        public const int SUPER_ADMIN = 0;
        public const int ADMIN = 1;
        public const int DEALER = 2;
        public const int SUB_DEALER = 3;        

        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public string CompanyUsername { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public long ParentUserId { get; set; }
        [DataMember]
        public string ClientDevice { get; set; }
        [DataMember]
        public long ClientDeviceCode { get; set; }
        [DataMember]
        public string SessionId { get; set; }
        [DataMember]
        public double ApkVersion { get; set; }
    }

    [DataContract]
    public class CBoolMessage : CServerMessage
    {
        
    }

    [DataContract]
    public class CBulkTicketSalesMessage : CServerMessage
    {
        public const int SUCCESS = 2;
        public const int PARTIAL = 3;
        public const int FAILED = 4;
        [DataMember]
        public string BlockedNumbers = "";
        [DataMember]
        public string DisplayMessage = "";
        [DataMember]
        public int SuccessScale = 0;
    }

    [DataContract]
    public class CTransactionMessage : CServerMessage
    {
        /*[DataMember]
        public long Id { get; set; }
        [DataMember]
        public double Amount { get; set; }*/
    }

    [DataContract]
    public class CDealerTicketSettingsMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class CTicketPrizeSettingsMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class CPaymentMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }        
    }

    [DataContract]
    public class CReceiptMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class COutDealerMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class CTicketLimitMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class CDealerTicketLimitMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class CDealerHoldingLimitMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class COutDealerSalesMessage : CServerMessage
    {        
        [DataMember]
        public long Id { get; set; }     
    }

    [DataContract]
    public class CBlockedMessage : CServerMessage
    {
        [DataMember]
        public long Id { get; set; }
    }

    public class COutTicketSalesMessage : CServerMessage
    {
        public Dictionary<long, double> TicketRates = new Dictionary<long, double>();
        public List<CBlockedNumber> BlockedNumbers = new List<CBlockedNumber>();

    }

    public class CBlockedNumber
    {
        public long TicketId { get; set; }
        public string TicketNo { get; set; }
    }
}
