using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.Database
{
    public interface IAdo
    {
        string SqlParameterKeyWord { get; }
        IDbConnection Connection { get; set; }
        IDbTransaction Transaction { get; set; }
        IDataParameter[] ToIDbDataParameter(params CommonDBParameter[] pars);
        CommonDBParameter[] GetParameters(object obj, PropertyInfo[] propertyInfo = null);
        void ExecuteBefore(string sql, CommonDBParameter[] pars);
        void ExecuteAfter(string sql, CommonDBParameter[] pars);
        bool IsEnableLogEvent { get; set; }

        IDataParameterCollection DataReaderParameters { get; set; }
        CommandType CommandType { get; set; }

        bool IsDisableMasterSlaveSeparation { get; set; }
        bool IsClearParameters { get; set; }
        int CommandTimeOut { get; set; }
        TimeSpan SqlExecutionTime { get; }
        void SetCommandToAdapter(IDataAdapter adapter, DbCommand command);
        IDataAdapter GetAdapter();
        DbCommand GetCommand(string sql, CommonDBParameter[] parameters);


        DataTable GetDataTable(string sql, object parameters);
        DataTable GetDataTable(string sql, params CommonDBParameter[] parameters);
        DataTable GetDataTable(string sql, List<CommonDBParameter> parameters);

        Task<DataTable> GetDataTableAsync(string sql, object parameters);
        Task<DataTable> GetDataTableAsync(string sql, params CommonDBParameter[] parameters);
        Task<DataTable> GetDataTableAsync(string sql, List<CommonDBParameter> parameters);

        DataSet GetDataSetAll(string sql, object parameters);
        DataSet GetDataSetAll(string sql, params CommonDBParameter[] parameters);
        DataSet GetDataSetAll(string sql, List<CommonDBParameter> parameters);

        Task<DataSet> GetDataSetAllAsync(string sql, object parameters);
        Task<DataSet> GetDataSetAllAsync(string sql, params CommonDBParameter[] parameters);
        Task<DataSet> GetDataSetAllAsync(string sql, List<CommonDBParameter> parameters);

        int ExecuteCommand(string sql, object parameters);
        int ExecuteCommand(string sql, params CommonDBParameter[] parameters);
        int ExecuteCommand(string sql, List<CommonDBParameter> parameters);

        Task<int> ExecuteCommandAsync(string sql, params CommonDBParameter[] parameters);
        Task<int> ExecuteCommandAsync(string sql, object parameters);
        Task<int> ExecuteCommandAsync(string sql, List<CommonDBParameter> parameters);

        string GetString(string sql, object parameters);
        string GetString(string sql, params CommonDBParameter[] parameters);
        string GetString(string sql, List<CommonDBParameter> parameters);
        Task<string> GetStringAsync(string sql, object parameters);
        Task<string> GetStringAsync(string sql, params CommonDBParameter[] parameters);
        Task<string> GetStringAsync(string sql, List<CommonDBParameter> parameters);


        int GetInt(string sql, object pars);
        int GetInt(string sql, params CommonDBParameter[] parameters);
        int GetInt(string sql, List<CommonDBParameter> parameters);

        Task<int> GetIntAsync(string sql, object pars);
        Task<int> GetIntAsync(string sql, params CommonDBParameter[] parameters);
        Task<int> GetIntAsync(string sql, List<CommonDBParameter> parameters);


        long GetLong(string sql, object pars = null);

        Task<long> GetLongAsync(string sql, object pars = null);


        Double GetDouble(string sql, object parameters);
        Double GetDouble(string sql, params CommonDBParameter[] parameters);
        Double GetDouble(string sql, List<CommonDBParameter> parameters);


        Task<Double> GetDoubleAsync(string sql, object parameters);
        Task<Double> GetDoubleAsync(string sql, params CommonDBParameter[] parameters);
        Task<Double> GetDoubleAsync(string sql, List<CommonDBParameter> parameters);


        decimal GetDecimal(string sql, object parameters);
        decimal GetDecimal(string sql, params CommonDBParameter[] parameters);
        decimal GetDecimal(string sql, List<CommonDBParameter> parameters);

        Task<decimal> GetDecimalAsync(string sql, object parameters);
        Task<decimal> GetDecimalAsync(string sql, params CommonDBParameter[] parameters);
        Task<decimal> GetDecimalAsync(string sql, List<CommonDBParameter> parameters);


        DateTime GetDateTime(string sql, object parameters);
        DateTime GetDateTime(string sql, params CommonDBParameter[] parameters);
        DateTime GetDateTime(string sql, List<CommonDBParameter> parameters);

        Task<DateTime> GetDateTimeAsync(string sql, object parameters);
        Task<DateTime> GetDateTimeAsync(string sql, params CommonDBParameter[] parameters);
        Task<DateTime> GetDateTimeAsync(string sql, List<CommonDBParameter> parameters);

        DataTable QueryBySQLId(string sqlId, List<CommonDBParameter> parameters);
        List<T> QueryClassBySQLId<T>(string sqlId, List<CommonDBParameter> parameters);
        int ExecuteCommandBySQLId(string sqlId, List<CommonDBParameter> parameters);
        int InsertBatch(string tableName, DataTable batchData);
        int GetIntBySQLId(string sqlId, List<CommonDBParameter> parameters);
        DateTime GetDateTimeBySQLId(string sqlId, List<CommonDBParameter> parameters);
        Double GetDoubleBySQLId(string sqlId, List<CommonDBParameter> parameters);
        string GetStringBySQLId(string sqlId, List<CommonDBParameter> parameters);
        decimal GetDecimalBySQLId(string sqlId, List<CommonDBParameter> parameters);
        DataTable GetColumnNameAndTypeFromTable(string tableName);
        int GetSequeceNextValue(string tableName);

        void Dispose();
        void Close();
        void Open();
        void CheckConnection();

        void BeginTran();
        void BeginTran(IsolationLevel iso);
        void BeginTran(string transactionName);
        void BeginTran(IsolationLevel iso, string transactionName);
        void RollbackTran();
        void CommitTran();

        DbExecResult<bool> UseTran(Action action, Action<Exception> errorCallBack = null);
        DbExecResult<T> UseTran<T>(Func<T> action, Action<Exception> errorCallBack = null);
        Task<DbExecResult<bool>> UseTranAsync(Action action, Action<Exception> errorCallBack = null);
        Task<DbExecResult<T>> UseTranAsync<T>(Func<T> action, Action<Exception> errorCallBack = null);
        IAdo UseStoredProcedure();
        IDataReader GetDataReader(string sql, List<CommonDBParameter> parameters);
        IDataReader GetDataReader(string sql, CommonDBParameter[] parameters);
        IDataReader GetDataReaderBySQLId(string sql, List<CommonDBParameter> parameters);
        IDataReader GetDataReaderBySQLId(string sql, CommonDBParameter[] parameters);
    }
}
