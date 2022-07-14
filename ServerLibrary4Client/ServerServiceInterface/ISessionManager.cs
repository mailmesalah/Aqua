using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServerServiceInterface
{    
    [ServiceContract]
    public interface ISessionManager
    {        
        [OperationContract]
        CBoolMessage RemoveSession(long userId, long companyId, string sessionId);        
        [OperationContract]
        List<CSessionStatus> ReadSessionStatus(long companyId, string sessionId);      
    }
    
    [DataContract]
    public class CSessionStatus
    {
        public long SerialNo { get; set; }
        [DataMember]
        public long UserId { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
        public string Status { get; set; }
    }    
}
