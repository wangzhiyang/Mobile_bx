using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xhestore.FrameWork.DBAccess
{
    public class DataAccessPaging
    {
        #region 构造区域

        /// <summary>
        /// 构造函数。
        /// </summary>
        public DataAccessPaging()
        {
        }

        #endregion

        #region 字段区域

        private int m_pageIndex;
        private int m_pageSize = 10;
        private int m_rowsCount;

        #endregion

        #region 属性区域

        /// <summary>
        /// 获取或设置要返回的数据页的索引号（从0开始）。
        /// </summary>
        public int PageIndex
        {
            get { return m_pageIndex; }
            set { m_pageIndex = value > 0 ? value : 0; }
        }

        /// <summary>
        /// 获取或设置每个数据页的大小。默认值为10。<br/>
        /// 如果PageSize小于等于0，则返回所有页的数据。
        /// </summary>
        public int PageSize
        {
            get { return m_pageSize; }
            set { m_pageSize = value > 0 ? value : 0; }
        }

        /// <summary>
        /// 获取数据的总页数。
        /// </summary>
        public int PagesCount
        {
            get { return PageSize > 0 ? (int)Math.Ceiling((decimal)RowsCount / PageSize) : 0; }
        }

        /// <summary>
        /// 获取或设置数据的总行数。
        /// 执行分页查询时，会自动计算该属性的值。
        /// </summary>
        public int RowsCount
        {
            get { return m_rowsCount; }
            internal set { m_rowsCount = value > 0 ? value : 0; }
        }

        #endregion
    }
}
