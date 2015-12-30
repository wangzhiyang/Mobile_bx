using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xhestore.Models
{
    public class SearchBase
    {
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; }//总页数
        /// <summary>
        /// 当前页数
        /// </summary>
        private int _PageIndex;
        public int PageIndex
        {
            get
            {
                if (_PageIndex <= 0)
                {
                    return 1;
                }
                else
                {
                    return _PageIndex;
                }
            }
            set { _PageIndex = value <= 0 ? 1 : value; }
        }

        /// <summary>
        /// 每页显示记录数
        /// </summary>
        private int _PageSize;
        public int PageSize
        {
            get
            {
                if (_PageSize <= 0)
                {
                    return 10;
                }
                else
                {
                    return _PageSize;
                }
            }
            set
            {
                _PageSize = value <= 0 ? 10 : value;
            }
        }
    }
}
