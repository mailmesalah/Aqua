using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AquaServer.StorageModel;

namespace AquaServer.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    class BillNoGeneratorService
    {
        public const int GENERAL = 0;
        public const int DEVICE_CODE = 1;
        public const int ENTRY_BILLNO = 2;
        
        private static readonly object @billNoLock = new object();

     
        public long GetNextBillNo(long companyId, int billType)
        {
            long billNo = 1;
            try
            {
                /*Add Day code for bill no generation in next update
                SettingsService ss = new SettingsService();
                long dayCode = ss.GetDayCode(companyId);
                */

                using (var dataB = new AquaStorage())
                {
                    var val = dataB.BillNos.Where(billno=>billno.BillType==billType && billno.CompanyId==companyId).FirstOrDefault();

                    if (val == null)//Create a row 
                    {
                        lock (@billNoLock)
                        {
                            BillNoGenerator bn = dataB.BillNos.Create();
                            bn.BillNo = 1;
                            bn.BillType = billType;
                            bn.CompanyId = companyId;                            
                            dataB.BillNos.Add(bn);
                            dataB.SaveChanges();
                        }
                    }
                    else
                    {
                        billNo = val.BillNo;
                    }
                }
            }
            catch
            {

            }
            return billNo;
        }


        public bool SetBillNo(long companyId, int billType, long billNo)
        {
            bool success = true;
            try
            {
                lock (@billNoLock)
                {

                    using (var dataB = new AquaStorage())
                    {
                        var val = dataB.BillNos.Where(billno => billno.BillType == billType && billno.CompanyId == companyId).FirstOrDefault();

                        if (val == null)//Create a row for new Financial year
                        {
                            BillNoGenerator bn = dataB.BillNos.Create();
                            bn.BillNo = 1;
                            bn.BillType = billType;
                            bn.CompanyId = companyId;                            
                            dataB.BillNos.Add(bn);
                        }
                        else//Or Edit
                        {
                            val.BillNo = billNo;
                        }

                        dataB.SaveChanges();
                    }
                }
            }
            catch
            {
                success = false;
            }
            return success;
        }

    }
}
