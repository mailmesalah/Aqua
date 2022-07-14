using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerServiceInterface.RestClasses
{

    [DataContract]
    public class CRUserDeleteReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class CRLimitDeleteReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class CRFilterDeleteReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class CRUserUpdateReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }        
    }

    [DataContract]
    public class CRLimitUpdateReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class CRFilterUpdateReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }


    [DataContract]
    public class CRRemoveSessionReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
    }


    [DataContract]
    public class CRUserCreateReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public long UserId { get; set; }
    }

    [DataContract]
    public class CRLimitCreateReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class CRFilterCreateReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public long Id { get; set; }
    }

    [DataContract]
    public class CRTicketSalesReportReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CRTicketSalesReport> Report { get; set; }
    }

    [DataContract]
    public class CRTicketSalesReport
    {
        [DataMember]
        public long BillId { get; set; }
        [DataMember]
        public int BillYear { get; set; }
        [DataMember]
        public int BillMonth { get; set; }
        [DataMember]
        public int BillDay { get; set; }
        [DataMember]
        public long BillNo { get; set; }
        [DataMember]
        public int SerialNo { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string TicketName { get; set; }
        [DataMember]
        public string TicketNo { get; set; }
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public double Amount { get; set; }
        [DataMember]
        public long UserId { get; set; }        
    }
}
