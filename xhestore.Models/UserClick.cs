using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xhestore.Models
{
    public class UserClick
    {
        public int BookID { get; set; }

        public string UserID { get; set; }

        public DateTime ReadTime { get; set; }
    }
}
