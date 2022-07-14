using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerServiceInterface.RestClasses
{
    [DataContract]
    public class CRBlockReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }        
    }

    [DataContract]
    public class CRProfitReportReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CRProfitReport> Report { get; set; }
    }

    [DataContract]
    public class CRProfitReport
    {
        [DataMember]
        public long? DayCode { get; set; }        
        [DataMember]
        public double? SalesAmount { get; set; }
        [DataMember]
        public double? SalesWinningAmount { get; set; }
        [DataMember]
        public double? OutDealerSalesAmount { get; set; }
        [DataMember]
        public double? OutDealerWinningAmount { get; set; }
    }

    [DataContract]
    public class CRProfitnLossReportReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CRProfitnLossReport> Report { get; set; }
    }

    [DataContract]
    public class CRProfitnLossReport
    {       
        [DataMember]
        public long? DayCode { get; set; }        
        [DataMember]
        public string UserName { get; set; }      
        [DataMember]
        public double? WinningAmount { get; set; }
        [DataMember]
        public double? SalesAmount { get; set; }
    }
}
