using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace xhestore.FrameWork.DBAccess
{
    public class DataAccess
    {
        #region 构造区域

        /// <summary>
        /// 构造函数。
        /// </summary>
        public DataAccess()
        {
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="provider">数据提供程序类型（可以是：System.Data.Odbc、System.Data.OleDb、System.Data.OracleClient、System.Data.SqlClient、Microsoft.SqlServerCe.Client，以及其它第三方数据提供程序）。</param>
        /// <param name="connectionString">数据源连接字符串。</param>
        public DataAccess(string provider, string connectionString)
        {
            m_provider = provider;
            m_connectionString = connectionString;
        }

        #endregion

        #region 字段区域

        private string m_provider = "System.Data.OleDb";
        private string m_connectionString = string.Empty;
        private char m_parameterToken = '?';

        #endregion

        #region 属性区域

        /// <summary>
        /// 获取或设置数据提供程序类型（可以是：System.Data.Odbc、System.Data.OleDb、System.Data.OracleClient、System.Data.SqlClient、Microsoft.SqlServerCe.Client，以及其它第三方数据提供程序）。
        /// </summary>
        public string Provider
        {
            get { return m_provider; }
            set { m_provider = value; }
        }

        /// <summary>
        /// 获取或设置数据源连接字符串。
        /// </summary>
        public string ConnectionString
        {
            get { return m_connectionString; }
            set { m_connectionString = value; }
        }

        /// <summary>
        /// 获取或设置标准的动态参数标记。<br/>
        /// 目前支持以下三种：“@”、“:”、“?”，默认值为“?”。
        /// </summary>
        public char ParameterToken
        {
            get { return m_parameterToken; }
            set { m_parameterToken = value; }
        }

        #endregion

        #region 方法区域

        /// <summary>
        /// 测试连接。<br/>
        /// 如果Provider或ConnectionString为空，则抛出<see cref="ArgumentException"/>异常。<br/>
        /// 如果连接失败，则抛出数据库异常。
        /// </summary>
        public void TestConnection()
        {
            if (string.IsNullOrEmpty(Provider))
                throw new ArgumentException(Properties.Resources.DataProviderIsEmpty);
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ArgumentException(Properties.Resources.ConnectionStringIsEmpty);

            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
            }
        }

        /// <summary>
        /// 执行查询语句，返回查询结果。
        /// </summary>
        /// <param name="sql">要执行的查询语句（无动态参数）。</param>
        /// <returns>查询结果。</returns>
        /// <exception cref="ArgumentException">如果sql为空，则抛出该异常。</exception>
        public DataTable ExecuteDataTable(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "sql");

            return ExecuteDataTable(sql, null, -1);
        }

        /// <summary>
        /// 执行查询语句，返回指定行数的查询结果。
        /// </summary>
        /// <param name="sql">要执行的查询语句（无动态参数）。</param>
        /// <param name="returnCount">返回行数（如果小于0，则返回所有行）。</param>
        /// <returns>查询结果。</returns>
        /// <exception cref="ArgumentException">如果sql为空，则抛出该异常。</exception>
        public DataTable ExecuteDataTable(string sql, int returnCount)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "sql");

            return ExecuteDataTable(sql, null, returnCount);
        }

        /// <summary>
        /// 执行数据访问命令，返回查询结果。
        /// </summary>
        /// <param name="cmd">要执行的数据访问命令。</param>
        /// <returns>查询结果。</returns>
        /// <exception cref="ArgumentNullException">如果cmd为空引用，则抛出该异常。</exception>
        /// <exception cref="ArgumentException">如果cmd.Sql为空，则抛出该异常。</exception>
        public DataTable ExecuteDataTable(DataAccessCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "cmd");

            return ExecuteDataTable(cmd.Sql, cmd.Parameters, cmd.ReturnCount);
        }

        /// <summary>
        /// 执行数据访问命令，根据分页方案，返回查询结果。<br/>
        /// 该方法采用的是通用的分页方案，如果数据量太大会影响性能。所以，如果想获得更高的查询性能，可以针对具体数据库，编写专门的分页SQL。
        /// </summary>
        /// <param name="cmd">要执行的数据访问命令。</param>
        /// <param name="paging">分页方案。</param>
        /// <returns>查询结果。</returns>
        /// <exception cref="ArgumentNullException">如果cmd或paging为空引用，则抛出该异常。</exception>
        /// <exception cref="ArgumentException">如果cmd.Sql为空，则抛出该异常。</exception>
        public DataTable ExecuteDataTable(DataAccessCommand cmd, DataAccessPaging paging)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "cmd");
            if (paging == null) throw new ArgumentNullException("paging");

            //更新记录总数
            string sqlCount = GetCountSql(cmd.Sql);
            paging.RowsCount = Convert.ToInt32(ExecuteScalar(sqlCount, cmd.Parameters));

            //返回所有页的数据
            if (paging.PageSize <= 0) return ExecuteDataTable(cmd);

            //检查要返回的数据的范围
            if (paging.PageIndex >= paging.PagesCount)
                paging.PageIndex = paging.PagesCount - 1;
            int startRow = paging.PageIndex * paging.PageSize;
            int endRow = startRow + paging.PageSize - 1;

            //返回指定页的数据
            DataTable dataTable = new DataTable("T");
            dataTable.Locale = System.Globalization.CultureInfo.CurrentCulture;

            //读取指定页的数据
            using (DbDataReader reader = ExecuteDataReader(cmd))
            {
                //复制表结构
                for (int j = 0; j < reader.FieldCount; ++j)
                {
                    dataTable.Columns.Add(reader.GetName(j), reader.GetFieldType(j));
                }

                //循环读取数据行
                int i = 0;
                while (reader.Read() && i <= endRow)
                {
                    //限制每页返回的记录数
                    if (cmd.ReturnCount >= 0 && dataTable.Rows.Count >= cmd.ReturnCount) break;

                    if (i >= startRow)
                    {
                        DataRow row = dataTable.NewRow();
                        for (int j = 0; j < reader.FieldCount; ++j)
                        {
                            row[j] = reader[j];
                        }
                        dataTable.Rows.Add(row);
                    }
                    i++;
                }
            }

            return dataTable;
        }

        /// <summary>
        /// 执行查询语句，返回数据读取流。
        /// 关闭数据读取流时，会自动关闭相应的数据库连接。
        /// </summary>
        /// <param name="sql">要执行的查询语句（无动态参数）。</param>
        /// <returns>数据读取流。</returns>
        /// <exception cref="ArgumentException">如果sql为空，则抛出该异常。</exception>
        public DbDataReader ExecuteDataReader(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "sql");

            return ExecuteDataReader(sql, null);
        }

        /// <summary>
        /// 执行数据访问命令，返回数据读取流。
        /// 关闭数据读取流时，会自动关闭相应的数据库连接。
        /// </summary>
        /// <param name="cmd">要执行的数据访问命令。</param>
        /// <returns>数据读取流。</returns>
        /// <exception cref="ArgumentNullException">如果cmd为空引用，则抛出该异常。</exception>
        /// <exception cref="ArgumentException">如果cmd.Sql为空，则抛出该异常。</exception>
        public DbDataReader ExecuteDataReader(DataAccessCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "cmd");

            return ExecuteDataReader(cmd.Sql, cmd.Parameters);
        }

        /// <summary>
        /// 执行查询语句，返回查询结果中第一行第一列的值。
        /// </summary>
        /// <param name="sql">要执行的查询语句（无动态参数）。</param>
        /// <returns>查询结果中第一行第一列的值。</returns>
        /// <exception cref="ArgumentException">如果sql为空，则抛出该异常。</exception>
        public object ExecuteScalar(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "sql");

            return ExecuteScalar(sql, null);
        }

        /// <summary>
        /// 执行数据访问命令，返回查询结果中第一行第一列的值。
        /// </summary>
        /// <param name="cmd">要执行的数据访问命令。</param>
        /// <returns>查询结果中第一行第一列的值。</returns>
        /// <exception cref="ArgumentNullException">如果cmd为空引用，则抛出该异常。</exception>
        /// <exception cref="ArgumentException">如果cmd.Sql为空，则抛出该异常。</exception>
        public object ExecuteScalar(DataAccessCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "cmd");

            return ExecuteScalar(cmd.Sql, cmd.Parameters);
        }

        /// <summary>
        /// 执行DML语句，返回受影响的行数。
        /// </summary>
        /// <param name="sql">要执行的DML语句（INSERT、DELETE、UPDATE，无动态参数）。</param>
        /// <returns>受影响的行数。</returns>
        /// <exception cref="ArgumentException">如果sql为空，则抛出该异常。</exception>
        public int ExecuteNonQuery(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "sql");

            return ExecuteNonQuery(sql, null, CommandType.Text);
        }

        /// <summary>
        /// 执行DML语句或存储过程，返回受影响的行数。
        /// </summary>
        /// <param name="sql">要执行的DML语句（INSERT、DELETE、UPDATE）或存储过程的名称（无动态参数）。</param>
        /// <param name="sqlType">SQL语句类型（Text、StoredProcedure、TableDirect）。</param>
        /// <returns>受影响的行数。</returns>
        /// <exception cref="ArgumentException">如果sql为空，则抛出该异常。</exception>
        public int ExecuteNonQuery(string sql, CommandType sqlType)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "sql");

            return ExecuteNonQuery(sql, null, sqlType);
        }

        /// <summary>
        /// 执行数据访问命令，返回受影响的行数。
        /// </summary>
        /// <param name="cmd">要执行的数据访问命令。</param>
        /// <returns>受影响的行数。</returns>
        /// <exception cref="ArgumentNullException">如果cmd为空引用，则抛出该异常。</exception>
        /// <exception cref="ArgumentException">如果cmd.Sql为空，则抛出该异常。</exception>
        public int ExecuteNonQuery(DataAccessCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "cmd");

            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                DbCommand command = connection.CreateCommand();
                try
                {
                    command.Transaction = connection.BeginTransaction();

                    command.CommandText = cmd.Sql;
                    command.CommandType = cmd.SqlType;
                    command.Parameters.Clear();
                    foreach (DbParameter parameter in cmd.Parameters)
                    {
                        command.Parameters.Add(CloneParameter(parameter));
                    }

                    int result = 0;

                    //如果参数列表空，或者不需要批量替换参数值，则直接执行
                    if (cmd.Parameters.Count == 0 || cmd.ValuesList.Count == 0)
                    {
                        result = command.ExecuteNonQuery();
                    }
                    else
                    {
                        //批量替换参数值，实现同一SQL语句使用不同参数值的批量执行
                        foreach (object[] values in cmd.ValuesList)
                        {
                            for (int j = 0; j < command.Parameters.Count; j++)
                            {
                                command.Parameters[j].Value = values[j];
                            }
                            int tmp = command.ExecuteNonQuery();
                            if (tmp > 0) result += tmp;
                        }
                    }

                    command.Transaction.Commit();

                    return result;
                }
                catch
                {
                    command.Transaction.Rollback();
                    throw;
                }
                finally
                {
                    command.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行一组数据访问命令。
        /// 一组数据访问命令是在同一个事务中执行的，如果失败，则全部回滚。
        /// </summary>
        /// <param name="list">要执行的一组数据访问命令。</param>
        /// <exception cref="ArgumentNullException">如果list为空引用，则抛出该异常。</exception>
        public void ExecuteNonQuery(IEnumerable<DataAccessCommand> list)
        {
            if (list == null) throw new ArgumentNullException("list");

            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                DbCommand command = connection.CreateCommand();
                try
                {
                    command.Transaction = connection.BeginTransaction();

                    foreach (DataAccessCommand cmd in list)
                    {
                        if (cmd == null || string.IsNullOrEmpty(cmd.Sql)) continue;

                        command.CommandText = cmd.Sql;
                        command.CommandType = cmd.SqlType;
                        command.Parameters.Clear();
                        foreach (DbParameter parameter in cmd.Parameters)
                        {
                            command.Parameters.Add(CloneParameter(parameter));
                        }

                        //如果参数列表空，或者不需要批量替换参数值，则直接执行
                        if (cmd.Parameters.Count == 0 || cmd.ValuesList.Count == 0)
                        {
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            //批量替换参数值，实现同一SQL语句使用不同参数值的批量执行
                            foreach (object[] values in cmd.ValuesList)
                            {
                                for (int j = 0; j < command.Parameters.Count; j++)
                                {
                                    command.Parameters[j].Value = values[j];
                                }
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    command.Transaction.Commit();
                }
                catch
                {
                    command.Transaction.Rollback();
                    throw;
                }
                finally
                {
                    command.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行SQL脚本（DML语句或存储过程）。
        /// 通过逗号分隔符，可以将多条SQL语句连接为一条语句，在同一个事务中依次执行。
        /// </summary>
        /// <param name="script">要执行的SQL脚本（DML语句或存储过程，无动态参数）。</param>
        /// <param name="ignoreComment">是否忽略SQL脚本中的注释（//、--、/**/）。</param>
        /// <param name="multiStatements">是否多条语句（语句之间用半角分号分隔）。</param>
        /// <exception cref="ArgumentException">如果script为空，则抛出该异常。</exception>
        public void ExecuteNonQuery(string script, bool ignoreComment, bool multiStatements)
        {
            if (string.IsNullOrEmpty(script))
                throw new ArgumentException(Properties.Resources.InvalidSql, "script");

            //去除注释
            if (ignoreComment)
                script = DataAccessCommand.IgnoreScriptComment(script);
            if (string.IsNullOrEmpty(script))
                throw new ArgumentException(Properties.Resources.InvalidSql, "script");

            if (multiStatements)
            {
                //在同一个事务中执行多条语句
                Collection<DataAccessCommand> commands = new Collection<DataAccessCommand>();
                foreach (string statement in DataAccessCommand.ParseScriptToStatements(script))
                {
                    DataAccessCommand command = CreateCommand(statement);
                    if (DataAccessCommand.IsProcedure(statement)) command.SqlType = CommandType.StoredProcedure;
                    commands.Add(command);
                }
                if (commands.Count > 0) ExecuteNonQuery(commands);
            }
            else if (DataAccessCommand.IsProcedure(script))
            {
                //执行存储过程
                ExecuteNonQuery(script, null, CommandType.StoredProcedure);
            }
            else
            {
                //执行普通SQL语句
                ExecuteNonQuery(script, null, CommandType.Text);
            }
        }

        /// <summary>
        /// 执行存储过程。
        /// </summary>
        /// <param name="procedure">要执行的存储过程（名称，或名称加参数，无动态参数）。</param>
        /// <exception cref="ArgumentException">如果procedure为空，则抛出该异常。</exception>
        public void ExecuteProcedure(string procedure)
        {
            if (string.IsNullOrEmpty(procedure))
                throw new ArgumentException(Properties.Resources.InvalidSql, "procedure");

            ExecuteNonQuery(procedure, null, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 根据指定的架构名称，以及限制值，返回数据库的架构信息。
        /// </summary>
        /// <param name="collectionName">架构名称。</param>
        /// <param name="restrictionValues">限制值。</param>
        /// <returns>数据库的架构信息。</returns>
        public DataTable ExecuteSchema(string collectionName, string[] restrictionValues)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return connection.GetSchema(collectionName, restrictionValues);
            }
        }

        /// <summary>
        /// 执行查询语句，返回查询结果的数量。
        /// </summary>
        /// <param name="sql">要执行的查询语句（无动态参数）。</param>
        /// <returns>查询结果的数量。</returns>
        /// <exception cref="ArgumentException">如果sql为空，则抛出该异常。</exception>
        public object ExecuteCount(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "sql");

            return ExecuteScalar(GetCountSql(sql));
        }

        /// <summary>
        /// 执行查询语句，返回查询结果的数量。
        /// </summary>
        /// <param name="cmd">要执行的数据访问命令。</param>
        /// <returns>查询结果的数量。</returns>
        /// <exception cref="ArgumentNullException">如果cmd为空引用，则抛出该异常。</exception>
        /// <exception cref="ArgumentException">如果cmd.Sql为空，则抛出该异常。</exception>
        public object ExecuteCount(DataAccessCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");
            if (string.IsNullOrEmpty(cmd.Sql))
                throw new ArgumentException(Properties.Resources.InvalidSql, "cmd");

            DataAccessCommand cmd2 = new DataAccessCommand(cmd.Parent);
            cmd2.Sql = GetCountSql(cmd.Sql);
            foreach (DbParameter param in cmd.Parameters)
            {
                cmd2.Parameters.Add(CloneParameter(param));
            }

            return ExecuteScalar(cmd2);
        }

        /// <summary>
        /// 创建数据访问命令。
        /// </summary>
        /// <returns>数据访问命令。</returns>
        public DataAccessCommand CreateCommand()
        {
            return new DataAccessCommand(this);
        }

        /// <summary>
        /// 创建数据访问命令。
        /// </summary>
        /// <param name="sql">SQL语句。</param>
        /// <returns>数据访问命令。</returns>
        public DataAccessCommand CreateCommand(string sql)
        {
            DataAccessCommand cmd = new DataAccessCommand(this);
            cmd.Sql = sql;
            return cmd;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 执行查询语句，返回数据读取器。
        /// 关闭数据读取器时，会自动关闭相应的数据库连接。
        /// </summary>
        /// <param name="sql">要执行的查询语句（无动态参数）。</param>
        /// <param name="parameters">参数列表。不需要任何参数时，可以传递空引用或空集合。</param>
        /// <returns>数据读取器。</returns>
        private DbDataReader ExecuteDataReader(string sql, IEnumerable<DbParameter> parameters)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = ConnectionString;
            connection.Open();
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.Clear();
                if (parameters != null)
                {
                    foreach (DbParameter parameter in parameters)
                    {
                        command.Parameters.Add(CloneParameter(parameter));
                    }
                }
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }

        /// <summary>
        /// 执行查询语句，返回指定行数的查询结果。
        /// </summary>
        /// <param name="sql">要执行的查询语句。</param>
        /// <param name="parameters">参数集合。不需要任何参数时，可以传递空引用或空集合。</param>
        /// <param name="returnCount">返回行数（如果小于0，则返回所有行）。</param>
        /// <returns>查询结果。</returns>
        private DataTable ExecuteDataTable(string sql, IEnumerable<DbParameter> parameters, int returnCount)
        {
            DataTable dataTable = new DataTable("T");
            dataTable.Locale = System.Globalization.CultureInfo.CurrentCulture;

            if (returnCount >= 0)
            {
                using (DbDataReader reader = ExecuteDataReader(sql, parameters))
                {
                    //复制表结构
                    for (int j = 0; j < reader.FieldCount; ++j)
                    {
                        dataTable.Columns.Add(reader.GetName(j), reader.GetFieldType(j));
                    }

                    //循环读取数据行
                    int i = 0;
                    while (reader.Read() && (i < returnCount || returnCount < 0))
                    {
                        DataRow row = dataTable.NewRow();
                        for (int j = 0; j < reader.FieldCount; ++j)
                        {
                            row[j] = reader[j];
                        }
                        dataTable.Rows.Add(row);
                        i++;
                    }
                }
            }
            else
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
                using (DbConnection connection = factory.CreateConnection())
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    using (DbDataAdapter adapter = factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = connection.CreateCommand();
                        adapter.SelectCommand.CommandText = sql;
                        adapter.SelectCommand.Parameters.Clear();
                        if (parameters != null)
                        {
                            foreach (DbParameter parameter in parameters)
                            {
                                adapter.SelectCommand.Parameters.Add(CloneParameter(parameter));
                            }
                        }
                        adapter.Fill(dataTable);
                    }
                }
            }

            return dataTable;
        }

        /// <summary>
        /// 执行查询语句，返回查询结果中第一行第一列的值。
        /// </summary>
        /// <param name="sql">要执行的查询语句。</param>
        /// <param name="parameters">参数集合。不需要任何参数时，可以传递空引用或空集合。</param>
        /// <returns>查询结果中第一行第一列的值。</returns>
        private object ExecuteScalar(string sql, IEnumerable<DbParameter> parameters)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.Clear();
                    if (parameters != null)
                    {
                        foreach (DbParameter parameter in parameters)
                        {
                            command.Parameters.Add(CloneParameter(parameter));
                        }
                    }
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 执行DML语句或存储过程，返回受影响的行数。
        /// </summary>
        /// <param name="sql">要执行的DML语句（INSERT、DELETE、UPDATE）或存储过程的名称，可以有动态参数。</param>
        /// <param name="parameters">参数集合。不需要任何参数时，可以传递空引用或空集合。</param>
        /// <param name="sqlType">命令类型（Text、StoredProcedure、TableDirect）。</param>
        /// <returns>受影响的行数。</returns>
        private int ExecuteNonQuery(string sql, IEnumerable<DbParameter> parameters, CommandType sqlType)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                DbCommand command = factory.CreateCommand();
                command.Connection = connection;
                try
                {
                    command.Transaction = connection.BeginTransaction();
                    command.CommandText = sql;
                    command.CommandType = sqlType;
                    command.Parameters.Clear();
                    if (parameters != null)
                    {
                        foreach (DbParameter parameter in parameters)
                        {
                            command.Parameters.Add(CloneParameter(parameter));
                        }
                    }

                    int result = command.ExecuteNonQuery();
                    command.Transaction.Commit();

                    return result;
                }
                catch
                {
                    command.Transaction.Rollback();
                    throw;
                }
                finally
                {
                    command.Dispose();
                }
            }
        }

        /// <summary>
        /// 克隆动态参数对象。
        /// </summary>
        /// <param name="parameter">要克隆的参数对象。</param>
        /// <returns>新的动态参数对象。</returns>
        private DbParameter CloneParameter(DbParameter parameter)
        {
            if (parameter == null) return null;

            DbProviderFactory factory = DbProviderFactories.GetFactory(Provider);
            DbParameter parameter2 = factory.CreateParameter();
            parameter2.ParameterName = parameter.ParameterName;
            parameter2.Value = parameter.Value;
            parameter2.DbType = parameter.DbType;
            parameter2.Direction = parameter.Direction;
            parameter2.Size = parameter.Size;
            parameter2.SourceColumn = parameter.SourceColumn;
            parameter2.SourceColumnNullMapping = parameter.SourceColumnNullMapping;
            parameter2.SourceVersion = parameter.SourceVersion;

            return parameter2;
        }

        #endregion

        #region 静态方法

        /// <summary>
        /// 获取指定SQL语句对应的COUNT语句。
        /// </summary>
        /// <param name="sql">SQL语句。</param>
        /// <returns>COUNT语句。</returns>
        public static string GetCountSql(string sql)
        {
            return "SELECT COUNT(*) FROM (" + Environment.NewLine + sql + Environment.NewLine + ") V";
        }

        /// <summary>
        /// 将指定对象转换为String类型。
        /// </summary>
        /// <param name="obj">要转换的对象。</param>
        /// <returns>转换后的值。</returns>
        public static string GetStringValue(object obj)
        {
            if (obj == null || obj == DBNull.Value) return string.Empty;
            if (string.IsNullOrEmpty(obj.ToString())) return string.Empty;
            return obj.ToString();
        }

        /// <summary>
        /// 将指定对象转换为DateTime类型。
        /// </summary>
        /// <param name="obj">要转换的对象。</param>
        /// <returns>转换后的值。</returns>
        public static DateTime GetDateTimeValue(object obj)
        {
            if (obj == null || obj == DBNull.Value) return DateTime.MinValue;
            if (string.IsNullOrEmpty(obj.ToString())) return DateTime.MinValue;
            return Convert.ToDateTime(obj);
        }

        /// <summary>
        /// 将指定对象转换为Int32类型。
        /// </summary>
        /// <param name="obj">要转换的对象。</param>
        /// <returns>转换后的值。</returns>
        public static int GetInt32Value(object obj)
        {
            if (obj == null || obj == DBNull.Value) return 0;
            if (string.IsNullOrEmpty(obj.ToString())) return 0;
            return Convert.ToInt32(obj);
        }

        /// <summary>
        /// 将指定对象转换为Int64类型。
        /// </summary>
        /// <param name="obj">要转换的对象。</param>
        /// <returns>转换后的值。</returns>
        public static long GetInt64Value(object obj)
        {
            if (obj == null || obj == DBNull.Value) return 0;
            if (string.IsNullOrEmpty(obj.ToString())) return 0;
            return Convert.ToInt64(obj);
        }

        /// <summary>
        /// 将指定对象转换为Single类型。
        /// </summary>
        /// <param name="obj">要转换的对象。</param>
        /// <returns>转换后的值。</returns>
        public static float GetSingleValue(object obj)
        {
            if (obj == null || obj == DBNull.Value) return 0;
            if (string.IsNullOrEmpty(obj.ToString())) return 0;
            return Convert.ToSingle(obj);
        }

        /// <summary>
        /// 将指定对象转换为Decimal类型。
        /// </summary>
        /// <param name="obj">要转换的对象。</param>
        /// <returns>转换后的值。</returns>
        public static decimal GetDecimalValue(object obj)
        {
            if (obj == null || obj == DBNull.Value) return 0;
            if (string.IsNullOrEmpty(obj.ToString())) return 0;
            return Convert.ToDecimal(obj);
        }

        /// <summary>
        /// 将指定对象转换为Byte[]类型。
        /// </summary>
        /// <param name="obj">要转换的对象。</param>
        /// <returns>转换后的值。</returns>
        public static byte[] GetBytesValue(object obj)
        {
            if (obj == null || obj == DBNull.Value) return new byte[] { };
            byte[] bytes = obj as byte[];
            if (bytes == null) bytes = new byte[] { };
            return bytes;
        }

        /// <summary>
        /// 将指定String值转换为数据库值。
        /// </summary>
        /// <param name="obj">要转换的值。</param>
        /// <returns>转换后的值。</returns>
        public static object GetStringDbValue(string obj)
        {
            if (string.IsNullOrEmpty(obj)) return DBNull.Value;
            return obj;
        }

        /// <summary>
        /// 将指定DateTime值转换为数据库值。
        /// </summary>
        /// <param name="obj">要转换的值。</param>
        /// <returns>转换后的值。</returns>
        public static object GetDateTimeDbValue(DateTime obj)
        {
            if (obj == DateTime.MinValue) return DBNull.Value;
            return obj;
        }

        /// <summary>
        /// 将指定Int32值转换为数据库值。
        /// </summary>
        /// <param name="obj">要转换的值。</param>
        /// <returns>转换后的值。</returns>
        public static object GetInt32DbValue(int obj)
        {
            return obj;
        }

        /// <summary>
        /// 将指定Int64值转换为数据库值。
        /// </summary>
        /// <param name="obj">要转换的值。</param>
        /// <returns>转换后的值。</returns>
        public static object GetInt64DbValue(long obj)
        {
            return obj;
        }

        /// <summary>
        /// 将指定Single值转换为数据库值。
        /// </summary>
        /// <param name="obj">要转换的值。</param>
        /// <returns>转换后的值。</returns>
        public static object GetSingleDbValue(float obj)
        {
            return obj;
        }

        /// <summary>
        /// 将指定Decimal值转换为数据库值。
        /// </summary>
        /// <param name="obj">要转换的值。</param>
        /// <returns>转换后的值。</returns>
        public static object GetDecimalDbValue(decimal obj)
        {
            return obj;
        }

        /// <summary>
        /// 将指定Byte[]值转换为数据库值。
        /// </summary>
        /// <param name="obj">要转换的值。</param>
        /// <returns>转换后的值。</returns>
        public static object GetBytesDbValue(byte[] obj)
        {
            if (obj == null) return new byte[] { };
            return obj;
        }

        #endregion
    }
}
