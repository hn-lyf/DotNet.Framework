using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using DotNet.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using System.Linq;

namespace DotNet.Data
{
    /// <summary>
    /// 数据库操作辅助类。
    /// </summary>
    public class DbHelper : IDisposable
    {
#if !NETFRAMEWORK
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static DbHelper()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
        }
#endif
        /// <summary>
        /// 析构函数，释放资源。
        /// </summary>
        ~DbHelper()
        {
            Dispose(false);
        }
        /// <summary>
        /// 使用默认数据库的辅助类。
        /// <para>注意：如果配置文件中有名称为default的节点，如果节点不存在则使用Server=.;Integrated Security=SSPI;Database=master</para>
        /// </summary>
        public static DbHelper Default
        {
            get
            {
#if NETFRAMEWORK

                if (ConfigurationManager.ConnectionStrings.Count <= 0)
                {
                    return new DbHelper("Server=.;Integrated Security=SSPI;Database=master", SqlClientFactory.Instance);
                }
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["default"] ?? ConfigurationManager.ConnectionStrings[0];
                return new DbHelper(settings.ConnectionString, settings.ProviderName);
#else
                var connectionString = Configuration.GetConnectionString();
                if (connectionString != null)
                {
                    return new DbHelper(connectionString, Configuration.ConnectionProviderName);
                }
                return new DbHelper("Server=.;Integrated Security=SSPI;Database=master", SqlClientFactory.Instance);
#endif

            }
        }
        /// <summary>
        /// 根据提供程序的固定名称获取<see cref="DbProviderFactory"/>对象。
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public virtual DbProviderFactory GetFactory(string providerName)
        {
#if NETFRAMEWORK
            return DbProviderFactories.GetFactory(providerName);
#else
            if (DbProviderFactories.TryGetFactory(providerName, out DbProviderFactory providerFactory))
            {
                return providerFactory;
            }
            var providerFactoryType = typeof(DbProviderFactory);
            foreach (var assemblie in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assemblie.GlobalAssemblyCache)
                {
                    continue;
                }
                var factoryType = assemblie.GetTypes().FirstOrDefault((t) => t.Namespace == providerName && t.IsSubclassOf(providerFactoryType));
                if (factoryType != null)
                {
                    DbProviderFactories.RegisterFactory(factoryType.Namespace, factoryType);
                    return DbProviderFactories.GetFactory(providerName);
                }
            }
            var path = System.IO.Path.GetFullPath(System.IO.Path.ChangeExtension(providerName, "dll"));
            if (System.IO.File.Exists(path))
            {
                var assembly = Assembly.LoadFile(path);
                var factoryType = assembly.GetTypes().FirstOrDefault((t) => t.Namespace == providerName && t.IsSubclassOf(providerFactoryType));
                if (factoryType != null)
                {
                    DbProviderFactories.RegisterFactory(factoryType.Namespace, factoryType);
                    return DbProviderFactories.GetFactory(providerName);
                }
            }
            return null;
#endif

        }
        /// <summary>
        /// 使用连接字符串初始化<see cref="DbHelper"/>辅助类。<para>注意：为SQL Server数据库。</para>
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        public DbHelper(string connectionString)
            : this(connectionString, string.Empty)
        {

        }
        /// <summary>
        /// 使用提供程序的固定名称和连接字符串初始化<see cref="DbHelper"/>辅助类。
        /// </summary>
        /// <param name="providerName">提供程序的固定名称。
        /// <para>注意：如果<see cref="string.IsNullOrEmpty(string)"/>返回真则使用<see cref="System.Data.SqlClient.SqlClientFactory.Instance"/>进行初始化。</para>
        /// <para>取值范围一般有：</para>
        /// <para>System.Data.Odbc、System.Data.OleDb、System.Data.OracleClient、System.Data.SqlClient、System.Data.SqlServerCe.4.0</para>
        /// </param>
        /// <param name="connectionString">连接字符串。</param>
        public DbHelper(string connectionString, string providerName)
        {
            this.m_ConnectionString = connectionString;
            m_ProviderFactory = string.IsNullOrEmpty(providerName) ? System.Data.SqlClient.SqlClientFactory.Instance : GetFactory(providerName);
        }
        /// <summary>
        /// 使用提供程序和连接字符串初始化<see cref="DbHelper"/>辅助类。
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <param name="providerFactory">提供程序对数据源类的实现的实例。</param>
        public DbHelper(string connectionString, DbProviderFactory providerFactory)
        {
            m_ProviderFactory = providerFactory;
            this.m_ConnectionString = connectionString;
        }
        private readonly DbProviderFactory m_ProviderFactory;
        private DbConnection m_Connection;
        private string m_ConnectionString;
        private DbTransaction m_Transaction;
        /// <summary>
        /// 创建提供程序对数据源类的实现的实例。
        /// </summary>
        protected virtual DbProviderFactory DbFactory
        {
            get
            {
                return m_ProviderFactory;
            }
        }
        /// <summary>
        /// 获取或设置一个链接字符串，该值标示此帮助类的数据库连接字符串。
        /// </summary>
        public virtual string ConnectionString
        {
            get
            {
                return m_ConnectionString;
            }
            set
            {
                m_ConnectionString = value;
            }
        }
        /// <summary>
        /// 获取一个值，该值表示要在数据库中处理的 Transact-SQL 事务。
        /// </summary>
        public virtual DbTransaction Transaction { get { return m_Transaction; } }
        /// <summary>
        /// 获取一个值，该值指示当前是否处于事务中。
        /// </summary>
        public virtual bool IsTransaction { get { return Transaction != null; } }
        /// <summary>
        /// 获取一个值，该值表示一个到数据库的打开的连接。 
        /// </summary>
        public virtual DbConnection Connection
        {
            get
            {
                if (m_Connection == null)
                {
                    m_Connection = DbFactory.CreateConnection();
                    m_Connection.ConnectionString = ConnectionString;
                }
                return m_Connection;
            }
        }
        /// <summary>
        /// 使用命令文本创建<see cref="DbCommand"/>对象。
        /// </summary>
        /// <param name="commandText">要执行的命令文本。</param>
        /// <returns><see cref="DbCommand"/>对象。</returns>
        protected virtual DbCommand CreateCommand(string commandText)
        {
            var command = DbFactory.CreateCommand();
            command.Connection = Connection;
            command.CommandText = commandText;
            if (this.IsTransaction)
            {
                command.Transaction = this.Transaction;
            }
            return command;
        }
        /// <summary>
        /// 根据参数名称、参数值创建<see cref="DbParameter"/>对象。
        /// </summary>
        /// <typeparam name="T">参数值类型。</typeparam>
        /// <param name="name">参数名称。</param>
        /// <param name="value">参数值</param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter<T>(string name, T value)
        {
            var parameter = DbFactory.CreateParameter();
            parameter.ParameterName = string.Format("@{0}", name);
            parameter.Value = value;
            return parameter;
        }
        /// <summary>
        /// 向<see cref="DbCommand"/>命令中添加参数。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="command">see cref="DbCommand"/>命令</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。</param>
        protected virtual void AddParameters<P>(DbCommand command, P parameters)
        {
            if (!command.CommandText.StartsWith("sys.", StringComparison.OrdinalIgnoreCase))
            {
                if (command.CommandType == CommandType.StoredProcedure)
                {
                    var returnParameter = CreateParameter("ReturnIntValue", DBNull.Value);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.Add(returnParameter);
                }
            }
            if (parameters.IsNull())
            {
                return;
            }
            else
            if (parameters is System.Collections.Generic.IEnumerable<DbParameter> dbParameters)
            {
                foreach (DbParameter parameter in dbParameters)
                {
                    if ((parameter.Value == null) && ((parameter.Direction | ParameterDirection.Input) == ParameterDirection.Input))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);

                }

            }
            else
            {
                var itmes = parameters.ToEnumerable();
                foreach (var entry in itmes)
                {
                    command.Parameters.Add(CreateParameter(entry.Key, entry.Value ?? DBNull.Value));
                }
            }


        }
        /// <summary>
        /// 打开数据库连接。
        /// </summary>
        protected virtual void OpenConnection()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }
        /// <summary>
        /// 开始数据库事务。
        /// </summary>
        /// <param name="isolationLevel">指定连接的事务锁定行为。（其中比较经典的是<see cref="IsolationLevel.Unspecified"/>）</param>
        /// <exception cref="ConstraintException">当<see cref="IsTransaction"/>为true时则引发此异常。</exception>
        public virtual void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (IsTransaction)
            {
                throw new ConstraintException("已处于存在事务中。");
            }
            OpenConnection();
            m_Transaction = Connection.BeginTransaction(isolationLevel);
        }
        /// <summary>
        /// 提交数据库事务。
        /// </summary>
        /// <returns>返回一个<see cref="Result"/>结果，该结果指示此次提交是否成功。</returns>
        public virtual Result Commit()
        {
            Result result = new Result()
            {
                Message = "未处于事务中"
            };
            if (this.IsTransaction)
            {
                try
                {
                    this.Transaction.Commit();
                    this.m_Transaction = null;
                    result.Success = true;
                    result.Message = "操作成功";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
                finally
                {
                    CloseConnection();
                }
            }
            return result;
        }
        /// <summary>
        /// 从挂起状态回滚事务。
        /// </summary>
        /// <returns>返回一个<see cref="Result"/>结果，该结果指示此次回滚是否成功。</returns>
        public virtual Result Rollback()
        {
            Result result = new Result()
            {
                Message = "未处于事务中"
            };
            if (this.IsTransaction)
            {
                try
                {
                    this.Transaction.Rollback();
                    this.m_Transaction = null;
                    result.Success = true;
                    result.Message = "操作成功";
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message;
                }
                finally
                {
                    CloseConnection();
                }
            }
            return result;
        }
        /// <summary>
        /// 关闭数据库连接。
        ///  <para>通常数据库会自动关闭。</para>
        /// </summary>
        /// <returns></returns>
        public virtual Result CloseConnection()
        {
            Result result = new Result();
            try
            {
                using (Connection)
                {
                    if (Connection.State != ConnectionState.Closed)
                    {
                        Connection.Close();
                    }
                }
                Connection.ConnectionString = ConnectionString;
                result.Success = true;
                result.Message = "操作成功";
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        #region 事件
        static EventHandler<ErrorEventArgs> m_StaticError;
        /// <summary>
        /// 当发生错误时执行的事件。
        /// </summary>
        public static event EventHandler<ErrorEventArgs> StaticError { add { m_StaticError += value; } remove { m_StaticError -= value; } }
        static void OnStaticError(DbHelper helper, ErrorEventArgs e)
        {
            try
            {
                m_StaticError?.Invoke(helper, e);
            }
            catch
            {

            }
        }

        private EventHandler<ErrorEventArgs> m_Error;

        /// <summary>
        /// 当发生错误时执行的事件。
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error { add { m_Error += value; } remove { m_Error -= value; } }
        /// <summary>
        /// 发生错误。
        /// </summary>
        /// <param name="exception">错误信息。</param>
        protected virtual void OnError(Exception exception)
        {

            var e = new ErrorEventArgs(exception);
            OnStaticError(this, e);
            try
            {
                m_Error?.Invoke(this, e);
            }
            catch
            {

            }
        }
        #endregion
        /// <summary>
        /// 执行命令。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <typeparam name="R">返回值类型。</typeparam>
        /// <param name="result">返回值。</param>
        /// <param name="action">执行命令。</param>
        /// <param name="commandText">要执行的命令文本</param>
        /// <param name="isClose">执行完后是否关闭数据库连接。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 Text。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。</param>
        protected virtual void Execute<P, R>(R result, Action<DbCommand, R> action, string commandText, bool isClose = true, CommandType commandType = CommandType.Text, P parameters = default)
             where R : Result
        {

            using (var command = CreateCommand(commandText))
            {
                command.CommandType = commandType;
                AddParameters(command, parameters);
                try
                {
                    OpenConnection();
                    action(command, result);
                    result.Message = "执行成功";
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                    result.Code = -1;
                    OnError(ex);
                }
                finally
                {
                    command.Parameters.Clear();
                    if (isClose && !IsTransaction)//如果为事务模式也不能关闭。
                    {
                        CloseConnection();
                    }
                }
            }
        }
#if !NET40
        /// <summary>
        /// 执行命令。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <typeparam name="R">返回值类型。</typeparam>
        /// <param name="result">返回值。</param>
        /// <param name="action">执行命令。</param>
        /// <param name="commandText">要执行的命令文本</param>
        /// <param name="isClose">执行完后是否关闭数据库连接。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 Text。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。</param>
        protected virtual async Task ExecuteAsync<P, R>(R result, Func<DbCommand, R, Task> action, string commandText, bool isClose = true, CommandType commandType = CommandType.Text, P parameters = default)
             where R : Result
        {

            using (var command = CreateCommand(commandText))
            {


                command.CommandType = commandType;
                AddParameters(command, parameters);
                try
                {
                    OpenConnection();
                    await action(command, result);
                    result.Message = "执行成功";
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                    result.Code = -1;
                    OnError(ex);
                }
                finally
                {
                    command.Parameters.Clear();
                    if (isClose && !IsTransaction)//如果为事务模式也不能关闭。
                    {
                        CloseConnection();
                    }
                }
            }
        }
#endif
        /// <summary>
        /// 执行命令语句并返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。</returns>
        public virtual Result ExecuteSqlNonQuery<P>(string commandText, P parameters)
        {
            return ExecuteNonQuery(commandText, CommandType.Text, parameters);
        }
        /// <summary>
        /// 执行命令语句并返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <returns>返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。</returns>
        public virtual Result ExecuteSqlNonQuery(string commandText)
        {
            return ExecuteNonQuery(commandText, CommandType.Text);
        }
        /// <summary>
        /// 执行存储过程并返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="procedureName">要执行的存储过程名称。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。</returns>
        public virtual Result ExecutePrcNonQuery<P>(string procedureName, P parameters)
        {
            return ExecuteNonQuery(procedureName, CommandType.StoredProcedure, parameters);
        }
        /// <summary>
        /// 执行存储过程并返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。
        /// </summary>
        /// <param name="procedureName">要执行的存储过程名称。</param>
        /// <returns>返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。</returns>
        public virtual Result ExecutePrcNonQuery(string procedureName)
        {
            return ExecuteNonQuery(procedureName, CommandType.Text);
        }
        /// <summary>
        /// 执行命令语句并返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 Text。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。</returns>
        public virtual Result ExecuteNonQuery<P>(string commandText, CommandType commandType = CommandType.Text, P parameters = default)
        {
            var result = new Result();
            Execute(result, (m, r) =>
            {
                r.Code = m.ExecuteNonQuery();
            }, commandText, true, commandType, parameters);
            return result;
        }
#if !NET40
        /// <summary>
        /// 执行命令语句并返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 Text。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。</returns>
        public virtual async Task<Result> ExecuteNonQueryAsync<P>(string commandText, CommandType commandType = CommandType.Text, P parameters = default)
        {
            var result = new Result();
            await ExecuteAsync(result, async (m, r) =>
            {
                r.Code = await m.ExecuteNonQueryAsync();
            }, commandText, true, commandType, parameters);
            return result;
        }
        /// <summary>
        /// 执行命令语句并返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="System.Data.CommandType.Text"/>。</param>
        /// <returns>返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。</returns>
        public virtual async Task<Result> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.Text)
        {
            return await ExecuteNonQueryAsync<DbParameter>(commandText, commandType, null);
        }
#endif
        /// <summary>
        /// 执行命令语句并返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="System.Data.CommandType.Text"/>。</param>
        /// <returns>返回<see cref="Result"/>结果，结果中<see cref="Result.Code"/>的值为受影响的行数。</returns>
        public virtual Result ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text)
        {
            return ExecuteNonQuery<DbParameter>(commandText, commandType, null);
        }
#if !NET40
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 Text。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。</returns>
        public virtual async Task<Result<object>> ExecuteScalarAsync<P>(string commandText, CommandType commandType = CommandType.Text, P parameters = default)
        {
            var result = new Result<object>();
            await ExecuteAsync(result, async (m, r) =>
            {
                r.Data = await m.ExecuteScalarAsync();
            }, commandText, true, commandType, parameters);
            return result;
        }
#endif
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 Text。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。</returns>
        public virtual Result<object> ExecuteScalar<P>(string commandText, CommandType commandType = CommandType.Text, P parameters = default)
        {
            var result = new Result<object>();
            Execute(result, (m, r) =>
            {
                r.Data = m.ExecuteScalar();
            }, commandText, true, commandType, parameters);
            return result;
        }
#if !NET40
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为<see cref="System.Data.CommandType.Text"/>。</param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。</returns>
        public virtual async Task<Result<object>> ExecuteScalarAsync(string commandText, CommandType commandType = CommandType.Text)
        {
            return await ExecuteScalarAsync<DbParameter>(commandText, commandType, null);
        }
#endif
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为<see cref="System.Data.CommandType.Text"/>。</param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。</returns>
        public virtual Result<object> ExecuteScalar(string commandText, CommandType commandType = CommandType.Text)
        {
            return ExecuteScalar<DbParameter>(commandText, commandType, null);
        }

        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DataTable"/>结果集。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="System.Data.CommandType.Text"/>。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DataTable"/>结果集。</returns>
        public virtual Result<DataTable> ExecuteTable<P>(string commandText, CommandType commandType = CommandType.Text, P parameters = default)
        {
            var result = new Result<DataTable>();
            Execute(result, (m, r) =>
            {
                using (DbDataAdapter da = DbFactory.CreateDataAdapter())
                {
                    da.SelectCommand = m;
                    DataTable table = new DataTable();
                    da.Fill(table);
                    r.Data = table;
                    if (commandType == CommandType.StoredProcedure && m.Parameters["@ReturnIntValue"].Value is int code)
                    {
                        r.Code = code;
                    }
                }
            }, commandText, true, commandType, parameters);
            return result;
        }

        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DataTable"/>结果集。
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="System.Data.CommandType.Text"/>。</param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DataTable"/>结果集。</returns>
        public virtual Result<DataTable> ExecuteTable(string commandText, CommandType commandType = CommandType.Text)
        {
            return ExecuteTable<DbParameter>(commandText, commandType, null);
        }

        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DataSet"/>结果集。
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="System.Data.CommandType.Text"/>。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DataSet"/>结果集。</returns>
        public virtual Result<DataSet> ExecuteDataSet<P>(string commandText, CommandType commandType = CommandType.Text, P parameters = default)
        {
            var result = new Result<DataSet>();
            Execute(result, (m, r) =>
            {
                using (DbDataAdapter da = DbFactory.CreateDataAdapter())
                {
                    da.SelectCommand = m;
                    DataSet dataSet = new DataSet();
                    da.Fill(dataSet);
                    r.Data = dataSet;
                }
            }, commandText, true, commandType, parameters);
            return result;
        }
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DataSet"/>结果集。
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="CommandType.Text"/>。</param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DataSet"/>结果集。</returns>
        public virtual Result<DataSet> ExecuteDataSet(string commandText, CommandType commandType = CommandType.Text)
        {
            return ExecuteDataSet<DbParameter>(commandText, commandType, null);
        }
#if !NET40
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DbDataReader"/>结果集。
        /// <para>注意：需要手动释放资源。</para>
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="CommandType.Text"/>。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DbDataReader"/>结果集。</returns>
        public virtual async Task<Result<DbDataReader>> ExecuteReaderAsync<P>(string commandText, CommandType commandType = CommandType.Text, P parameters = default)
        {
            var result = new Result<DbDataReader>();
            await ExecuteAsync(result, async (m, r) =>
            {
                r.Data = await m.ExecuteReaderAsync(IsTransaction ? CommandBehavior.Default : CommandBehavior.CloseConnection);
                if (commandType == CommandType.StoredProcedure && m.Parameters["@ReturnIntValue"].Value is int code)
                {
                    r.Code = code;
                }
            }, commandText, false, commandType, parameters);
            return result;
        }
#endif
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DbDataReader"/>结果集。
        /// <para>注意：需要手动释放资源。</para>
        /// </summary>
        /// <typeparam name="P">参数类型。</typeparam>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="CommandType.Text"/>。</param>
        /// <param name="parameters">SQL 语句或存储过程的参数。<para>注意：可以为匿名类，例如：new{A=1},那么参数就是A</para></param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DbDataReader"/>结果集。</returns>
        public virtual Result<DbDataReader> ExecuteReader<P>(string commandText, CommandType commandType = CommandType.Text, P parameters = default)
        {
            var result = new Result<DbDataReader>();
            Execute(result, (m, r) =>
            {
                r.Data = m.ExecuteReader(IsTransaction ? CommandBehavior.Default : CommandBehavior.CloseConnection);
                if (commandType == CommandType.StoredProcedure && m.Parameters["@ReturnIntValue"].Value is int code)
                {
                    r.Code = code;
                }
            }, commandText, false, commandType, parameters);
            return result;
        }
#if !NET40
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DbDataReader"/>结果集。
        /// <para>注意：需要手动释放资源。</para>
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="CommandType.Text"/>。</param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DbDataReader"/>结果集。</returns>
        public virtual async Task<Result<DbDataReader>> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.Text)
        {
            return await ExecuteReaderAsync<DbParameter>(commandText, commandType, null);
        }
