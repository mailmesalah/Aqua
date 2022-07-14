using ServerServiceInterface;
using System.Collections.Generic;
using AquaServer.StorageModel;

namespace AquaServer.General
{
    public class StaticGlobalInfos
    {

        public StaticGlobalInfos()
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

        
        
    }

}
