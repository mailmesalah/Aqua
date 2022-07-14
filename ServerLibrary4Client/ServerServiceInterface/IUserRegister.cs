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
    public interface IUserRegister
    {
        [OperationContract]
        CBoolMessage DeleteUser(long userId, long companyId, string sessionId);
        [OperationContract]
        CBoolMessage CreateUser(CUser user, long companyId, string sessionId);
        [OperationContract]
        CBoolMessage UpdateUser(CUser user, long companyId, string sessionId);
        [OperationContract]
        CUser ReadUser(long userId, long companyId, string sessionId);
        [OperationContract]
        List<CUser> ReadAllUsers(long companyId, string sessionId);        
    }


    [DataContract]
    public class CUser
    {
        //User Type
        public const int SUPER_ADMIN = 0;
        public const int USER = 1;

        [DataMember]
        public long CompanyId { get; set; }
        [DataMember]
        public int UserType { get; set; }
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
    }    
}