#endif
        /// <summary>
        /// 执行命令语句并返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DbDataReader"/>结果集。
        /// <para>注意：需要手动释放资源。</para>
        /// </summary>
        /// <param name="commandText">要执行的文本命令。</param>
        /// <param name="commandType">指定如何解释<paramref name="commandText"/>参数，<see cref="System.Data.CommandType"/>值之一。默认值为 <see cref="CommandType.Text"/>。</param>
        /// <returns>返回<see cref="Result{T}"/>结果，结果中<see cref="Result{T}.Data"/>的值为查询所返回的<see cref="DbDataReader"/>结果集。</returns>
        public virtual Result<DbDataReader> ExecuteReader(string commandText, CommandType commandType = CommandType.Text)
        {
            return ExecuteReader<DbParameter>(commandText, commandType, null);
        }

        /// <summary>
        /// 执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        /// <param name="disposing">是否释放托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsTransaction)
            {
                Rollback();
            }
            CloseConnection();
            if (disposing)
            {
                //释放托管资源

                //只做事务回滚
                //this.m_Connection = null;
                //this.m_Error = null;
                //this.m_ProviderFactory = null;
                //this.m_Transaction = null;
            }
        }
        /// <summary>
        /// 执行存储过程并返回一个<see cref="DbDataReader"/>
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <returns></returns>
        public virtual Result<DbDataReader> QueryPrcToDataReader(string procedureName)
        {
            return this.ExecuteReader(procedureName, CommandType.StoredProcedure);
        }
        /// <summary>
        /// 执行存储过程并返回一个<see cref="DbDataReader"/>
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">存储过程所需要的参数。</param>
        /// <returns></returns>
        public virtual Result<DbDataReader> QueryPrcToDataReader<P>(string procedureName, P parameters) =>
            this.ExecuteReader<P>(procedureName, CommandType.StoredProcedure, parameters);
        /// <summary>
        /// 执行存储过程并返回一个<see cref="DataSet"/>
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <returns></returns>
        public virtual Result<DataSet> QueryPrcToDataSet(string procedureName) =>
            this.ExecuteDataSet(procedureName, CommandType.StoredProcedure);
        /// <summary>
        /// 执行存储过程并返回一个<see cref="DataSet"/>
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">存储过程所需要的参数。</param>
        /// <returns></returns>
        public virtual Result<DataSet> QueryPrcToDataSet<P>(string procedureName, P parameters) =>
            this.ExecuteDataSet<P>(procedureName, CommandType.StoredProcedure, parameters);
        /// <summary>
        /// 执行存储过程并返回第一行第一列的值。
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <returns></returns>
        public virtual Result<object> QueryPrcToScalar(string procedureName) =>
            this.ExecuteScalar(procedureName, CommandType.StoredProcedure);
        /// <summary>
        /// 执行存储过程并返回第一行第一列的值。
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">存储过程所需要的参数</param>
        /// <returns></returns>
        public virtual Result<object> QueryPrcToScalar<P>(string procedureName, P parameters) =>
            this.ExecuteScalar<P>(procedureName, CommandType.StoredProcedure, parameters);
        /// <summary>
        /// 执行存储过程并返回一个<see cref="DataTable"/>
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <returns></returns>
        public virtual Result<DataTable> QueryPrcToTable(string procedureName) =>
            this.ExecuteTable(procedureName, CommandType.StoredProcedure);
        /// <summary>
        /// 执行存储过程并返回一个<see cref="DataTable"/>
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">存储过程所需要的参数。</param>
        /// <returns></returns>
        public virtual Result<DataTable> QueryPrcToTable<P>(string procedureName, P parameters) =>
            this.ExecuteTable<P>(procedureName, CommandType.StoredProcedure, parameters);
        /// <summary>
        /// 执行T-SQL语句并返回一个<see cref="DbDataReader"/>
        /// </summary>
        /// <param name="commandText">T-SQL语句</param>
        /// <returns></returns>
        public virtual Result<DbDataReader> QuerySqlToDataReader(string commandText) =>
            this.ExecuteReader(commandText, CommandType.Text);
        /// <summary>
        /// 执行T-SQL语句并返回一个<see cref="DbDataReader"/>
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="commandText">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句所需要的参数</param>
        /// <returns></returns>
        public virtual Result<DbDataReader> QuerySqlToDataReader<P>(string commandText, P parameters) =>
            this.ExecuteReader<P>(commandText, CommandType.Text, parameters);
        /// <summary>
        /// 执行T-SQL语句并返回一个<see cref="DataSet"/>
        /// </summary>
        /// <param name="commandText">T-SQL语句</param>
        /// <returns></returns>
        public virtual Result<DataSet> QuerySqlToDataSet(string commandText) =>
            this.ExecuteDataSet(commandText, CommandType.Text);
        /// <summary>
        /// 执行T-SQL语句并返回一个<see cref="DataSet"/>
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="commandText">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句所需要的参数</param>
        /// <returns></returns>
        public virtual Result<DataSet> QuerySqlToDataSet<P>(string commandText, P parameters) =>
            this.ExecuteDataSet<P>(commandText, CommandType.Text, parameters);
        /// <summary>
        /// 执行T-SQL语句并返回第一行第一列的值。
        /// </summary>
        /// <param name="commandText">T-SQL语句</param>
        /// <returns></returns>
        public virtual Result<object> QuerySqlToScalar(string commandText) =>
            this.ExecuteScalar(commandText, CommandType.Text);
        /// <summary>
        /// 执行T-SQL语句并返回第一行第一列的值。
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="commandText">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句所需要的参数</param>
        /// <returns></returns>
        public virtual Result<object> QuerySqlToScalar<P>(string commandText, P parameters) =>
            this.ExecuteScalar<P>(commandText, CommandType.Text, parameters);
        /// <summary>
        /// 执行T-SQL语句并返回一个<see cref="DataTable"/>
        /// </summary>
        /// <param name="commandText">T-SQL语句</param>
        /// <returns></returns>
        public virtual Result<DataTable> QuerySqlToTable(string commandText) =>
            this.ExecuteTable(commandText, CommandType.Text);
        /// <summary>
        /// 执行T-SQL语句并返回一个<see cref="DataTable"/>
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="commandText">T-SQL语句</param>
        /// <param name="parameters">T-SQL语句所需要的参数</param>
        /// <returns></returns>
        public virtual Result<DataTable> QuerySqlToTable<P>(string commandText, P parameters) =>
            this.ExecuteTable<P>(commandText, CommandType.Text, parameters);
        /// <summary>
        /// 释放资源
        /// <para>如果<see cref="DbHelper"/>处于事务模式（<see cref="IsTransaction"/>为true）则会将该事务进行回滚操作。</para>
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
        }
    }
}
