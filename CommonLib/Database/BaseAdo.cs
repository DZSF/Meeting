using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.Database
{
    public abstract class BaseAdo : IAdo
    {
        protected IDbConnection _DbConnection;
        #region Constructor
        public BaseAdo()
        {
            this.IsEnableLogEvent = false;
            this.CommandType = CommandType.Text;
            this.IsClearParameters = true;
            this.CommandTimeOut = 300;
        }
        #endregion

        #region Properties
        protected List<IDataParameter> OutputParameters { get; set; }
        public virtual string SqlParameterKeyWord { get { return "@"; } }
        public IDbTransaction Transaction { get; set; }
        internal CommandType OldCommandType { get; set; }
        internal bool OldClearParameters { get; set; }
        public IDataParameterCollection DataReaderParameters { get; set; }
        public TimeSpan SqlExecutionTime { get { return AfterTime - BeforeTime; } }
        public bool IsDisableMasterSlaveSeparation { get; set; }
        internal DateTime BeforeTime = DateTime.MinValue;
        internal DateTime AfterTime = DateTime.MinValue;

        public virtual int CommandTimeOut { get; set; }
        public virtual CommandType CommandType { get; set; }
        public virtual bool IsEnableLogEvent { get; set; }
        public virtual bool IsClearParameters { get; set; }
        public virtual List<IDbConnection> SlaveConnections { get; set; }
        public virtual IDbConnection MasterConnection { get; set; }
        #endregion

        #region Connection
        public virtual void Open()
        {
            CheckConnection();
        }
        public virtual void Close()
        {
            if (this.Transaction != null)
            {
                this.Transaction = null;
            }
            if (this.Connection != null && this.Connection.State == ConnectionState.Open)
            {
                this.Connection.Close();
            }
            if (this.IsMasterSlaveSeparation && this.SlaveConnections.HasValue())
            {
                foreach (var slaveConnection in this.SlaveConnections)
                {
                    if (slaveConnection != null && slaveConnection.State == ConnectionState.Open)
                    {
                        slaveConnection.Close();
                    }
                }
            }
        }
        public virtual void Dispose()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Commit();
                this.Transaction = null;
            }
            if (this.Connection != null && this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Close();
            }
            if (this.Connection != null)
            {
                this.Connection.Dispose();
            }
            this.Connection = null;

            if (this.IsMasterSlaveSeparation)
            {
                if (this.SlaveConnections != null)
                {
                    foreach (var slaveConnection in this.SlaveConnections)
                    {
                        if (slaveConnection != null && slaveConnection.State == ConnectionState.Open)
                        {
                            slaveConnection.Dispose();
                        }
                    }
                }
            }
        }
        public virtual void CheckConnection()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                try
                {
                    this.Connection.Open();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #endregion

        #region Transaction
        public virtual void BeginTran()
        {
            CheckConnection();
            if (this.Transaction == null)
                this.Transaction = this.Connection.BeginTransaction();
        }
        public virtual void BeginTran(IsolationLevel iso)
        {
            CheckConnection();
            if (this.Transaction == null)
                this.Transaction = this.Connection.BeginTransaction(iso);
        }
        public virtual void RollbackTran()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Rollback();
                this.Transaction = null;
                this.Close();
            }
        }
        public virtual void CommitTran()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Commit();
                this.Transaction = null;
                this.Close();
            }
        }
        #endregion

        #region abstract
        public abstract IDataParameter[] ToIDbDataParameter(params CommonDBParameter[] pars);
        public abstract void SetCommandToAdapter(IDataAdapter adapter, DbCommand command);
        public abstract IDataAdapter GetAdapter();
        public abstract DbCommand GetCommand(string sql, CommonDBParameter[] pars);
        public abstract IDbConnection Connection { get; set; }
        public abstract void BeginTran(string transactionName);//Only SqlServer
        public abstract void BeginTran(IsolationLevel iso, string transactionName);//Only SqlServer 
        #endregion

        #region Use
        public DbExecResult<bool> UseTran(Action action, Action<Exception> errorCallBack = null)
        {
            var result = new DbExecResult<bool>();
            try
            {
                this.BeginTran();
                if (action != null)
                    action();
                this.CommitTran();
                result.Data = result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.ErrorException = ex;
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
                this.RollbackTran();
                if (errorCallBack != null)
                {
                    errorCallBack(ex);
                }
            }
            return result;
        }

        public Task<DbExecResult<bool>> UseTranAsync(Action action, Action<Exception> errorCallBack = null)
        {
            return Task.FromResult(UseTran(action, errorCallBack));
        }

        public DbExecResult<T> UseTran<T>(Func<T> action, Action<Exception> errorCallBack = null)
        {
            var result = new DbExecResult<T>();
            try
            {
                this.BeginTran();
                if (action != null)
                    result.Data = action();
                this.CommitTran();
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.ErrorException = ex;
                result.ErrorMessage = ex.Message;
                result.IsSuccess = false;
                this.RollbackTran();
                if (errorCallBack != null)
                {
                    errorCallBack(ex);
                }
            }
            return result;
        }

        public Task<DbExecResult<T>> UseTranAsync<T>(Func<T> action, Action<Exception> errorCallBack = null)
        {
            return Task.FromResult(UseTran(action, errorCallBack));
        }

        public virtual IAdo UseStoredProcedure()
        {
            this.OldCommandType = this.CommandType;
            this.OldClearParameters = this.IsClearParameters;
            this.CommandType = CommandType.StoredProcedure;
            this.IsClearParameters = false;
            return this;
        }
        #endregion

        #region Core
        public virtual int ExecuteCommand(string sql, params CommonDBParameter[] parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                SetConnectionStart(sql);
                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                int count = sqlCommand.ExecuteNonQuery();
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                return count;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }

        public virtual DataSet GetDataSetAll(string sql, params CommonDBParameter[] parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                SetConnectionStart(sql);
                ExecuteBefore(sql, parameters);
                IDataAdapter dataAdapter = this.GetAdapter();
                DbCommand sqlCommand = GetCommand(sql, parameters);
                this.SetCommandToAdapter(dataAdapter, sqlCommand);
                DataSet ds = new DataSet();
                dataAdapter.Fill(ds);
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                return ds;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }
        public virtual object GetScalar(string sql, params CommonDBParameter[] parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                SetConnectionStart(sql);
                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                object scalar = sqlCommand.ExecuteScalar();
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                return scalar;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }

        public virtual async Task<int> ExecuteCommandAsync(string sql, params CommonDBParameter[] parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                SetConnectionStart(sql);
                ExecuteBefore(sql, parameters);
                var sqlCommand = GetCommand(sql, parameters);
                int count = await sqlCommand.ExecuteNonQueryAsync();
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                return count;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }
        public virtual async Task<IDataReader> GetDataReaderAsync(string sql, params CommonDBParameter[] parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                SetConnectionStart(sql);
                var isSp = this.CommandType == CommandType.StoredProcedure;
                ExecuteBefore(sql, parameters);
                var sqlCommand = GetCommand(sql, parameters);
                var sqlDataReader = await sqlCommand.ExecuteReaderAsync(this.IsAutoClose() ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                if (isSp)
                    DataReaderParameters = sqlCommand.Parameters;
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                SetConnectionEnd(sql);
                return sqlDataReader;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                throw ex;
            }
        }
        public virtual async Task<object> GetScalarAsync(string sql, params CommonDBParameter[] parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                SetConnectionStart(sql);
                ExecuteBefore(sql, parameters);
                var sqlCommand = GetCommand(sql, parameters);
                var scalar = await sqlCommand.ExecuteScalarAsync();
                //scalar = (scalar == null ? 0 : scalar);
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                return scalar;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }
        public virtual Task<DataSet> GetDataSetAllAsync(string sql, params CommonDBParameter[] parameters)
        {
            //False asynchrony . No Support DataSet
            return Task.FromResult(GetDataSetAll(sql, parameters));
        }
        #endregion

        #region Methods

        public virtual string GetString(string sql, object parameters)
        {
            return GetString(sql, this.GetParameters(parameters));
        }
        public virtual string GetString(string sql, params CommonDBParameter[] parameters)
        {
            return Convert.ToString(GetScalar(sql, parameters));
        }
        public virtual string GetString(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetString(sql);
            }
            else
            {
                return GetString(sql, parameters.ToArray());
            }
        }


        public virtual Task<string> GetStringAsync(string sql, object parameters)
        {
            return GetStringAsync(sql, this.GetParameters(parameters));
        }
        public virtual async Task<string> GetStringAsync(string sql, params CommonDBParameter[] parameters)
        {
            return Convert.ToString(await GetScalarAsync(sql, parameters));
        }
        public virtual Task<string> GetStringAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetStringAsync(sql);
            }
            else
            {
                return GetStringAsync(sql, parameters.ToArray());
            }
        }



        public virtual long GetLong(string sql, object parameters = null)
        {
            return Convert.ToInt64(GetScalar(sql, GetParameters(parameters)));
        }
        public virtual async Task<long> GetLongAsync(string sql, object parameters = null)
        {
            return Convert.ToInt64(await GetScalarAsync(sql, GetParameters(parameters)));
        }


        public virtual int GetInt(string sql, object parameters)
        {
            return GetInt(sql, this.GetParameters(parameters));
        }
        public virtual int GetInt(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetInt(sql);
            }
            else
            {
                return GetInt(sql, parameters.ToArray());
            }
        }
        public virtual int GetInt(string sql, params CommonDBParameter[] parameters)
        {
            return GetScalar(sql, parameters).ObjToInt();
        }

        public virtual Task<int> GetIntAsync(string sql, object parameters)
        {
            return GetIntAsync(sql, this.GetParameters(parameters));
        }
        public virtual Task<int> GetIntAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetIntAsync(sql);
            }
            else
            {
                return GetIntAsync(sql, parameters.ToArray());
            }
        }
        public virtual async Task<int> GetIntAsync(string sql, params CommonDBParameter[] parameters)
        {
            var list = await GetScalarAsync(sql, parameters);
            return list.ObjToInt();
        }

        public virtual Double GetDouble(string sql, object parameters)
        {
            return GetDouble(sql, this.GetParameters(parameters));
        }
        public virtual Double GetDouble(string sql, params CommonDBParameter[] parameters)
        {
            return GetScalar(sql, parameters).ObjToMoney();
        }
        public virtual Double GetDouble(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDouble(sql);
            }
            else
            {
                return GetDouble(sql, parameters.ToArray());
            }
        }

        public virtual Task<Double> GetDoubleAsync(string sql, object parameters)
        {
            return GetDoubleAsync(sql, this.GetParameters(parameters));
        }
        public virtual async Task<Double> GetDoubleAsync(string sql, params CommonDBParameter[] parameters)
        {
            var result = await GetScalarAsync(sql, parameters);
            return result.ObjToMoney();
        }
        public virtual Task<Double> GetDoubleAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDoubleAsync(sql);
            }
            else
            {
                return GetDoubleAsync(sql, parameters.ToArray());
            }
        }


        public virtual decimal GetDecimal(string sql, object parameters)
        {
            return GetDecimal(sql, this.GetParameters(parameters));
        }
        public virtual decimal GetDecimal(string sql, params CommonDBParameter[] parameters)
        {
            return GetScalar(sql, parameters).ObjToDecimal();
        }
        public virtual decimal GetDecimal(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDecimal(sql);
            }
            else
            {
                return GetDecimal(sql, parameters.ToArray());
            }
        }


        public virtual Task<decimal> GetDecimalAsync(string sql, object parameters)
        {
            return GetDecimalAsync(sql, this.GetParameters(parameters));
        }
        public virtual async Task<decimal> GetDecimalAsync(string sql, params CommonDBParameter[] parameters)
        {
            var result = await GetScalarAsync(sql, parameters);
            return result.ObjToDecimal();
        }
        public virtual Task<decimal> GetDecimalAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDecimalAsync(sql);
            }
            else
            {
                return GetDecimalAsync(sql, parameters.ToArray());
            }
        }



        public virtual DateTime GetDateTime(string sql, object parameters)
        {
            return GetDateTime(sql, this.GetParameters(parameters));
        }
        public virtual DateTime GetDateTime(string sql, params CommonDBParameter[] parameters)
        {
            return GetScalar(sql, parameters).ObjToDate();
        }
        public virtual DateTime GetDateTime(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDateTime(sql);
            }
            else
            {
                return GetDateTime(sql, parameters.ToArray());
            }
        }




        public virtual Task<DateTime> GetDateTimeAsync(string sql, object parameters)
        {
            return GetDateTimeAsync(sql, this.GetParameters(parameters));
        }
        public virtual async Task<DateTime> GetDateTimeAsync(string sql, params CommonDBParameter[] parameters)
        {
            var list = await GetScalarAsync(sql, parameters);
            return list.ObjToDate();
        }
        public virtual Task<DateTime> GetDateTimeAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDateTimeAsync(sql);
            }
            else
            {
                return GetDateTimeAsync(sql, parameters.ToArray());
            }
        }


        public virtual DataTable GetDataTable(string sql, params CommonDBParameter[] parameters)
        {
            var ds = GetDataSetAll(sql, parameters);
            if (ds.Tables.Count != 0 && ds.Tables.Count > 0) return ds.Tables[0];
            return new DataTable();
        }
        
        public virtual DataTable GetDataTable(string sql, object parameters)
        {
            return GetDataTable(sql, this.GetParameters(parameters));
        }
        public virtual DataTable GetDataTable(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDataTable(sql);
            }
            else
            {
                return GetDataTable(sql, parameters.ToArray());
            }
        }


        public virtual async Task<DataTable> GetDataTableAsync(string sql, params CommonDBParameter[] parameters)
        {
            var ds = await GetDataSetAllAsync(sql, parameters);
            if (ds.Tables.Count != 0 && ds.Tables.Count > 0) return ds.Tables[0];
            return new DataTable();
        }
        public virtual Task<DataTable> GetDataTableAsync(string sql, object parameters)
        {
            return GetDataTableAsync(sql, this.GetParameters(parameters));
        }
        public virtual Task<DataTable> GetDataTableAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDataTableAsync(sql);
            }
            else
            {
                return GetDataTableAsync(sql, parameters.ToArray());
            }
        }


        public virtual DataSet GetDataSetAll(string sql, object parameters)
        {
            return GetDataSetAll(sql, this.GetParameters(parameters));
        }
        public virtual DataSet GetDataSetAll(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDataSetAll(sql);
            }
            else
            {
                return GetDataSetAll(sql, parameters.ToArray());
            }
        }

        public virtual Task<DataSet> GetDataSetAllAsync(string sql, object parameters)
        {
            return GetDataSetAllAsync(sql, this.GetParameters(parameters));
        }
        public virtual Task<DataSet> GetDataSetAllAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDataSetAllAsync(sql);
            }
            else
            {
                return GetDataSetAllAsync(sql, parameters.ToArray());
            }
        }

        public virtual Task<IDataReader> GetDataReaderAsync(string sql, object parameters)
        {
            return GetDataReaderAsync(sql, this.GetParameters(parameters));
        }
        public virtual Task<IDataReader> GetDataReaderAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetDataReaderAsync(sql);
            }
            else
            {
                return GetDataReaderAsync(sql, parameters.ToArray());
            }
        }
        public virtual object GetScalar(string sql, object parameters)
        {
            return GetScalar(sql, this.GetParameters(parameters));
        }
        public virtual object GetScalar(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetScalar(sql);
            }
            else
            {
                return GetScalar(sql, parameters.ToArray());
            }
        }
        public virtual Task<object> GetScalarAsync(string sql, object parameters)
        {
            return GetScalarAsync(sql, this.GetParameters(parameters));
        }
        public virtual Task<object> GetScalarAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return GetScalarAsync(sql);
            }
            else
            {
                return GetScalarAsync(sql, parameters.ToArray());
            }
        }
        public virtual int ExecuteCommand(string sql, object parameters)
        {
            return ExecuteCommand(sql, GetParameters(parameters));
        }
        public virtual int ExecuteCommand(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return ExecuteCommand(sql);
            }
            else
            {
                return ExecuteCommand(sql, parameters.ToArray());
            }
        }
        public virtual Task<int> ExecuteCommandAsync(string sql, object parameters)
        {
            return ExecuteCommandAsync(sql, GetParameters(parameters));
        }
        public virtual Task<int> ExecuteCommandAsync(string sql, List<CommonDBParameter> parameters)
        {
            if (parameters == null)
            {
                return ExecuteCommandAsync(sql);
            }
            else
            {
                return ExecuteCommandAsync(sql, parameters.ToArray());
            }
        }
        #endregion
        public virtual DataTable QueryBySQLId(string sqlId, List<CommonDBParameter> parameters)
        {
            string sql = SQLStringFactory.GetSQLById(sqlId);
            if (parameters == null)
            {
                return GetDataTable(sql);
            }
            else
            {
                return GetDataTable(sql, parameters.ToArray());
            }
        }
        public virtual List<T> QueryClassBySQLId<T>(string sqlId, List<CommonDBParameter> parameters)
        {
            DataTable dt = null;
            dt = QueryBySQLId(sqlId, parameters);
            return UtilConvert.ConvertDataTableToClass<T>(dt);
        }
        public virtual int ExecuteCommandBySQLId(string sqlId, List<CommonDBParameter> parameters)
        {
            string sql = SQLStringFactory.GetSQLById(sqlId);
            if (parameters == null)
            {
                return ExecuteCommand(sql);
            }
            else
            {
                return ExecuteCommand(sql, parameters.ToArray());
            }
        }
        public virtual int InsertBatch(string tableName, DataTable batchData)
        {
            return 0;
        }
        
        public virtual int GetIntBySQLId(string sqlId, List<CommonDBParameter> parameters)
        {
            string sql = SQLStringFactory.GetSQLById(sqlId);
            return GetInt(sql, parameters);
        }
        public virtual DateTime GetDateTimeBySQLId(string sqlId, List<CommonDBParameter> parameters)
        {
            string sql = SQLStringFactory.GetSQLById(sqlId);
            return GetDateTime(sql, parameters);
        }
        public virtual Double GetDoubleBySQLId(string sqlId, List<CommonDBParameter> parameters)
        {
            string sql = SQLStringFactory.GetSQLById(sqlId);
            return GetDouble(sql, parameters);
        }
        public virtual string GetStringBySQLId(string sqlId, List<CommonDBParameter> parameters)
        {
            string sql = SQLStringFactory.GetSQLById(sqlId);
            return GetString(sql, parameters);
        }
        public virtual decimal GetDecimalBySQLId(string sqlId, List<CommonDBParameter> parameters)
        {
            string sql = SQLStringFactory.GetSQLById(sqlId);
            return GetDecimal(sql, parameters);
        }
        #region  Helper
        private void Async()
        {
            Guid tmpG = Guid.NewGuid(); ;
        }
        private static bool NextResult(IDataReader dataReader)
        {
            try
            {
                return dataReader.NextResult();
            }
            catch
            {
                return false;
            }
        }

        public virtual void ExecuteBefore(string sql, CommonDBParameter[] parameters)
        {
            this.BeforeTime = DateTime.Now;
        }
        public virtual void ExecuteAfter(string sql, CommonDBParameter[] parameters)
        {
            this.AfterTime = DateTime.Now;
            var hasParameter = parameters.HasValue();
            if (hasParameter)
            {
                foreach (var outputParameter in parameters.Where(it => it.Direction.IsIn(ParameterDirection.Output, ParameterDirection.InputOutput, ParameterDirection.ReturnValue)))
                {
                    var gobalOutputParamter = this.OutputParameters.FirstOrDefault(it => it.ParameterName == outputParameter.ParameterName);
                    if (gobalOutputParamter == null)
                    {//Oracle bug
                        gobalOutputParamter = this.OutputParameters.FirstOrDefault(it => it.ParameterName == outputParameter.ParameterName.TrimStart(outputParameter.ParameterName.First()));
                    }
                    outputParameter.Value = gobalOutputParamter.Value;
                    this.OutputParameters.Remove(gobalOutputParamter);
                }
            }
            if (this.OldCommandType != 0)
            {
                this.CommandType = this.OldCommandType;
                this.IsClearParameters = this.OldClearParameters;
                this.OldCommandType = 0;
                this.OldClearParameters = false;
            }
        }
        public virtual CommonDBParameter[] GetParameters(object parameters, PropertyInfo[] propertyInfo = null)
        {
            if (parameters == null) return null;
            return GetParametersInner(parameters, propertyInfo, this.SqlParameterKeyWord);
        }
        protected bool IsAutoClose()
        {
            return this.Transaction == null;
        }
        private bool IsMasterSlaveSeparation
        {
            get
            {
                return false;
            }
        }
        private void SetConnectionStart(string sql)
        {
            //if (this.Transaction == null && this.IsMasterSlaveSeparation && IsRead(sql))
            //{
            //    if (this.MasterConnection == null)
            //    {
            //        this.MasterConnection = this.Connection;
            //    }
            //}
        }
        private void SetConnectionEnd(string sql)
        {
            //if (this.IsMasterSlaveSeparation && IsRead(sql) && this.Transaction == null)
            //{
            //    this.Connection = this.MasterConnection;
            //}
        }

        private bool IsRead(string sql)
        {
            var sqlLower = sql.ToLower();
            var result = Regex.IsMatch(sqlLower, "[ ]*select[ ]") && !Regex.IsMatch(sqlLower, "[ ]*insert[ ]|[ ]*update[ ]|[ ]*delete[ ]");
            return result;
        }

        private void InitParameters(ref string sql, CommonDBParameter[] parameters)
        {
            if (parameters.HasValue())
            {
                foreach (var item in parameters)
                {
                    if (item.Value != null)
                    {
                        var type = item.Value.GetType();
                        if ((type != DBConstant.ByteArrayType && type.IsArray) || type.FullName.IsCollectionsList())
                        {
                            var newValues = new List<string>();
                            foreach (var inValute in item.Value as System.Collections.IEnumerable)
                            {
                                newValues.Add(inValute.ObjToString());
                            }
                            if (newValues.IsNullOrEmpty())
                            {
                                newValues.Add("-1");
                            }
                            if (item.ParameterName.Substring(0, 1) == ":")
                            {
                                sql = sql.Replace("@" + item.ParameterName.Substring(1), newValues.ToArray().ToJoinSqlInVals());
                            }
                            sql = sql.Replace(item.ParameterName, newValues.ToArray().ToJoinSqlInVals());
                            item.Value = DBNull.Value;
                        }
                    }
                }
            }
        }
 
        #endregion

        protected virtual CommonDBParameter[] GetParametersInner(object parameters, PropertyInfo[] propertyInfo, string sqlParameterKeyWord)
        {
            List<CommonDBParameter> result = new List<CommonDBParameter>();
            if (parameters != null)
            {
                var entityType = parameters.GetType();
                var isDictionary = entityType.IsIn(DBConstant.DicArraySO, DBConstant.DicArraySS);
                if (isDictionary)
                    DictionaryToParameters(parameters, sqlParameterKeyWord, result, entityType);
                else if (parameters is List<CommonDBParameter>)
                {
                    result = (parameters as List<CommonDBParameter>);
                }
                else if (parameters is CommonDBParameter[])
                {
                    result = (parameters as CommonDBParameter[]).ToList();
                }
                else
                {
                    ProperyToParameter(parameters, propertyInfo, sqlParameterKeyWord, result, entityType);
                }
            }
            return result.ToArray();
        }
        protected void ProperyToParameter(object parameters, PropertyInfo[] propertyInfo, string sqlParameterKeyWord, List<CommonDBParameter> listParams, Type entityType)
        {
            PropertyInfo[] properties = null;
            if (propertyInfo != null)
                properties = propertyInfo;
            else
                properties = entityType.GetProperties();

            foreach (PropertyInfo properyty in properties)
            {
                var value = properyty.GetValue(parameters, null);
                if (properyty.PropertyType.IsEnum())
                    value = Convert.ToInt64(value);
                if (value == null || value.Equals(DateTime.MinValue)) value = DBNull.Value;
                if (properyty.Name.ToLower().Contains("hierarchyid"))
                {
                    var parameter = new CommonDBParameter(sqlParameterKeyWord + properyty.Name, SqlDbType.Udt);
                    parameter.UdtTypeName = "HIERARCHYID";
                    parameter.Value = value;
                    listParams.Add(parameter);
                }
                else
                {
                    var parameter = new CommonDBParameter(sqlParameterKeyWord + properyty.Name, value);
                    listParams.Add(parameter);
                }
            }
        }
        protected void DictionaryToParameters(object parameters, string sqlParameterKeyWord, List<CommonDBParameter> listParams, Type entityType)
        {
            if (entityType == DBConstant.DicArraySO)
            {
                var dictionaryParameters = (Dictionary<string, object>)parameters;
                var CommomDBParameters = dictionaryParameters.Select(it => new CommonDBParameter(sqlParameterKeyWord + it.Key, it.Value));
                listParams.AddRange(CommomDBParameters);
            }
            else
            {
                var dictionaryParameters = (Dictionary<string, string>)parameters;
                var CommomDBParameters = dictionaryParameters.Select(it => new CommonDBParameter(sqlParameterKeyWord + it.Key, it.Value));
                listParams.AddRange(CommomDBParameters); ;
            }
        }
        public virtual DataTable GetColumnNameAndTypeFromTable(string tableName)
        {
            return new DataTable();
        }
        public virtual int GetSequeceNextValue(string tableName)
        {
            return 0;
        }
        public virtual IDataReader GetDataReader(string sql, params CommonDBParameter[] parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                SetConnectionStart(sql);
                var isSp = this.CommandType == CommandType.StoredProcedure;
                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                IDataReader sqlDataReader = sqlCommand.ExecuteReader(this.IsAutoClose() ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                if (isSp)
                    DataReaderParameters = sqlCommand.Parameters;
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                SetConnectionEnd(sql);
                return sqlDataReader;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                throw ex;
            }
        }
        public virtual IDataReader GetDataReader(string sql, List<CommonDBParameter> parameters)
        {
            return GetDataReader(sql, parameters.ToArray());
        }
        public virtual IDataReader GetDataReaderBySQLId(string sqlId, List<CommonDBParameter> parameters)
        {
            return GetDataReaderBySQLId(sqlId, parameters.ToArray());
        }
        public virtual IDataReader GetDataReaderBySQLId(string sqlId, CommonDBParameter[] parameters)
        {
            string sql = SQLStringFactory.GetSQLById(sqlId);
            return GetDataReader(sql, parameters);
        }
    }
}
