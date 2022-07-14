using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AquaServer.General;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AquaServer.StorageModel;
using AquaServer.Services;
using System.Diagnostics;
using System.Threading;

namespace AquaServer.Local
{
    public class CompanyBackend
    {
        public CBoolMessage CreateCompany(CCompany oCompany)
        {
            CBoolMessage cbm = new CBoolMessage();
            cbm.IsSuccess = false;
            try
            {
                if (IsCompanyUsernameAlreadyUsed(oCompany.CompanyUsername))
                {
                    cbm.Message = "Company Username Already Exist";
                    return cbm;
                }

                lock (Synchronizer.@lock)
                {

                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {

                            Company c = dataB.Companies.Create();
                            c.CompanyCode = oCompany.CompanyUsername;
                            c.Name = oCompany.Company;
                            c.ExpiryDate = oCompany.Expiry;
                            c.IsActive = oCompany.IsActive;
                            dataB.Companies.Add(c);

                            dataB.SaveChanges();
                            //Saving the Company Id
                            oCompany.Id = c.Id;

                            //Create Super Admin
                            User u = dataB.Users.Create();
                            u.CompanyId = oCompany.Id;
                            u.Name = oCompany.AdminName;
                            u.Username = oCompany.AdminUsername;
                            u.Password = oCompany.AdminPassword;
                            u.UserType = CUser.SUPER_ADMIN;
                            dataB.Users.Add(u);

                            long companyId = c.Id;


                            /*Ticket lsksuper = dataB.Tickets.Create();
                            lsksuper.CompanyId = companyId;
                            lsksuper.Name = "SUPER";
                            lsksuper.NoOfDigits = 2;
                            lsksuper.Mask = CTicket.MASK_ABC;
                            lsksuper.Type = CTicket.STRAIGHT;
                            lsksuper.IsActive = true;
                            lsksuper.Cost = 10;
                            dataB.Tickets.Add(lsksuper);
                            //End of Ticket Creation
                            */

                            //Save All
                            dataB.SaveChanges();

                            //Loading Default Settings
                            SettingsService ss = new SettingsService();
                            ss.ReadDefaultValues(companyId);

                            
                            //Success
                            cbm.IsSuccess = true;

                            dataBTransaction.Commit();
                        }
                        catch (Exception e)
                        {
                            cbm.IsSuccess = false;
                            cbm.Message = e.Message + " " + e.InnerException;

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

        public CBoolMessage UpdateCompany(CCompany oCompany)
        {
            CBoolMessage cbm = new CBoolMessage();
            cbm.IsSuccess = false;

            try
            {

                if (IsCompanyUsernameAlreadyUsed(oCompany.Id, oCompany.CompanyUsername))
                {
                    cbm.Message = "Company Username already Exist";
                    return cbm;
                }

                lock (Synchronizer.@lock)
                {
                    using (var dataB = new AquaStorage())
                    {
                        var dataBTransaction = dataB.Database.BeginTransaction();
                        try
                        {
                            Company c = dataB.Companies.Select(x => x).Where(x => x.Id == oCompany.Id).FirstOrDefault();
                            c.Name = oCompany.Company;
                            c.CompanyCode = oCompany.CompanyUsername;
                            c.ExpiryDate = oCompany.Expiry;
                            c.IsActive = oCompany.IsActive;

                            dataB.SaveChanges();

                            User u = dataB.Users.Select(x => x).Where(x => x.Id == oCompany.AdminId).FirstOrDefault();
                            u.Name = oCompany.AdminName;
                            u.Username = oCompany.AdminUsername;
                            u.Password = oCompany.AdminPassword;
                            
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

        public CBoolMessage DeleteCompany(long companyCode)
        {
            CBoolMessage cbm = new CBoolMessage();
            //Under Construction
            return cbm;
        }


        private bool IsCompanyUsernameAlreadyUsed(string username)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Companies.Select(e => e).Where(e => e.CompanyCode == username);
                if (data.Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsCompanyUsernameAlreadyUsed(long companyId, string username)
        {
            using (var dataB = new AquaStorage())
            {
                var data = dataB.Companies.Select(e => e).Where(e => e.CompanyCode == username && e.Id != companyId);
                if (data.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public List<CCompany> ReadAllCompanies()
        {
            List<CCompany> users = new List<CCompany>();

            try
            {
                using (var dataB = new AquaStorage())
                {
                    var datas = dataB.Companies.Select(e => e).OrderBy(e => e.Name);
                    foreach (var i in datas)
                    {
                        UserRegisterService urs = new UserRegisterService();
                        CUser u = urs.ReadSuperAdmin(i.Id);
                        users.Add(new CCompany() { Company = i.Name, CompanyUsername = i.CompanyCode, Expiry = i.ExpiryDate, IsActive = i.IsActive, Id = i.Id, AdminId = u.Id, AdminName = u.Name, AdminUsername = u.Username, AdminPassword = u.Password });
                    }
                }
            }
            catch { }

            return users;
        }

        public bool Backup()
        {
            try
            {
                DateTime date = System.DateTime.Now;
                string backup=string.Format("{0}{1:00}{2:00}{3:00}{4:00}", date.Year, date.Month, date.Day,date.Hour,date.Minute)+".sql";

                Process MySqlDump = new Process();
                MySqlDump.StartInfo.FileName = @"C:/Program Files/MySQL/MySQL Server 5.6/bin/mysqldump.exe";
                MySqlDump.StartInfo.UseShellExecute = false;
                MySqlDump.StartInfo.Arguments = "-uroot -pthreedigit.123 aquastorage --result-file=C:/"+backup;
                MySqlDump.StartInfo.RedirectStandardInput = false;
                MySqlDump.StartInfo.RedirectStandardOutput = true;

                MySqlDump.Start();

                MySqlDump.WaitForExit();
                MySqlDump.Close();
                //Thread.Sleep(10000);
                return true;
            } catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }

        public bool Delete(long companyId, DateTime date)
        {
            try
            {
                using (var dataB = new AquaStorage())
                {
                    var dataBTransaction = dataB.Database.BeginTransaction();
                    try
                    {
                        long dayCode = long.Parse(string.Format("{0}{1:00}{2:00}", date.Year, date.Month, date.Day));
                        /*
                        var cr0 = dataB.AccountEntries.Select(c => c).Where(x => x.CompanyId==companyId && x.BillDate < date);
                        dataB.AccountEntries.RemoveRange(cr0);

                        var cr1=dataB.OutDealerSales.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.OutDealerSales.RemoveRange(cr1);

                        var cr2 = dataB.PrizeWins.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.PrizeWins.RemoveRange(cr2);

                        var cr3 = dataB.ProfitnLosses.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.ProfitnLosses.RemoveRange(cr3);

                        var cr4 = dataB.ProfitReports.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.ProfitReports.RemoveRange(cr4);

                        var cr6 = dataB.TicketSales.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.TicketSales.RemoveRange(cr6);

                        var cr7 = dataB.TicketSalesCache.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.TicketSalesCache.RemoveRange(cr7);

                        var cr8 = dataB.Winners.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.Winners.RemoveRange(cr8);

                        var cr9 = dataB.OutDealerProfitnLosses.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.OutDealerProfitnLosses.RemoveRange(cr9);

                        var cr10 = dataB.OutDealerWinners.Select(c => c).Where(x => x.CompanyId == companyId && x.DayCode < dayCode);
                        dataB.OutDealerWinners.RemoveRange(cr10);
                        */
                        dataB.SaveChanges();

                        dataBTransaction.Commit();

                        return true;
                    }
                    catch (Exception e)
                    {
                        dataBTransaction.Rollback();
                        MessageBox.Show(e.Message);
                    }
                    finally
                    {

                    }
                }


            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return false;
        }

        
    }
}
