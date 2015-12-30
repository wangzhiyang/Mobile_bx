using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using System.Reflection;
namespace xhestore.FrameWork.DBAccess
{
    public class DataAccessCommand
    {
        #region 构造区域

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="parent">数据访问对象。</param>
        /// <exception cref="ArgumentNullException">如果parent为空引用，则抛出该异常。</exception>
        public DataAccessCommand(DataAccess parent)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            m_parent = parent;
        }

        #endregion

        #region 字段区域

        private DataAccess m_parent;
        private string m_sql = string.Empty;
        private DataAccessParameterCollection m_parameters;
        private CommandType m_sqlType = CommandType.Text;
        private int m_returnCount = -1;
        private Collection<object[]> m_valuesList;
        private bool m_transplantable;

        #endregion

        #region 属性区域

        /// <summary>
        /// 获取数据访问对象。该属性始终不为空引用。
        /// </summary>
        public DataAccess Parent
        {
            get { return m_parent; }
        }

        /// <summary>
        /// 获取或设置SQL语句（不允许存在多条语句）。<br/>
        /// 如果设置Transplantable=true，则获取时会返回一个经过处理的SQL语句，处理后的SQL语句具有更好的可移植性。
        /// </summary>
        public string Sql
        {
            get
            {
                if (!Transplantable) return m_sql;
                string s = IgnoreScriptComment(m_sql);
                return ConvertParameterToken(s, Parent.ParameterToken);
            }
            set { m_sql = value; }
        }

        /// <summary>
        /// 获取动态参数的集合。该属性始终不为空引用。<br/>
        /// 注意：参数的顺序要与SQL语句中的参数的顺序保持一致。
        /// </summary>
        public DataAccessParameterCollection Parameters
        {
            get
            {
                if (m_parameters == null)
                    m_parameters = new DataAccessParameterCollection(Parent);
                return m_parameters;
            }
            set { m_parameters = value; }
        }

        /// <summary>
        /// 获取或设置SQL语句类型。
        /// </summary>
        public CommandType SqlType
        {
            get { return m_sqlType; }
            set { m_sqlType = value; }
        }

        /// <summary>
        /// 获取或设置返回行数。<br/>
        /// 该属性只对SELECT语句有效。<br/>
        /// 如果小于0，则返回所有行。<br/>
        /// 默认值为-1。
        /// </summary>
        public int ReturnCount
        {
            get { return m_returnCount; }
            set { m_returnCount = value; }
        }

