using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServerServiceInterface
{    
    [ServiceContract]
    public interface ISettings
    {        
        [OperationContract]
        CBoolMessage UpdateDefaultValues(CDefaultValues oReceipt, long companyId, string sessionId);        
        [OperationContract]
        CDefaultValues ReadDefaultValues(long companyId);      
    }
    
    [DataContract]
    public class CDefaultValues
    {      
        [DataMember]
        public int StartMonth { get; set; }
        [DataMember]
        public int StartDay { get; set; }  
        [DataMember]
        public int EndMonth { get; set; }
        [DataMember]
        public int EndDay { get; set; }        
    }    
}
