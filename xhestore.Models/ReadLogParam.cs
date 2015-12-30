using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xhestore.Models
{
    public class ReadLogParam
    {
        public int BookID { get; set; }

        public string UserID { get; set; }

        public string UserName { get; set; }
        public string BookName { get; set; }

        public int Chapter { get; set; }

        public string ClientFrom { get; set; }

        public string IP { get; set; }
    }
}
