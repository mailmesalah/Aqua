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
    class SettingsService : ISettings
    {
        public const string START_MONTH = "START_MONTH";
        public const string START_DAY = "START_DAY";
        public const string END_MONTH = "END_MONTH";
        public const string END_DAY = "END_DAY";
        
        private static readonly object @billNoLock = new object();

        public long GetFinancialCode(DateTime date, long companyId)
        {            
            try
            {
                
                int startMonth = 0, endMonth = 0;
                int startDay = 0, endDay = 0;

                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        var ds = dataB.DefaultValues.Select(c => c).Where(x => x.CompanyId == companyId);
                        if (ds != null && ds.Count() > 0)
                        {
                            foreach (var i in ds)
                            {
                                    switch (i.Name)
                                    {
                                        case START_MONTH:
                                            startMonth = i.IntValue;
                                            break;
                                        case START_DAY:
                                            startDay = i.IntValue;
                                            break;
                                        case END_MONTH:
                                            endMonth = i.IntValue;
                                            break;
                                        case END_DAY:
                                            endDay = i.IntValue;
                                            break;                                       
                                    }
                            }
                        }
                    }
                    catch { }
                }

                int m = date.Month;
                int d = date.Day;

                if (m == 0 && d == 0)
                {
                    //Date Selected from DateTime Picker So consider the Start Time for time
                    m = endMonth;
                    d = endDay;
                }

                double curDateD = Double.Parse(string.Format("{0:00}" ,m) + "." + string.Format("{0:00}", d));
                double startDateD = Double.Parse(string.Format("{0:00}", startMonth) + "." + string.Format("{0:00}", startDay));
                double endDateD = Double.Parse(string.Format("{0:00}", endMonth) + "." + string.Format("{0:00}", endDay));

                if (startDateD > endDateD)
                {
                    if (curDateD >= startDateD)
                    {
                        date = date.AddYears(1);
                        return long.Parse(string.Format("{0}", date.Year));
                    }
                    else if (curDateD <= endDateD)
                    {
                        return long.Parse(string.Format("{0}", date.Year));
                    }
                    else
                    {
                        //Date time is out of financialcode range
                        return -1;
                    }
                }
                else if (startDateD < endDateD)
                {
                    if (curDateD <= endDateD && curDateD >= startDateD)
                    {
                        return long.Parse(string.Format("{0}", date.Year));
                    }
                    else
                    {
                        //Date time is out of financialcode range
                        return -1;
                    }
                }
                else
                {
                    //Date time is out of financialcode range
                    return -1;
                }
            }
            catch
            {
                return -1;
            }       
        }

        public long GetFinancialCode(long companyId)
        {
            return GetFinancialCode(System.DateTime.Now, companyId);
        }

        public bool IsInsideWorkingTimePeriod(DateTime dt, long companyId)
        {
            return GetFinancialCode(dt, companyId) != -1;
        }

        public CBoolMessage UpdateDefaultValues(CDefaultValues oDef, long companyId, string sessionId)
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

                //lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            var ds = dataB.DefaultValues.Select(c => c).Where(x => x.CompanyId == companyId);
                            if (ds != null && ds.Count() > 0)
                            {
                                foreach (var i in ds)
                                {

                                    switch (i.Name)
                                    {
                                        case START_MONTH:
                                            i.IntValue = oDef.StartMonth;
                                            break;
                                        case START_DAY:
                                            i.IntValue = oDef.StartDay;
                                            break;
                                        case END_MONTH:
                                            i.IntValue = oDef.EndMonth;
                                            break;
                                        case END_DAY:
                                            i.IntValue = oDef.EndDay;
                                            break;
                                    }

                                }
                            }

                            dataB.SaveChanges();
                            //Success
                            cbm.IsSuccess = true;

                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            cbm.IsSuccess = false;
                            cbm.Message = e.Message;

                            dataBTransaction.Rollback();
                        }
                        finally
                        {

                        }
                    }
                }
            }
            catch (Exception e) { cbm.Message = e.Message; }
            return cbm;
        }

        public CDefaultValues ReadDefaultValues(long companyId)
        {
            CDefaultValues oDef = new CDefaultValues();
            try
            {
                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        var ds = dataB.DefaultValues.Select(c => c).Where(x => x.CompanyId == companyId);
                        if (ds != null && ds.Count() > 0)
                        {
                            foreach (var i in ds)
                            {

                                switch (i.Name)
                                {
                                    case START_MONTH:
                                        oDef.StartMonth = i.IntValue;
                                        break;
                                    case START_DAY:
                                        oDef.StartDay = i.IntValue;
                                        break;
                                    case END_MONTH:
                                        oDef.EndMonth = i.IntValue;
                                        break;
                                    case END_DAY:
                                        oDef.EndDay = i.IntValue;
                                        break;

                                }

                            }

                        }
                        else
                        {
                            //lock (Synchronizer.@lock)
                            {
                                var dataBTransaction = dataB.Database.BeginTransaction();
                                try
                                {
                                    DefaultValue d = dataB.DefaultValues.Create();
                                    d.IntValue = 4;
                                    d.CompanyId = companyId;
                                    d.Name = START_MONTH;
                                    dataB.DefaultValues.Add(d);
                                    dataB.SaveChanges();

                                    d = dataB.DefaultValues.Create();
                                    d.IntValue = 1;
                                    d.CompanyId = companyId;
                                    d.Name = START_DAY;
                                    dataB.DefaultValues.Add(d);
                                    dataB.SaveChanges();

                                    d = dataB.DefaultValues.Create();
                                    d.IntValue = 3;
                                    d.CompanyId = companyId;
                                    d.Name = END_MONTH;
                                    dataB.DefaultValues.Add(d);
                                    dataB.SaveChanges();

                                    d = dataB.DefaultValues.Create();
                                    d.IntValue = 31;
                                    d.CompanyId = companyId;
                                    d.Name = END_DAY;
                                    dataB.DefaultValues.Add(d);
                                    dataB.SaveChanges();

                                    //Success

                                    dataBTransaction.Commit();
                                }
                                catch
                                {
                                    dataBTransaction.Rollback();
                                }
                            }

                            oDef = ReadDefaultValues(companyId);
                        }


                    }
                    catch
                    {

                    }
                }
            }
            catch { }
            return oDef;
        }

        public bool IsSessionLocked(long companyId, string type)
        {
            bool rVal = false;
            try
            {
                using (var dataB = new AquaStorage())
                {
                    try
                    {
                        var ds = dataB.DefaultValues.Select(c => c).Where<DefaultValue>(x => x.CompanyId == companyId && x.Name.Equals(type)).FirstOrDefault();
                        if (ds != null)
                        {
                            rVal = ds.IntValue == 1;                                                          
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch { }
            return rVal;
        }
    }
}
