using ServerServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaClient.General
{
    public static class ApplicationStaticVariables
    {
        public static CClientLocalInfo gClientLocalInfo= new CClientLocalInfo();
        public static string gServerAddress = "http://localhost:55555";
    }
}
