using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using xhestore.FrameWork.DBAccess;
namespace xhestore.Dao
{
    public class DBHelper
    {
        /// <summary>
        /// 根据查询sql语句及参数，返回对应查询结果的List集合
        /// </summary>
        /// <typeparam name="T">返回集合包含的类型</typeparam>
        /// <typeparam name="T1">查询语句中的参数对象类型,无则传入Object</typeparam>
        /// <param name="selstr">数据库查询语句</param>
        /// <param name="connEnumStr">数据库连接串</param>
        /// <param name="obj">查询语句中的参数对象，无则传入null</param>
        /// <returns></returns>
        public List<T> SelectListFromDB<T, T1>(string SelStr, string ConnEnumStr, T1 obj) where T : new()
        {
            DataAccess dac = DataAccessFactory.CreateSqlServerInstance(ConnEnumStr);
            DataAccessCommand cmd = dac.CreateCommand(SelStr);
            if (obj != null)
            {
                cmd.SetParameterCollection<T1>(obj);
            }
            return DAOHelper.ConvertDataReader2ModelList<T>(dac.ExecuteDataReader(cmd));
        }
        public List<T> SelectListFromDB<T>(string SelStr, string ConnEnumStr) where T : new()
        {
            return SelectListFromDB<T, Object>(SelStr, ConnEnumStr, null);
        }
        /// <summary>
        /// 根据查询sql语句及参数，返回对应查询结果的对象
        /// </summary>
        /// <typeparam name="T">返回对象的类型</typeparam>
        /// <typeparam name="T1">查询语句中的参数对象类型,无则传入Object</typeparam>
        /// <param name="selstr">数据库查询语句</param>
        /// <param name="connEnumStr">数据库连接串</param>
        /// <param name="obj">查询语句中的参数对象，无则传入null</param>
        /// <returns></returns>
        public T SelectObjFromDB<T, T1>(string SelStr, string ConnEnumStr, T1 obj) where T : new()
        {
            DataAccess dac = DataAccessFactory.CreateSqlServerInstance(ConnEnumStr);
            DataAccessCommand cmd = dac.CreateCommand(SelStr);
            if (obj != null)
            {
                cmd.SetParameterCollection<T1>(obj);
            }
            return DAOHelper.ConvertDataReader2Model<T>(dac.ExecuteDataReader(cmd));
        }
        public T SelectObjFromDB<T>(string SelStr, string ConnEnumStr) where T : new()
        {
            return SelectObjFromDB<T, Object>(SelStr, ConnEnumStr, null);
        }
        /// <summary>
        /// 执行sql语句，并返回受影响的行数
        /// </summary>
        /// <typeparam name="T">sql语句中参数对象类型,无则传入Object</typeparam>
        /// <param name="sqlstr">sql语句</param>
        /// <param name="connEnumStr">数据库连接串</param>
        /// <param name="obj">语句中参数对象，无则传入null</param>
        /// <returns></returns>
        public int ExecuteNonQueryFromDB<T>(string SelStr, string ConnEnumStr, T obj)
        {
            DataAccess dac = DataAccessFactory.CreateSqlServerInstance(ConnEnumStr);
            DataAccessCommand cmd = dac.CreateCommand(SelStr);
            if (obj != null)
            {
                cmd.SetParameterCollection<T>(obj);
            }
            return dac.ExecuteNonQuery(cmd);
        }
        public int ExecuteNonQueryFromDB(string SelStr, string ConnEnumStr)
        {
            return ExecuteNonQueryFromDB<Object>(SelStr, ConnEnumStr, null);
        }
    }
}
