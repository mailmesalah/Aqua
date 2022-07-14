using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerServiceInterface.RestClasses
{

    [DataContract]
    public class CRResultReportReply
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public List<CRResultReport> Report { get; set; }
    }

    [DataContract]
    public class CRResultReport
    {       
        [DataMember]
        public string TicketName { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public string PrizeName { get; set; }
        [DataMember]
        public string Mask { get; set; }
    }
}
