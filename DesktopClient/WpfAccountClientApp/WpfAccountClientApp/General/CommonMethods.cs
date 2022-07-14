using Newtonsoft.Json;
using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AquaClient.General
{
    public class CommonMethods
    {
        public CommonMethods()
        {
            LoadMaskData();
        }
        public static Dictionary<int, string> TicketMask = new Dictionary<int, string>();

        private static void LoadMaskData()
        {
            TicketMask.Clear();
            /*TicketMask.Add(CTicket.MASK_A, "A");
            TicketMask.Add(CTicket.MASK_B, "B");
            TicketMask.Add(CTicket.MASK_C, "C");
            TicketMask.Add(CTicket.MASK_AB, "AB");
            TicketMask.Add(CTicket.MASK_AC, "AC");
            TicketMask.Add(CTicket.MASK_BC, "BC");
            TicketMask.Add(CTicket.MASK_ABC, "ABC");*/
        }

        public static void GetClientInfo()
        {            
            try
            {
                using (StreamReader sr = new StreamReader(@"ClientInfo.jsn"))
                {
                    using (JsonReader jr = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ApplicationStaticVariables.gClientLocalInfo=serializer.Deserialize<CClientLocalInfo>(jr);
                    }
                }
            }
            catch
            {

            }
      
        }

        public static bool SetClientInfo()
        {
            bool success = true;            
            try
            {
                using (StreamWriter wr = new StreamWriter(@"ClientInfo.jsn"))
                {
                    using (JsonWriter jw = new JsonTextWriter(wr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(jw,ApplicationStaticVariables.gClientLocalInfo);
                    }
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public static void GetServerInfo()
        {
            try
            {
                using (StreamReader sr = new StreamReader(@"ServerInfo.jsn"))
                {
                    using (JsonReader jr = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        ApplicationStaticVariables.gServerAddress = serializer.Deserialize<string>(jr);
                    }
                }
            }
            catch
            {

            }

        }

        public static bool SetServerInfo()
        {
            bool success = true;
            try
            {
                using (StreamWriter wr = new StreamWriter(@"ServerInfo.jsn"))
                {
                    using (JsonWriter jw = new JsonTextWriter(wr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(jw, ApplicationStaticVariables.gServerAddress);
                    }
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public static long GetOutDealerLiveTimer()
        {
            try
            {
                using (StreamReader sr = new StreamReader(@"OutDealerLiveTimer.jsn"))
                {
                    using (JsonReader jr = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return serializer.Deserialize<long>(jr);
                    }
                }
            }
            catch
            {

            }

            return 0;
        }

        public static void SetOutDealerLiveTimer(long time)
        {
            
            try
            {
                using (StreamWriter wr = new StreamWriter(@"OutDealerLiveTimer.jsn"))
                {
                    using (JsonWriter jw = new JsonTextWriter(wr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(jw, time);
                    }
                }
            }
            catch
            {
            
            }            
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
       where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            foreach (childItem child in FindVisualChildren<childItem>(obj))
            {
                return child;
            }

            return null;
        }        
    }
}