        /// <summary>
        /// 获取动态参数值数组的列表。该属性始终不为空引用。<br/>
        /// 如果ValuesList.Count和Parameters.Count都大于0，则ValuesList[0].Count和Parameters.Count必须相等。
        /// </summary>
        public Collection<object[]> ValuesList
        {
            get
            {
                if (m_valuesList == null)
                    m_valuesList = new Collection<object[]>();
                return m_valuesList;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示<see cref="DataAccessCommand"/>是否是可移植的。<br/>
        /// 需要注意的是，如果Transplantable=true，并且Parent.ParameterToken='?'，则需要确保Text中的动态参数的数量与Parameters.Count相等。
        /// </summary>
        public bool Transplantable
        {
            get { return m_transplantable; }
            set { m_transplantable = value; }
        }

        #endregion

        #region 方法区域
        /// <summary>
        ///  设置动态参数的集合，将类型实例转换为参数集合
        /// </summary>
        /// <typeparam name="T">接收参数的类型</typeparam>
        /// <param name="t">接收参数的类型实例</param>
        public void SetParameterCollection<T>(T t)
        {
            if (m_parameters == null)
                m_parameters = new DataAccessParameterCollection(Parent);

            PropertyInfo[] propertys = t.GetType().GetProperties();
            foreach (PropertyInfo p in propertys)
            {
                object obj = p.GetValue(t, null);
                if (obj != null)
                {
                    m_parameters.Add("@" + p.Name, ConvertToDbType(p.PropertyType), obj);
                }
            }
        }

        /// <summary>
        /// 返回数据访问命令的字符串表示，显示详细的SQL语句信息。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append(SqlType.ToString());
            sb.Append(": ");
            sb.AppendLine(Sql);

            for (int i = 0; i < Parameters.Count; i++)
            {
                //参数名
                sb.Append("Parameters[\"");
                sb.Append(Parameters[i].ParameterName);
                sb.Append("\"] = ");

                //参数值
                if (Parameters[i].Value is string)
                {
                    sb.Append("'");
                    sb.Append(Parameters[i].Value);
                    sb.Append("'");
                }
                else if (Parameters[i].Value != null && Parameters[i].Value.ToString().Length > 0)
                {
                    sb.Append(Parameters[i].Value);
                }
                else if (Parameters[i].Value == null)
                {
                    sb.Append("NULL");
                }
                else
                {
                    sb.Append(string.Empty);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// 将指定字段值数组添加到ValuesList属性。
        /// </summary>
        /// <param name="values">字段值数组（一条记录的动态参数值）。</param>
        /// <exception cref="ArgumentNullException">如果values为空引用，则抛出该异常。</exception>
        public void AddValues(object[] values)
        {
            if (values == null) throw new ArgumentNullException("values");
            if (values.Length > 0) ValuesList.Add(values);
        }

        /// <summary>
        /// 将指定数据表中的值添加到ValuesList属性。
        /// </summary>
        /// <param name="dt">数据表。</param>
        /// <exception cref="ArgumentNullException">如果dt为空引用，则抛出该异常。</exception>
        public void AddValues(DataTable dt)
        {
            if (dt == null) throw new ArgumentNullException("dt");
            if (dt.Columns.Count == 0 || dt.Rows.Count == 0) return;

            //将数据表中每条记录的字段值数组，复制到ValuesList属性中
            foreach (DataRow row in dt.Rows) ValuesList.Add(row.ItemArray);
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 判断指定的SQL是否是存储过程。
        /// </summary>
        /// <param name="sql">SQL语句。</param>
        /// <returns>如果是存储过程，则返回True，否则返回False。</returns>
        public static bool IsProcedure(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return false;

            //获取开头关键字
            StringBuilder sb = new StringBuilder();
            foreach (char c in TrimBlank(sql))
            {
                if (Char.IsLetterOrDigit(c)) sb.Append(c);
                else break;
            }
            if (sb.Length == 0) return false;

            //判断开头关键字
            StringCollection keywords = new StringCollection();
            keywords.AddRange(new string[] { 
                "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "TRUNCATE", "DROP" });
            if (keywords.Contains(sb.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture))) return false;

            return true;
        }

        /// <summary>
        /// 解析指定的SQL脚本，返回一组SQL语句。多个语句之间用半角分号分隔。
        /// </summary>
        /// <param name="script">要解析的SQL脚本。</param>
        /// <returns>一组SQL语句。</returns>
        public static StringCollection ParseScriptToStatements(string script)
        {
            StringCollection statements = new StringCollection();

            if (string.IsNullOrEmpty(script)) return statements;

            int i = 0;
            StringBuilder sb = new StringBuilder();
            foreach (char c in script)
            {
                if (c == '\'')
                {
                    i++;
                    sb.Append(c);
                }
                else if (c == ';' && i % 2 == 0)
                {
                    string a = sb.ToString();
                    sb.Remove(0, sb.Length);
                    sb.Append(TrimBlank(a));
                    if (sb.Length > 0) statements.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                }
                else sb.Append(c);
            }
            string b = sb.ToString();
            sb.Remove(0, sb.Length);
            sb.Append(TrimBlank(b));
            if (sb.Length > 0) statements.Add(sb.ToString());

            return statements;
        }

        /// <summary>
        /// 忽略SQL脚本中的注释（//、--、/**/）。<br/>
        /// 目前不支持嵌套注释。
        /// </summary>
        /// <param name="script">SQL脚本。</param>
        /// <returns>去除注释后的SQL脚本。</returns>
        public static string IgnoreScriptComment(string script)
        {
            if (string.IsNullOrEmpty(script)) return string.Empty;

            string[] lines = script.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder text2 = new StringBuilder();
            foreach (string line in lines)
            {
                string s = TrimBlank(line);

                int c1 = s.IndexOf("--", StringComparison.CurrentCultureIgnoreCase);
                if (c1 >= 0) s = TrimBlank(s.Substring(0, c1));

                int c2 = s.IndexOf("//", StringComparison.CurrentCultureIgnoreCase);
                if (c2 >= 0) s = TrimBlank(s.Substring(0, c2));

                if (!string.IsNullOrEmpty(s))
                {
                    //去除多行注释/**/
                    //同一行中可能嵌入多个/**/，所以要循环处理所有此类注释
                    int c3 = s.IndexOf("/*", StringComparison.CurrentCultureIgnoreCase);
                    int c4 = s.IndexOf("*/", StringComparison.CurrentCultureIgnoreCase);
                    while (c3 >= 0 && c4 - c3 >= 2)
                    {
                        s = TrimBlank(s.Remove(c3, c4 - c3 + 2));
                        c3 = s.IndexOf("/*", StringComparison.CurrentCultureIgnoreCase);
                        c4 = s.IndexOf("*/", StringComparison.CurrentCultureIgnoreCase);
                    }
                }

                if (!string.IsNullOrEmpty(s))
                {
                    if (text2.Length == 0)
                        text2.Append(s);
                    else text2.Append(Environment.NewLine + s);
                }
            }

            return text2.ToString();
        }

        /// <summary>
        /// 将指定SQL语句中的动态参数标记转换为指定值。
        /// </summary>
        /// <param name="text">SQL语句。</param>
        /// <param name="token">统一转换后的动态参数标记（可选值：“@”、“:”、“?”。如果是其它字符，则直接返回SQL语句，不做任何转换）。</param>
        /// <returns>转换参数标记后的SQL语句。</returns>
        public static string ConvertParameterToken(string text, char token)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (!IsParameterToken(token)) return text;

            StringBuilder sb = new StringBuilder();

            //替换动态参数标记
            bool flag = true;
            int paramIndex = 0;
            bool ignoringName = false;
            foreach (char c in text)
            {
                if (c == '\'')
                {
                    flag = !flag;
                    sb.Append(c);
                }
                else if (flag && IsParameterToken(c))
                {
                    sb.Append(token);
                    if (token == '?')
                    {//以?标记的动态参数，需要忽略其后面的参数名称
                        ignoringName = true;
                    }
                    else if (c == '?')
                    {//以?标记的动态参数没有名称，替换为 @ 或 : 后，需要补上名称
                        sb.Append('p');
                        sb.Append(paramIndex++);
                    }
                }
                else
                {
                    //遇到终结符，则停止忽略?后面的参数名称
                    if (IsTerminators(c)) ignoringName = false;
                    if (!ignoringName) sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 判断指定的字符是否是终结符。
        /// </summary>
        /// <param name="c">目标字符。</param>
        /// <returns>如果是终结符，则返回True，否则返回False。</returns>
        private static bool IsTerminators(char c)
        {
            if (c == '(' || c == ')' || c == '<' || c == '>'
                || c == '=' || c == '!' || c == '^' || c == '&'
                || c == '+' || c == '-' || c == '*' || c == '/' || c == '|'
                || c == ' ' || c == '\n' || c == '\t' || c == '\r') return true;
            return false;
        }

        /// <summary>
        /// 判断指定的字符是否是有效的动态参数标记。
        /// </summary>
        /// <param name="c">目标字符。</param>
        /// <returns>如果是有效的动态参数标记，则返回True，否则返回False。</returns>
        private static bool IsParameterToken(char c)
        {
            return c == '@' || c == ':' || c == '?';
        }

        /// <summary>
        /// 去除指定字符串的首尾的空白字符。
        /// </summary>
        /// <param name="text">目标字符串（如果为空引用，则返回空字符串。）。</param>
        /// <returns>去除空白字符后的字符串。</returns>
        private static string TrimBlank(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Trim(' ', '\t', '\n', '\r');
        }

        /// <summary>
        /// 将数据类型转换<see cref="DbType"/>。
        /// </summary>
        /// <param name="dataType">数据类型。</param>
        /// <returns></returns>
        public static DbType ConvertToDbType(Type dataType)
        {
            if (dataType == typeof(String)) return DbType.String;
            if (dataType == typeof(DateTime)) return DbType.DateTime;
            if (dataType == typeof(Byte[])) return DbType.Binary;
            if (dataType == typeof(Byte)) return DbType.Byte;
            if (dataType == typeof(Int16)) return DbType.Int16;
            if (dataType == typeof(Int32)) return DbType.Int32;
            if (dataType == typeof(Int64)) return DbType.Int64;
            if (dataType == typeof(SByte)) return DbType.SByte;
            if (dataType == typeof(UInt16)) return DbType.UInt16;
            if (dataType == typeof(UInt32)) return DbType.UInt32;
            if (dataType == typeof(UInt64)) return DbType.UInt64;
            if (dataType == typeof(Single)) return DbType.Single;
            if (dataType == typeof(Double)) return DbType.Double;
            if (dataType == typeof(Decimal)) return DbType.Decimal;
            if (dataType == typeof(Boolean)) return DbType.Boolean;

            return DbType.String;
        }

        #endregion
    }
}
