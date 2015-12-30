using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xhestore.Models
{
    /// <summary>
    /// 模拟书本实体对象
    /// </summary>
    public class BookModel
    {
        
        public int ID { get; set; }
        /// <summary>
        /// 书名
        /// </summary>
        public string BookName { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// 内容简介
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 作者简介
        /// </summary>
        public string AuthorExpri { get; set; }
        /// <summary>
        /// 目录简介
        /// </summary>
        public List<string> ListExpri = new List<string>();
        /// <summary>
        /// 书本图片路径
        /// </summary>
        public string BookUrl;
    }
}
