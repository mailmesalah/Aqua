using MySql.Data.MySqlClient;
using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AquaServer.General;
using AquaServer.Local;
using AquaServer.StorageModel;

namespace AquaServer.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    class SessionManagerService : ISessionManager
    {        
        
        public CBoolMessage RemoveSession(long userId, long companyId, string sessionId)
        {
            CBoolMessage cbm = new CBoolMessage();
            cbm.IsSuccess = false;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null || si.UserType != CUser.SUPER_ADMIN)
            {                
                cbm.Message = "Session Error";
                return cbm;
            }

            try
            {
                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        string sQuery = "Delete From Sessions Where (Sessions.UserId=@userId && Sessions.DeviceType=" + CSession.DEVICE_MOBILE + ")";
                        long rows = dataB.Database.ExecuteSqlCommand(sQuery,
                            new MySqlParameter("@userId", userId));
                        if (rows > 0)
                        {
                            cbm.Message = "Successfully Removed Session";
                            cbm.IsSuccess = true;
                        }
                        else
                        {
                            cbm.Message = "Could Not Delete";
                            cbm.IsSuccess = false;
                        }
                    }
                    catch(Exception e)
                    {
                        cbm.Message = e.Message;
                        cbm.IsSuccess = false;
                    }
                }
            }
            catch (Exception e)
            {
                cbm.Message = e.Message;
                cbm.IsSuccess = false;
            }

            return cbm;
        }

        public List<CSessionStatus> ReadSessionStatus(long companyId, string sessionId)
        {
            List<CSessionStatus> sessions=null;

            CSession sesObj = new CSession();
            Session si = sesObj.GetSession(sessionId);
            if (si == null || si.UserType != CUser.SUPER_ADMIN)
            {
                return sessions;
            }
            try
            {               
                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        string sQuery = "Select U.Id As UserId, U.UserType, U.Name, (Select Count(*) From Sessions Where Sessions.UserId=U.Id && Sessions.DeviceType="+CSession.DEVICE_MOBILE+")>0 As IsActive From Users U Where (U.CompanyId=@companyId && U.UserType In ("+CUser.SUPER_ADMIN+ "," + CUser.USER + ")) Order By U.UserType, U.Name ";
                        sessions = dataB.Database.SqlQuery<CSessionStatus>(sQuery,
                            new MySqlParameter("@companyId", companyId)).ToList();
                    }
                    catch { }
                }
            }catch { }
            return sessions;
        }
            
    }
}
