using ServerServiceInterface;
using System;
using System.Linq;
using AquaServer.StorageModel;
using System.ServiceModel;
using AquaServer.Local;

namespace AquaServer.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    public class UserLoginService : IUserLogin
    {
        public CServerMessage GetLoginDetails(string companyUsername, string username, string password, string clientDevice)
        {
            CClientLocalInfo rculi = new CClientLocalInfo();
            rculi.ReturnObjectHint = CServerMessage.CClientLocalInfo;

            try
            {
                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        var company = dataB.Companies.Where<Company>(sup => sup.ExpiryDate>DateTime.Now && sup.IsActive==true && sup.CompanyCode.Equals(companyUsername)).FirstOrDefault();
                        if (company != null)
                        {
                            var userData = dataB.Users.Where<User>(user => (user.CompanyId == company.Id) && (user.Username == username) && (user.Password == password)).FirstOrDefault();
                            if (userData != null)
                            {
                                
                                rculi.ClientDevice = clientDevice;
                                rculi.CompanyId = company.Id;
                                rculi.CompanyUsername = company.CompanyCode;
                                rculi.Username = userData.Username;
                                rculi.Password = userData.Password;
                                rculi.UserType = userData.UserType;
                                rculi.UserId = userData.Id;
                                rculi.Name = userData.Name;

                                if (rculi.ClientDeviceCode == 0)
                                {
                                    BillNoGeneratorService bgs = new BillNoGeneratorService();
                                    long code = bgs.GetNextBillNo(company.Id, BillNoGeneratorService.DEVICE_CODE);
                                    bgs.SetBillNo(company.Id, BillNoGeneratorService.DEVICE_CODE, code);
                                    rculi.ClientDeviceCode = code;
                                }
                                rculi.IsSuccess=true;
                                rculi.Message = "Successful Login";

                                //Create Session for this Login
                                CSession ses = new CSession();
                                string session = ses.GetNewSession(userData.Id);
                                if (ses.SetSession(userData.Id, userData.UserType, session, CSession.DEVICE_COMPUTER))
                                {
                                    rculi.SessionId = session;
                                }
                                else
                                {
                                    rculi.IsSuccess = false;
                                    rculi.Message = "Session Error";
                                }
                            }
                            else
                            {
                                rculi.IsSuccess = false;
                                rculi.Message = "Username or Password does not Exist";
                            }
                        }                        
                        else
                        {                            
                            rculi.IsSuccess = false;
                            rculi.Message = "Not Registered";                            
                        }
                        
                    }
                    catch(Exception e)
                    {                        
                        rculi.IsSuccess = false;
                        rculi.Message = "Error "+e.Message;                        
                    }
                }
            }
            catch(Exception e)
            {                
                rculi.IsSuccess = false;
                rculi.Message = "Error "+ e.Message;                
            }

            return rculi;
        }
        
    }
    
}
