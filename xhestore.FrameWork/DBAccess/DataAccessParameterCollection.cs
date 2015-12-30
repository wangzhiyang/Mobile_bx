using System;
using System.Data;
using System.Data.Common;
using System.Collections.ObjectModel;

namespace xhestore.FrameWork.DBAccess
{
    public class DataAccessParameterCollection : Collection<DbParameter>
    {
        #region 构造区域

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="parent">数据访问对象。</param>
        /// <exception cref="ArgumentNullException">如果parent为空引用，则抛出该异常。</exception>
        public DataAccessParameterCollection(DataAccess parent)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            m_parent = parent;
        }

        #endregion

        #region 字段区域

        private DataAccess m_parent;

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
        /// 动态参数索引。
        /// </summary>
        /// <param name="parameterName">要获取的动态参数的名称（不区分大小写）。如果不存在，则返回空引用。</param>
        /// <returns>动态参数。</returns>
        public DbParameter this[string parameterName]
        {
            get
            {
                int index = IndexOf(parameterName);
                return index >= 0 ? this[index] : null;
            }
        }

        #endregion

        #region 方法区域

        /// <summary>
        /// 返回指定名称的参数在集合中的索引号。
        /// 如果不存在，则返回-1。
        /// </summary>
        /// <param name="paramName">参数名称（不区分大小写）。</param>
        /// <returns>参数在集合中的索引号。</returns>
        public int IndexOf(string paramName)
        {
            if (paramName == null) paramName = string.Empty;
            for (int i = 0; i < Count; i++)
            {
                if (string.Compare(this[i].ParameterName, paramName,
                    StringComparison.CurrentCultureIgnoreCase) == 0) return i;
            }
            return -1;
        }

        /// <summary>
        /// 判断是否包含指定名称的参数。
        /// </summary>
        /// <param name="paramName">参数名称（不区分大小写）。</param>
        /// <returns>如果包含，则返回True，否则返回False。</returns>
        public bool Contains(string paramName)
        {
            return IndexOf(paramName) >= 0;
        }

        /// <summary>
        /// 添加一个新的的动态参数对象。
        /// </summary>
        /// <returns>新的动态参数对象。</returns>
        public DbParameter Add()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(Parent.Provider);
            DbParameter parameter = factory.CreateParameter();
            Add(parameter);
            return parameter;
        }

        /// <summary>
        /// 添加一个指定名称的动态参数对象。
        /// </summary>
        /// <param name="paramName">参数名称。</param>
        /// <returns>动态参数对象。</returns>
        public DbParameter Add(string paramName)
        {
            if (paramName == null) paramName = string.Empty;
            DbProviderFactory factory = DbProviderFactories.GetFactory(Parent.Provider);
            DbParameter parameter = factory.CreateParameter();
            parameter.ParameterName = paramName;
            Add(parameter);
            return parameter;
        }

        /// <summary>
        /// 添加一个指定名称和值的动态参数对象。
        /// </summary>
        /// <param name="paramName">参数名称。</param>
        /// <param name="paramValue">参数值。</param>
        /// <returns>动态参数对象。</returns>
        public DbParameter Add(string paramName, object paramValue)
        {
            if (paramName == null) paramName = string.Empty;
            DbProviderFactory factory = DbProviderFactories.GetFactory(Parent.Provider);
            DbParameter parameter = factory.CreateParameter();
            parameter.ParameterName = paramName;
            parameter.Value = paramValue;
            Add(parameter);
            return parameter;
        }

        /// <summary>
        /// 添加一个指定名称、类型和值的动态参数对象。
        /// </summary>
        /// <param name="paramName">参数名称。</param>
        /// <param name="paramType">参数类型。</param>
        /// <param name="paramValue">参数值。</param>
        /// <returns>动态参数对象。</returns>
        public DbParameter Add(string paramName, DbType paramType, object paramValue)
        {
            if (paramName == null) paramName = string.Empty;
            DbProviderFactory factory = DbProviderFactories.GetFactory(Parent.Provider);
            DbParameter parameter = factory.CreateParameter();
            parameter.ParameterName = paramName;
            parameter.DbType = paramType;
            parameter.Value = paramValue;
            Add(parameter);
            return parameter;
        }

        #endregion
    }
}
