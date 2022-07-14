using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace ServerServiceInterface
{
    [ServiceContract]
    public interface IAccountRegister
    {
        [OperationContract]
        CBoolMessage DeleteAccount(long AccountId, long companyId, string sessionId);
        [OperationContract]
        CBoolMessage CreateAccount(CAccount Account, long companyId, string sessionId);
        [OperationContract]
        CBoolMessage UpdateAccount(CAccount Account, long companyId, string sessionId);
        [OperationContract]
        List<CAccount> ReadAllAccounts(long companyId, string sessionId);
        [OperationContract]
        List<CAccount> ReadAllAccountTypes(long companyId, string sessionId);
        [OperationContract]
        List<CAccount> ReadAllAccountsUnderMainGroup(int MainGroup, long companyId, string sessionId);
        [OperationContract]
        List<CAccount> ReadAllParentGroupsUnderMainGroup(int MainGroup, long companyId, string sessionId);
        [OperationContract]
        List<CAccount> ReadAllAccountsUnderParentGroup(long ParentGroupId, long companyId, string sessionId);
        [OperationContract]
        CAccount ReadAccount(long AccountId, long companyId, string sessionId);
        
    }

    
    [DataContract]
    public class CAccount
    {
        //Account Group
        public const int MONETARY_ACCOUNT = 0;
        public const int PERSONAL_ACCOUNT = 1;
        public const int STOCK_ACCOUNT = 2;
        public const int CREDIT_ACCOUNT = 3;
        //Account Type
        public const int GROUP = 0;
        public const int ACCOUNT = 1;

        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public long CompanyId { get; set; }        
        [DataMember]
        public int AccountType { get; set; }
        [DataMember]
        public int MainGroup { get; set; }
        [DataMember]
        public long ParentGroupId { get; set; }        
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ShortName { get; set; }
        [DataMember]
        public string Details1 { get; set; }
        [DataMember]
        public string Details2 { get; set; }
        [DataMember]
        public string Details3 { get; set; }        
    }

    
}
