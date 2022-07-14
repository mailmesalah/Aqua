using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerServiceInterface.RestClasses
{

    [DataContract]
    public class CRWinnersReportReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CRWinnersReport> Report { get; set; }
    }

    [DataContract]
    public class CRWinnersReport
    {       
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
        public string PrizeName { get; set; }       
        [DataMember]
        public string Mask { get; set; }        
        [DataMember]
        public double PrizeAmount { get; set; }
        [DataMember]
        public double Commission { get; set; }
    }
}
