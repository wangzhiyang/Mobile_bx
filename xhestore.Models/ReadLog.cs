using System;

namespace xhestore.Models
{
    public class ReadLog
    {
        public int SiteID { get; set; }

        public string UserID { get; set; }

        //public string UserName { get; set; }

        public int ReadType { get; set; }

        public int BookID { get; set; }

        //public string ISBN { get; set; }

        //public int NO { get; set; }

        //public int Chapter { get; set; }

        public int PageNumber { get; set; }

        //public DateTime ReadTime { get; set; }

        public string IP { get; set; }

        public int SourceType { get; set; }

        //public string SessionID { get; set; }
    }
}
