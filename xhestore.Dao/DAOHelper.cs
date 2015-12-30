using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Reflection;
using xhestore.FrameWork.DBAccess;
namespace xhestore.Dao
{
    public class DAOHelper
    {
        /// <summary>
        /// 将输入的IDataReader转换为指定类型的List
        /// </summary>
        /// <param name="reader">传入的IDataReader</param>
        /// <returns>返回List<T></returns>
        public static List<T> ConvertDataReader2ModelList<T>(IDataReader reader) where T:new ()
        {
            
            List<T> listModel = new List<T>();
            if (reader == null)           
            {
                return listModel;
            }
            int count = reader.FieldCount;//获取IDataReader中包含的列数
            List<string> readerTypeName = new List<string>();
            for (int i = 0; i < count; i++)//将IDataReader中列名添加到list
            {
                readerTypeName.Add(reader.GetName(i));
            }
            while (reader.Read())
            {
                T t = new T();
                //获取T  属性列表
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo p in propertys)
                {
                    if (readerTypeName.Contains(p.Name))
                    {
                        object value = reader[p.Name];
                        if (value != DBNull.Value)
                        {
                            p.SetValue(t, value, null);
                        }
                    }
                }
                listModel.Add(t);
            }
            reader.Close();
            return listModel;
        }
        /// <summary>
        /// 将传入的IDataReader 转换为指定类型实例
        /// </summary>
        /// <param name="reader">传入的IDataReader</param>
        /// <returns>返回指定类型实例</returns>
        public static T ConvertDataReader2Model<T>(IDataReader reader) where T:new()
        {
            T t = new T();
            if (reader == null)
            {
                return t;
            }
            int count = reader.FieldCount;//获取IDataReader中包含的列数
            List<string> readerTypeName = new List<string>();
            for (int i = 0; i < count; i++)//将IDataReader中列名添加到list
            {
                readerTypeName.Add(reader.GetName(i));
            }
            if (reader.Read())
            {               
                //获取T  属性列表
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo p in propertys)
                {
                    if (readerTypeName.Contains(p.Name))
                    {
                        object value = reader[p.Name];
                        if (value != DBNull.Value)
                        {
                            p.SetValue(t, value, null);
                        }
                    }
                }
            }
            reader.Close();
            return t;
        }
        /// <summary>
        /// 拼接sql语句中in 关键字的条件语句
        /// </summary>
        /// <typeparam name="T">参数集合中的基本类型，必须可ToString(),建议为基元类型</typeparam>
        /// <param name="tList">参数集合</param>
        /// <returns>返回IN 关键字括号内语句片段</returns>
        public static string GetSQL_IN_Statement<T>(List<T> tList)
        {
            StringBuilder sber = new StringBuilder();
            if (tList == null || tList.Count <= 0)
            {
                return sber.ToString();
            }
            else
            {
                foreach (T ID in tList)
                {
                    sber.Append(string.Format("'{0}',", ID.ToString()));
                }
                return  sber.ToString().TrimEnd(',');
            }
        
        }

    }
    internal enum QueryListTypeEnum
    {
        Single,
        Multiple
    }
}
