using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xhestore.Models
{
    public class BookInfo
    {
        public int BookID { set; get; }

        public string BookName { set; get; }

        public string Author { set; get; }
        public string ISBN { set; get; }

        public int NO { set; get; }

        public int ClickCount { set; get; }
    }
}
