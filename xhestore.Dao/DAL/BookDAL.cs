using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xhestore.Models;
using xhestore.FrameWork.DBAccess;

namespace xhestore.Dao.DAL
{
    public class BookDAL
    {
        DBHelper helper = new DBHelper();
        /// <summary>
        /// 分页获取书本集合列表
        /// </summary>
        /// <param name="SearchText">查询条件</param>
        /// <param name="TypeID">书本类型ID</param>
        /// <param name="PageIndex">当前页码</param>
        /// <param name="PageCount">每页显示个数</param>
        /// <returns></returns>
        public List<BookInfo> GetBookList(string SearchText = "", int TypeID = 0, int PageIndex = 1, int PageCount = 5)
        {

            List<BookInfo> list = new List<BookInfo>();
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Resource\\Book.txt";
            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath, Encoding.Default);
                string text = reader.ReadToEnd();
                reader.Close();
                string[] books = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                foreach (string book in books)
                {
                    count++;
                    if (count > 1)
                    {
                        string[] bi = book.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                        BookInfo b = new BookInfo();
                        b.BookID = Convert.ToInt32(bi[0]);
                        b.BookName = bi[1];
                        b.Author = bi[2];
                        b.ISBN = bi[3];
                        b.NO = Convert.ToInt32(bi[4]);
                        b.ClickCount = Convert.ToInt32(bi[5]);
                        list.Add(b);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取书本集合列表（首页显示）
        /// </summary>
        /// <returns></returns>
        public List<BookInfo> GetBookList()
        {
            try
            {
                string sql = string.Format("");
                return helper.SelectListFromDB<BookInfo>(sql, ConnectionEnum.SqlServerConnection);
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
