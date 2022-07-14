using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaServer.Local
{
    public class CCompany
    {
        public long Id { get; set; }
        public string Company { get; set; }
        public string CompanyUsername { get; set; }
        public DateTime Expiry { get; set; }
        public long AdminId { get; set; }
        public string AdminName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }        
        public bool IsActive { get; set; }
    }
}
