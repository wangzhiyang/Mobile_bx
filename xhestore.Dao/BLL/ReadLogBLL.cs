using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xhestore.Dao;
using xhestore.Dao.DAL;
using xhestore.Models;

namespace xhestore.Dao.BLL
{
    public class ReadLogBLL
    {
        DBHelper helper = new DBHelper();
        /// <summary>
        /// 图书有效点击次数
        /// </summary>
        /// <param name="SiteID"></param>
        /// <returns></returns>
        public List<BookClick> GetBookClick(int SiteID)
        {


            ReadLogDAL dal = new ReadLogDAL();
            List<UserClick> userClick = dal.GetUserClickList(SiteID);
            
            List<UserClick> tempList = new List<UserClick>();
            UserClick temp = new UserClick();
            foreach(UserClick uc in userClick)
            {
                if(temp.BookID >0)
                {
                    if (temp.BookID == uc.BookID)
                    {
                        if(temp.UserID == uc.UserID)
                        {
                            TimeSpan ts = uc.ReadTime.Date - temp.ReadTime.Date;
                            if (ts.Days >= 28)
                            {
                                tempList.Add(uc);
                                temp.ReadTime = temp.ReadTime.AddDays(28);
                            }
                        }
                        else
                        {
                            temp.UserID = uc.UserID;
                            temp.ReadTime = uc.ReadTime;
                            tempList.Add(uc);
                        }
                    }
                    else
                    {
                        temp.BookID = uc.BookID;
                        temp.UserID = uc.UserID;
                        temp.ReadTime = uc.ReadTime;
                        tempList.Add(uc);
                    }
                }
                else
                {
                    temp.BookID = uc.BookID;
                    temp.UserID = uc.UserID;
                    temp.ReadTime = uc.ReadTime;
                    tempList.Add(uc);
                }
            }

            var q = from t in tempList
                    group t by t.BookID into g
                    select new BookClick
                    {
                        BookID = g.Key,
                        ClickCount = g.Count()
                    };
            List<BookClick> bookClick = q.OrderByDescending(s=>s.ClickCount).ThenBy(s=>s.BookID).ToList();
            return bookClick;
        }
        /// <summary>
        /// 图书点击次数详情
        /// </summary>
        /// <param name="SiteID"></param>
        /// <param name="BookID"></param>
        /// <returns></returns>
        public PageResult<UserClick> GetBookClickDetails(int SiteID, int BookID, SearchBase search)
        {       
            List<UserClick> tempList = GetBookClickList(SiteID, BookID).OrderByDescending(s=>s.ReadTime).ToList();
            PageResult<UserClick> pageResult = new PageResult<UserClick>(search.PageSize, tempList.Count, search.PageIndex,tempList,string.Empty,true);
            return pageResult;
        }
        /// <summary>
        /// 图书点击次数详情列表
        /// </summary>
        /// <param name="SiteID"></param>
        /// <param name="BookID"></param>
        /// <returns></returns>
        public List<UserClick> GetBookClickList(int SiteID, int BookID)
        {
            ReadLogDAL dal = new ReadLogDAL();
            List<UserClick> userClick = dal.GetUserClickList(SiteID, BookID);

            List<UserClick> tempList = new List<UserClick>();
            UserClick temp = new UserClick();
            foreach (UserClick uc in userClick)
            {
                if (!string.IsNullOrEmpty(temp.UserID))
                {
                    if (temp.UserID == uc.UserID)
                    {
                        TimeSpan ts = uc.ReadTime.Date - temp.ReadTime.Date;
                        if (ts.Days >= 28)
                        {
                            tempList.Add(uc);
                            temp.ReadTime = temp.ReadTime.AddDays(28);
                        }
                    }
                    else
                    {
                        temp.UserID = uc.UserID;
                        temp.ReadTime = uc.ReadTime;
                        tempList.Add(uc);
                    }
                }
                else
                {
                    temp.UserID = uc.UserID;
                    temp.ReadTime = uc.ReadTime;
                    tempList.Add(uc);
                }
            }
            return tempList;
        }
    }
}
