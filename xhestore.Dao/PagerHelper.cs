using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using xhestore.FrameWork.DBAccess;
//using xhestore.FrameWork.Common;
namespace xhestore.Dao
{
    /// <summary>
    /// 处理分页
    /// </summary>
    public class PagerHelper<T> where T : new()
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SqlStr">基本查询语句</param>
        /// <param name="CurrrentPageNo">页数</param>
        /// <param name="Conn">数据库连接</param>
        public PagerHelper(string SqlStr, int CurrrentPageNo, string Conn)
        {
            QuerySqlStr = GetStandardSqlStr(SqlStr);
            CurrentPageIndex = CurrrentPageNo;
            Connection = Conn;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SqlStr">基本查询语句</param>
        /// <param name="CurrrentPageNo">页数</param>
        public PagerHelper(string SqlStr, int CurrrentPageNo)
        {
            QuerySqlStr = GetStandardSqlStr(SqlStr);
            CurrentPageIndex = CurrrentPageNo;
        }
        public PagerHelper() { }
        public string Connection { get; set; }
        /// <summary>
        /// 每页显示记录数量，默认为10条
        /// </summary>
        private int _PageSize = 10;
        public int PageSize
        {
            set { _PageSize = value; }
            get { return _PageSize; }
        }
        /// <summary>
        /// 当前页
        /// </summary>
        private int _CurrentPageIndex = 1;
        public int CurrentPageIndex
        {
            set
            {
                if (value > 0) _CurrentPageIndex = value;
            }
            get { return _CurrentPageIndex; }
        }
        public string RowNumberTag { get; set; }
        /// <summary>
        /// 基本查询语句
        /// </summary>
        public string QuerySqlStr { get; set; }

        /// <summary>
        /// 添加 ROW_NUMBER() over()后的查询语句
        /// </summary>
        public string LastQueryStr { get; set; }
        /// <summary>
        /// 查询结果数量语句
        /// </summary>
        public string LastQueryCountStr { get; set; }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <typeparam name="T1">查询参数类型,通常为ViewModel</typeparam>
        /// <param name="t">查询参数实例</param>
        /// <returns>返回分页结果，包含已分页后数据集</returns>
        public PageResult<T> GetPagerResult<T1>(T1 t)
        {
            if (string.IsNullOrEmpty(LastQueryCountStr))
            {
                LastQueryCountStr = GetLastQueryCountStr(QuerySqlStr);//查询结果数量语句
            }
            if (string.IsNullOrEmpty(LastQueryStr))
            {
                LastQueryStr = GetLastQueryStr(QuerySqlStr);//添加 ROW_NUMBER() over()后的查询语句   
            }

            DataAccess access;
            if (string.IsNullOrEmpty(Connection))
            {
                access = DataAccessFactory.CreateSqlServerInstance(ConnectionEnum.SqlServerConnection);
            }
            else
            {
                access = DataAccessFactory.CreateSqlServerInstance(Connection);
            }
            DataAccessCommand command = access.CreateCommand();
            if (t != null)
            {
                command.SetParameterCollection<T1>(t);//添加参数集
            }

            /// 获取总记录数
            int ItemCount = 0;//所有记录数，默认为0
            command.Sql = LastQueryCountStr;
            object count = access.ExecuteScalar(command);
            if (count is int)
            {
                ItemCount = (int)count;
                // Result.TotalItemCount = ItemCount;
            }

            //需返回的分页结果实例
            PageResult<T> Result = new PageResult<T>(_PageSize, ItemCount, _CurrentPageIndex, string.Empty);
            if (Result.CurrentPageIndex > 0)
            {
                _CurrentPageIndex = Result.CurrentPageIndex;
            }
            ///获取分页后数据集
            command.Sql = GetPagedSql();//获取分页查询语句
            Result.ResultList = DAOHelper.ConvertDataReader2ModelList<T>(access.ExecuteDataReader(command));

            return Result;
        }
        public PageResult<T> GetPagerResult()
        {
            return GetPagerResult<Object>(null);
        }

        /// <summary>
        /// 标准化查询语句中关键字
        /// </summary>
        /// <param name="SqlStr">基本查询语句</param>
        /// <returns>替换关键字后查询语句</returns>
        private string GetStandardSqlStr(string SqlStr)
        {
            string result= SqlStr.Replace("select", "SELECT").Replace("from", "FROM").Replace("group","GROUP");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"group\s*by", "GROUP BY", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return result;
        }
        /// <summary>
        /// 由基本查询语句 获取总记录数查询语句
        /// </summary>
        /// <param name="SqlStr">基本查询语句</param>
        /// <returns>总记录数查询语句</returns>
        private string GetLastQueryCountStr(string SqlStr)
        {
            int sIndex = SqlStr.IndexOf("FROM");
            string sql= " SELECT COUNT(0) AS ItemCount " + SqlStr.Substring(sIndex, SqlStr.Length - sIndex);
            if (SqlStr.Contains("GROUP BY"))
            {
                if (SqlStr.EndsWith("as tb where tb.BookID <> 0 "))
                {
                    return sql;
                }
                string head = " SELECT COUNT(0) as ItemCount From ( ";
                string tail = " ) as tb ";
                return head + sql + tail;
            }
            else
            {
                return sql;
            }

        }
        /// <summary>
        /// 为基本查询语句添加 ROW_NUMBER() over()
        /// </summary>
        /// <param name="sqlstr">基本查询语句</param>
        /// <returns>添加 ROW_NUMBER() over() 后语句</returns>
        private string GetLastQueryStr(string sqlstr)
        {
            int sIndex = sqlstr.IndexOf("SELECT") + "SELECT".Length;
            return " SELECT ROW_NUMBER() over( ORDER BY " + RowNumberTag + " ) as OrderID ," + sqlstr.Substring(sIndex, sqlstr.Length - sIndex);
        }
        /// <summary>
        /// 获取分页数据完善后查询语句
        /// </summary>
        /// <returns>分页数据完善后查询语句</returns>
        private string GetPagedSql()
        {
            int maxnum = _CurrentPageIndex * _PageSize;
            int minnum = maxnum - _PageSize + 1;
            return " SELECT * FROM (" + LastQueryStr + ") as tab where  tab.OrderID between " + minnum.ToString() + " and " + maxnum.ToString();
        }

    }
    /// <summary>
    /// 分页结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResult<T>
    {
        /// <summary>
        /// 构造函数
        /// </summary>

        public PageResult(int _PageSize, int _TotalItemCount)
        {
            PageSize = _PageSize;
            TotalItemCount = _TotalItemCount;
            TotalPageCount = GetTotalPageCount(_TotalItemCount);
            CheckInternalData();
        }
        public PageResult(int _PageSize, int _TotalItemCount, int _CurrentPageIndex)
        {
            PageSize = _PageSize;
            TotalItemCount = _TotalItemCount;           
            TotalPageCount = GetTotalPageCount(_TotalItemCount);
            if (TotalItemCount > 0 && TotalPageCount < _CurrentPageIndex)
            {
                _CurrentPageIndex = TotalPageCount;
            }
            CurrentPageIndex = _CurrentPageIndex;
            CheckInternalData();
        }
        public PageResult(int _PageSize, int _TotalItemCount, int _CurrentPageIndex, IList<T> _ResultList)
        {
            PageSize = _PageSize;
            TotalItemCount = _TotalItemCount;
            ResultList = _ResultList;
            TotalPageCount = GetTotalPageCount(_TotalItemCount);
            if (TotalItemCount > 0 && TotalPageCount < _CurrentPageIndex)
            {
                _CurrentPageIndex = TotalPageCount;
            }
            CurrentPageIndex = _CurrentPageIndex;
            CheckInternalData();
        }
        public PageResult(int _PageSize, int _TotalItemCount, int _CurrentPageIndex, string _PageFooter)
        {
            PageSize = _PageSize;
            TotalItemCount = _TotalItemCount;
            PagerFooter = _PageFooter;
            TotalPageCount = GetTotalPageCount(_TotalItemCount);
            if (TotalItemCount > 0 && TotalPageCount < _CurrentPageIndex)
            {
                _CurrentPageIndex = TotalPageCount;
            }
            CurrentPageIndex = _CurrentPageIndex;
            CheckInternalData();

        }
        public PageResult(int _PageSize, int _TotalItemCount, int _CurrentPageIndex, IList<T> _ResultList, string _PageFooter)
        {
            PageSize = _PageSize;
            TotalItemCount = _TotalItemCount;
            ResultList = _ResultList;
            PagerFooter = _PageFooter;
            TotalPageCount = GetTotalPageCount(_TotalItemCount);
            if (TotalItemCount > 0 && TotalPageCount < _CurrentPageIndex)
            {
                _CurrentPageIndex = TotalPageCount;
            }
            CurrentPageIndex = _CurrentPageIndex;
            CheckInternalData();
        }
        /// <summary>
        /// 处理 _ResultList 为未分页全部数据情况，返回分页数据
        /// </summary>
        /// <param name="_PageSize"></param>
        /// <param name="_TotalItemCount"></param>
        /// <param name="_CurrentPageIndex"></param>
        /// <param name="_ResultList"></param>
        /// <param name="_PageFooter"></param>
        /// <param name="IsAutoSplitList">false默认_ResultList 为全部数据，true只返回当前分页数据</param>
        public PageResult(int _PageSize, int _TotalItemCount, int _CurrentPageIndex, IList<T> _ResultList, string _PageFooter,bool IsAutoSplitList)
        {
            PageSize = _PageSize;
            TotalItemCount = _TotalItemCount;
            TotalPageCount = GetTotalPageCount(_TotalItemCount);
            if (TotalItemCount>0&&TotalPageCount < _CurrentPageIndex)
            {
                _CurrentPageIndex = TotalPageCount;
            }
            CurrentPageIndex = _CurrentPageIndex;
            if (IsAutoSplitList)
            {
                ResultList = new List<T>();
                Split_ResultList(_ResultList);
            }
            else
            {
                ResultList = _ResultList;
            }
            PagerFooter = _PageFooter;
            
            CheckInternalData();
        }
    

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalItemCount { get; set; }
        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>  
        public int TotalPageCount { get; set; }
        /// <summary>
        /// 当前页数
        /// </summary>
        public int CurrentPageIndex { get; set; }
        /// <summary>
        /// 结果集
        /// </summary>
        public IList<T> ResultList { get; set; }
        /// <summary>
        /// 分页链接 如上一页 下一页 首页 尾页 或  1 2 3 4 5 6 7 
        /// </summary>
        public string PagerFooter { get; set; }

        /// <summary>
        /// 根据总记录数，计算总页数
        /// </summary>
        /// <param name="ItemCount">总记录数</param>
        /// <returns>总页数</returns>
        private int GetTotalPageCount(int ItemCount)
        {
            int value = ItemCount / PageSize;
            int y = ItemCount % PageSize;
            if (y != 0)
            {
                return value + 1;
            }
            else
            {
                return value;
            }
        }
        /// <summary>
        /// 分页数据检查
        /// </summary>
        private void CheckInternalData()
        {
            if (TotalPageCount <= 0)
                CurrentPageIndex = 0;
        }
        /// <summary>
        /// 将分页结果添加到ResultList
        /// </summary>
        /// <param name="_ResultList">未分页总数据集</param>
        private void Split_ResultList(IList<T> _ResultList)
        {
            int startIndex = (CurrentPageIndex - 1) * PageSize;      
            int endIndex = startIndex + PageSize - 1;
            if (_ResultList.Count - 1 < endIndex) endIndex = _ResultList.Count - 1;
            //分页完成后 排序数据
           
            for (int i = startIndex; i <= endIndex; i++)
            {
                ResultList.Add(_ResultList[i]);
            }          
           
        }
    }
}
