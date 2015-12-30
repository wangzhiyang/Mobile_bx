using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using xhestore.Models;
using xhestore.FrameWork.DBAccess;

namespace xhestore.Dao.DAL
{
    public class ReadLogDAL
    {
        DBHelper helper = new DBHelper();

        /// <summary>
        /// 添加阅读记录
        /// </summary>
        /// <param name="rl"></param>
        /// <returns></returns>
        public int AddReadLog(ReadLog readlog)
        {
            string sqlstr = "INSERT INTO [dbo].[epub_readlog]([SiteID],[UserID],[BookID],[ReadType],[PageNumber],[IP],[SourceType]) " +
            " VALUES(@SiteID,@UserID,@BookID,@ReadType,@PageNumber,@IP,@SourceType)";
            return helper.ExecuteNonQueryFromDB<ReadLog>(sqlstr, ConnectionEnum.SqlServerLogConnection, readlog);
        }
        /// <summary>
        /// 获取点击列表
        /// </summary>
        /// <param name="SiteID"></param>
        /// <returns></returns>
        public List<UserClick> GetUserClickList(int SiteID)
        {
            string sqlstr = "select BookID, UserID, min(ReadTime) ReadTime  FROM [dbo].[epub_readlog] " +
                " where SiteID='" + SiteID + "' and UserID<>'' and BookID>0 group by BookID,UserID,Convert(date,ReadTime,21)" +
                " order by BookID,UserID,ReadTime";
            return helper.SelectListFromDB<UserClick>(sqlstr, ConnectionEnum.SqlServerLogConnection);
        }
        /// <summary>
        /// 获取图书的点击详情
        /// </summary>
        /// <param name="SiteID"></param>
        /// <param name="BookID"></param>
        /// <returns></returns>
        public List<UserClick> GetUserClickList(int SiteID, int BookID)
        {
            string sqlstr = "select UserID, min(ReadTime) ReadTime FROM [dbo].[epub_readlog] " +
                " where SiteID='" + SiteID + "' and UserID<>'' and BookID='" + BookID + "' group by UserID,Convert(date,ReadTime,21)" +
                " order by UserID,ReadTime";
            return helper.SelectListFromDB<UserClick>(sqlstr, ConnectionEnum.SqlServerLogConnection);
        }
    }
}
