using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xhestore.FrameWork.DBAccess
{
    /// <summary>
    /// 数据访问工厂类。该类不可继承。<br/>
    /// 该类主要作用是，提供了一系列的Create方法，开发人员通过这些方法，只要输入几个熟悉的、与具体数据库相关的参数，就可以创建<see cref="DataAccess"/>类的实例。<br/>
    /// 该类在多线程环境下是安全的。
    /// </summary>
    public sealed class DataAccessFactory
    {
        #region 构造区域

        /// <summary>
        /// 构造函数。
        /// </summary>
        private DataAccessFactory()
        {
        }

        #endregion

        #region 字段区域

        private static object LockObj = new object();

        #endregion

        #region 方法区域

        /// <summary>
        /// 创建Excel(97 - 2007)数据访问类的实例。
        /// 默认第一行为标题，并将日期和数值识别为字符串。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="fileName">Excel(97 - 2007)文件的名称。</param>
        /// <returns>Excel(97 - 2007)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果fileName为空，或者fileName不是以.xls或.xlsx为后缀名，则抛出该异常。</exception>
        public static DataAccess CreateExcelInstance(string fileName)
        {
            return CreateExcelInstance(fileName, true, true);
        }

        /// <summary>
        /// 创建Excel(97 - 2007)数据访问类的实例。
        /// 默认将日期和数值识别为字符串。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="fileName">Excel(97 - 2007)文件的名称。</param>
        /// <param name="firstRowAsTitle">第一行是否作为标题。</param>
        /// <returns>Excel(97 - 2007)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果fileName为空，或者fileName不是以.xls或.xlsx为后缀名，则抛出该异常。</exception>
        public static DataAccess CreateExcelInstance(string fileName, bool firstRowAsTitle)
        {
            return CreateExcelInstance(fileName, firstRowAsTitle, true);
        }

        /// <summary>
        /// 创建Excel(97 - 2007)数据访问类的实例。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="fileName">Excel(97 - 2007)文件的名称。</param>
        /// <param name="firstRowAsTitle">第一行是否作为标题。</param>
        /// <param name="dateAndNumberAsString">是否将日期和数值识别为字符串。注意，如果dateAndNumberAsString为True，则会导致firstRowAsTitle的值无效。</param>
        /// <returns>Excel(97 - 2007)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果fileName为空，或者fileName不是以.xls或.xlsx为后缀名，则抛出该异常。</exception>
        public static DataAccess CreateExcelInstance(string fileName, bool firstRowAsTitle, bool dateAndNumberAsString)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.InvalidExcelFileName;
                if (string.IsNullOrEmpty(fileName)) throw new ArgumentException(error, "fileName");

                string provider = "System.Data.OleDb";
                string hdr = firstRowAsTitle ? "YES" : "NO";
                string imex = dateAndNumberAsString ? "1" : "0";

                if (fileName.EndsWith(".xls", StringComparison.CurrentCultureIgnoreCase))
                {
                    //生成Excel(97 - 2003)连接字符串
                    System.Data.OleDb.OleDbConnectionStringBuilder builder =
                        new System.Data.OleDb.OleDbConnectionStringBuilder();
                    builder.Add("Provider", "Microsoft.Jet.OLEDB.4.0");
                    builder.Add("Data Source", fileName);
                    builder.Add("User Id", "admin");
                    builder.Add("Extended Properties", "Excel 8.0;HDR=" + hdr + ";IMEX=" + imex);

                    return new DataAccess(provider, builder.ConnectionString);
                }
                else if (fileName.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase))
                {
                    //生成Excel(2007)连接字符串
                    System.Data.OleDb.OleDbConnectionStringBuilder builder =
                        new System.Data.OleDb.OleDbConnectionStringBuilder();
                    builder.Add("Provider", "Microsoft.ACE.OLEDB.12.0");
                    builder.Add("Data Source", fileName);
                    builder.Add("User Id", "admin");
                    builder.Add("Extended Properties", "Excel 12.0;HDR=" + hdr + ";IMEX=" + imex);

                    return new DataAccess(provider, builder.ConnectionString);
                }
                throw new ArgumentException(error, "fileName");
            }
        }

        /// <summary>
        /// 创建Access(97 - 2007)数据访问类的实例。
        /// 默认为空的访问密码。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="fileName">Access(97 - 2007)文件的名称。</param>
        /// <returns>Access(97 - 2007)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果fileName为空，或者fileName不是以.mdb或.accdb为后缀名，则抛出该异常。</exception>
        public static DataAccess CreateAccessInstance(string fileName)
        {
            return CreateAccessInstance(fileName, string.Empty);
        }

        /// <summary>
        /// 创建Access(97 - 2007)数据访问类的实例。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="fileName">Access(97 - 2007)文件的名称。</param>
        /// <param name="password">访问密码。</param>
        /// <returns>Access(97 - 2007)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果fileName为空，或者fileName不是以.mdb或.accdb为后缀名，则抛出该异常。</exception>
        public static DataAccess CreateAccessInstance(string fileName, string password)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.InvalidAccessFileName;
                if (string.IsNullOrEmpty(fileName)) throw new ArgumentException(error, "fileName");

                string provider = "System.Data.OleDb";

                if (fileName.EndsWith(".mdb", StringComparison.CurrentCultureIgnoreCase))
                {
                    //生成Access(97 - 2003)连接字符串
                    System.Data.OleDb.OleDbConnectionStringBuilder builder =
                        new System.Data.OleDb.OleDbConnectionStringBuilder();
                    builder.Add("Provider", "Microsoft.Jet.OLEDB.4.0");
                    builder.Add("Data Source", fileName);
                    builder.Add("User Id", "admin");
                    builder.Add("Jet OLEDB:Database Password", password);

                    return new DataAccess(provider, builder.ConnectionString);
                }
                else if (fileName.EndsWith(".accdb", StringComparison.CurrentCultureIgnoreCase))
                {
                    //生成Access(2007)连接字符串
                    System.Data.OleDb.OleDbConnectionStringBuilder builder =
                        new System.Data.OleDb.OleDbConnectionStringBuilder();
                    builder.Add("Provider", "Microsoft.ACE.OLEDB.12.0");
                    builder.Add("Data Source", fileName);
                    builder.Add("User Id", "admin");
                    builder.Add("Jet OLEDB:Database Password", password);

                    return new DataAccess(provider, builder.ConnectionString);
                }
                throw new ArgumentException(error, "fileName");
            }
        }

        /// <summary>
        /// 创建Oracle(8i, 9i, 10g, 11g)数据访问类的实例。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串（默认Provider=System.Data.OracleClient）。</param>
        /// <returns>Oracle(8i, 9i, 10g, 11g)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果connectionString为空，则抛出该异常。</exception>
        public static DataAccess CreateOracleInstance(string connectionString)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.ConnectionStringIsEmpty;
                if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException(error, "connectionString");

                string provider = "System.Data.OracleClient";
                return new DataAccess(provider, connectionString) { ParameterToken = ':' };
            }
        }

        /// <summary>
        /// 创建Oracle(8i, 9i, 10g, 11g)数据访问类的实例。
        /// 该方法不用在本地配置TNS。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="sid">SID。</param>
        /// <param name="host">主机名。</param>
        /// <param name="port">端口号。</param>
        /// <param name="userId">用户名。</param>
        /// <param name="password">密码。</param>
        /// <returns>Oracle(8i, 9i, 10g, 11g)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果sid、host、port、userId、password任意一个参数为空，则抛出该异常。</exception>
        public static DataAccess CreateOracleInstance(string sid, string host, string port, string userId, string password)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.InvalidOracleConnectionInfo;
                if (string.IsNullOrEmpty(sid)) throw new ArgumentException(error, "sid");
                if (string.IsNullOrEmpty(host)) throw new ArgumentException(error, "host");
                if (string.IsNullOrEmpty(port)) throw new ArgumentException(error, "port");
                if (string.IsNullOrEmpty(userId)) throw new ArgumentException(error, "userId");
                if (string.IsNullOrEmpty(password)) throw new ArgumentException(error, "password");

                string provider = "System.Data.OracleClient";

                //生成连接字符串
                string tns = "(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = " + host +
                    ")(PORT = " + port + ")) ) (CONNECT_DATA = (SID = " + sid + ") (SERVER = DEDICATED)))";
                string connectionString = "User Id=" + userId + ";Data Source=" + tns + ";Password=" + password + ";";

                return new DataAccess(provider, connectionString) { ParameterToken = ':' };
            }
        }

        /// <summary>
        /// 创建Oracle(8i, 9i, 10g, 11g)数据访问类的实例。
        /// 该方法不用在本地配置TNS。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="sid">SID。</param>
        /// <param name="address">主机地址。格式：192.168.0.123:1521，如果是集群数据库，则用半角逗号分隔，例如192.168.0.123:1521,192.168.0.124:1521。 </param>
        /// <param name="userId">用户名。</param>
        /// <param name="password">密码。</param>
        /// <returns>Oracle(8i, 9i, 10g, 11g)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果sid、host、userId、password任意一个参数为空，则抛出该异常。</exception>
        public static DataAccess CreateOracleInstance(string sid, string address, string userId, string password)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.InvalidOracleConnectionInfo;
                if (string.IsNullOrEmpty(sid)) throw new ArgumentException(error, "sid");
                if (string.IsNullOrEmpty(address)) throw new ArgumentException(error, "address");
                if (address.IndexOf(':') < 7) throw new ArgumentException(error, "address");
                if (string.IsNullOrEmpty(userId)) throw new ArgumentException(error, "userId");
                if (string.IsNullOrEmpty(password)) throw new ArgumentException(error, "password");

                address = address.Replace(" ", string.Empty);
                string[] addressList = address.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (addressList.Length == 0) throw new ArgumentException(error, "address");

                string[] hostList = new string[addressList.Length];
                string[] portList = new string[addressList.Length];
                for (int i = 0; i < addressList.Length; i++)
                {
                    string[] items = addressList[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length != 2) throw new ArgumentException(error, "address");
                    hostList[i] = items[0];
                    portList[i] = items[1];
                }

                return CreateOracleInstance(sid, hostList, portList, userId, password);
            }
        }

        /// <summary>
        /// 创建Oracle(8i, 9i, 10g, 11g)数据访问类的实例。
        /// 该方法不用在本地配置TNS。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="sid">SID。</param>
        /// <param name="hostList">主机名列表。</param>
        /// <param name="portList">端口号列表。</param>
        /// <param name="userId">用户名。</param>
        /// <param name="password">密码。</param>
        /// <returns>Oracle(8i, 9i, 10g, 11g)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果sid、hostList、portList、userId、password任意一个参数为空，则抛出该异常。</exception>
        public static DataAccess CreateOracleInstance(string sid, string[] hostList, string[] portList, string userId, string password)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.InvalidOracleConnectionInfo;
                if (string.IsNullOrEmpty(sid)) throw new ArgumentException(error, "sid");
                if (hostList == null || hostList.Length == 0) throw new ArgumentException(error, "hostList");
                if (portList == null || portList.Length == 0) throw new ArgumentException(error, "portList");
                if (hostList.Length != portList.Length) throw new ArgumentException(error, "portList");
                if (string.IsNullOrEmpty(userId)) throw new ArgumentException(error, "userId");
                if (string.IsNullOrEmpty(password)) throw new ArgumentException(error, "password");

                string provider = "System.Data.OracleClient";

                //生成TNS
                string tns = "(DESCRIPTION = (ADDRESS_LIST = ";
                for (int i = 0; i < hostList.Length; i++)
                {
                    tns += "(ADDRESS = (PROTOCOL = TCP)(HOST = " + hostList[i] + ")(PORT = " + portList[i] + ")) ";
                }
                tns += ") (CONNECT_DATA = (SERVICE_NAME = " + sid + ")))";

                string connectionString = "User Id=" + userId + ";Data Source=" + tns + ";Password=" + password + ";";

                return new DataAccess(provider, connectionString) { ParameterToken = ':' };
            }
        }

        /// <summary>
        /// 创建Oracle(8i, 9i, 10g, 11g)数据访问类的实例。
        /// 该方法必须在本地配置TNS。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="serviceName">服务名（需要在本地配置TNS）。</param>
        /// <param name="userId">用户名。</param>
        /// <param name="password">密码。</param>
        /// <returns>Oracle(8i, 9i, 10g, 11g)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果serverName、userId、password任意一个参数为空，则抛出该异常。</exception>
        public static DataAccess CreateOracleInstance(string serviceName, string userId, string password)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.InvalidOracleConnectionInfo;
                if (string.IsNullOrEmpty(serviceName)) throw new ArgumentException(error, "serviceName");
                if (string.IsNullOrEmpty(userId)) throw new ArgumentException(error, "userId");
                if (string.IsNullOrEmpty(password)) throw new ArgumentException(error, "password");

                string provider = "System.Data.OracleClient";

                //生成连接字符串
                string connectionString = "User Id=" + userId + ";Data Source=" + serviceName + ";Password=" + password + ";";

                return new DataAccess(provider, connectionString) { ParameterToken = ':' };
            }
        }

        /// <summary>
        /// 创建SqlServer(2000, 2005, 2008)数据访问类的实例。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="server">服务器名。</param>
        /// <param name="database">数据库名。</param>
        /// <param name="userId">用户名。</param>
        /// <param name="password">密码。</param>
        /// <returns>SqlServer(2000, 2005, 2008)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果server、database、userId任意一个为空，则抛出该异常。</exception>
        public static DataAccess CreateSqlServerInstance(string server, string database, string userId, string password)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.InvalidSqlServerConnectionInfo;
                if (string.IsNullOrEmpty(server)) throw new ArgumentException(error, "server");
                if (string.IsNullOrEmpty(database)) throw new ArgumentException(error, "database");
                if (string.IsNullOrEmpty(userId)) throw new ArgumentException(error, "userId");

                string provider = "System.Data.SqlClient";

                //生成连接字符串
                System.Data.SqlClient.SqlConnectionStringBuilder builder =
                    new System.Data.SqlClient.SqlConnectionStringBuilder();
                builder.Add("Data Source", server);
                builder.Add("Initial Catalog", database);
                builder.Add("User Id", userId);
                builder.Add("Password", password);

                return new DataAccess(provider, builder.ConnectionString) { ParameterToken = '@' };
            }
        }

        /// <summary>
        /// 创建SqlServer(2000, 2005, 2008)数据访问类的实例。
        /// 该方法在多线程下是安全的。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串（默认Provider=System.Data.SqlClient）。</param>
        /// <returns>SqlServer(2000, 2005, 2008)数据访问类的实例。</returns>
        /// <exception cref="ArgumentException">如果connectionString为空，则抛出该异常。</exception>
        public static DataAccess CreateSqlServerInstance(string connectionString)
        {
            lock (LockObj)
            {
                string error = Properties.Resources.ConnectionStringIsEmpty;
                if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException(error, "connectionString");

                string provider = "System.Data.SqlClient";
                return new DataAccess(provider, connectionString) { ParameterToken = '@' };
            }
        }

        #endregion
    }
}
