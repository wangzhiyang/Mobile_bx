using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xhestore.FrameWork.DBAccess
{
   public class ConnectionEnum
    {
        /// <summary>
        /// 图书馆数据库连接字符串
        /// </summary>
        public static string SqlServerConnection
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerLibrary"].ConnectionString.Trim();
            }
        }
       /// <summary>
       /// 中心书库连接字符串
       /// </summary>
        public static string SqlServerBookCenterConnection
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerBookCenter"].ConnectionString.Trim();
            }
        }
       /// <summary>
       /// 日志记录数据库连接字符串
       /// </summary>
        public static string SqlServerLogConnection
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["SqlServerLog"].ConnectionString.Trim();
            }
        }

    }
}
