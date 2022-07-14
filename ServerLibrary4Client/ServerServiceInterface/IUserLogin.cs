using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ServerServiceInterface
{
    [ServiceContract]
    public interface IUserLogin
    {
        [OperationContract]
        CServerMessage GetLoginDetails(string companyUsername, string username, string password, string clientDevice);
    }    
}
