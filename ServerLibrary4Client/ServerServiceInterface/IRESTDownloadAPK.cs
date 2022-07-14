using ServerServiceInterface.RestClasses;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ServerServiceInterface
{
    [ServiceContract]
    public interface IRESTDownloadAPK
    {
        [OperationContract]        
        [WebGet]
        Stream DownloadApk();


    }
    
}
