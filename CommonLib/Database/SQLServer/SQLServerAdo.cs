using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Meeting.Base.CommonLib.Utility;

namespace Meeting.Base.CommonLib.Database.SQLServer
{
    public class SQLServerAdo : BaseAdo
    {
        protected int _linkId;

        public SQLServerAdo(int linkId = 0) 
        {
            _linkId = linkId;
        }
        public override IDbConnection Connection
        {
            get
            {
                if (base._DbConnection == null)
                {
                    try
                    {
                        base._DbConnection = new SqlConnection(ConfigManager.GetDBConnectionString(_linkId));
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                return base._DbConnection;
            }
            set
            {
                base._DbConnection = value;
            }
        }
        
        /// <summary>
        /// Only SqlServer
        /// </summary>
        /// <param name="transactionName"></param>
        public override void BeginTran(string transactionName)
        {
            CheckConnection();
            base.Transaction = ((SqlConnection)this.Connection).BeginTransaction(transactionName);
        }
        /// <summary>
        /// Only SqlServer
        /// </summary>
        /// <param name="iso"></param>
        /// <param name="transactionName"></param>
        public override void BeginTran(IsolationLevel iso, string transactionName)
        {
            CheckConnection();
            base.Transaction = ((SqlConnection)this.Connection).BeginTransaction(iso, transactionName);
        }
        public override IDataAdapter GetAdapter()
        {
            return new SqlDataAdapter();
        }
        public override DbCommand GetCommand(string sql, CommonDBParameter[] parameters)
        {
            SqlCommand sqlCommand = new SqlCommand(sql, (SqlConnection)this.Connection);
            sqlCommand.CommandType = this.CommandType;
            sqlCommand.CommandTimeout = this.CommandTimeOut;
            if (this.Transaction != null)
            {
                sqlCommand.Transaction = (SqlTransaction)this.Transaction;
            }
            if (parameters.HasValue())
            {
                SqlParameter[] ipars = GetSqlParameter(parameters);
                sqlCommand.Parameters.AddRange(ipars);
            }
            CheckConnection();
            return sqlCommand;
        }
        public override void SetCommandToAdapter(IDataAdapter dataAdapter, DbCommand command)
        {
            ((SqlDataAdapter)dataAdapter).SelectCommand = (SqlCommand)command;
        }
        /// <summary>
        /// if mysql return MySqlParameter[] pars
        /// if sqlerver return SqlParameter[] pars ...
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IDataParameter[] ToIDbDataParameter(params CommonDBParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            SqlParameter[] result = new SqlParameter[parameters.Length];
            int index = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                var sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = parameter.ParameterName;
                sqlParameter.UdtTypeName = parameter.UdtTypeName;
                sqlParameter.Size = parameter.Size;
                sqlParameter.Value = parameter.Value;
                sqlParameter.DbType = parameter.DbType;
                sqlParameter.Direction = parameter.Direction;
                result[index] = sqlParameter;
                if (sqlParameter.Direction.IsIn(ParameterDirection.Output, ParameterDirection.InputOutput,ParameterDirection.ReturnValue))
                {
                    if (this.OutputParameters == null) this.OutputParameters = new List<IDataParameter>();
                    this.OutputParameters.RemoveAll(it => it.ParameterName == sqlParameter.ParameterName);
                    this.OutputParameters.Add(sqlParameter);
                }
                ++index;
            }
            return result;
        }
        /// <summary>
        /// if mysql return MySqlParameter[] pars
        /// if sqlerver return SqlParameter[] pars ...
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SqlParameter[] GetSqlParameter(params CommonDBParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            SqlParameter[] result = new SqlParameter[parameters.Length];
            int index = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                var sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = parameter.ParameterName;
                sqlParameter.UdtTypeName = parameter.UdtTypeName;
                sqlParameter.Size = parameter.Size;
                sqlParameter.Value = parameter.Value;
                sqlParameter.DbType = parameter.DbType;
                if (sqlParameter.Value!=null&& sqlParameter.Value != DBNull.Value && sqlParameter.DbType == System.Data.DbType.DateTime)
                {
                    var date = Convert.ToDateTime(sqlParameter.Value);
                    if (date==DateTime.MinValue)
                    {
                        sqlParameter.Value = Convert.ToDateTime("1753/01/01");
                    }
                }
                sqlParameter.Direction = parameter.Direction;
                result[index] = sqlParameter;
                if (parameter.TypeName.HasValue()) {
                    sqlParameter.TypeName = parameter.TypeName;
                    sqlParameter.SqlDbType = SqlDbType.Structured;
                    sqlParameter.DbType = System.Data.DbType.Object;
                }
                if (sqlParameter.Direction.IsIn(ParameterDirection.Output, ParameterDirection.InputOutput, ParameterDirection.ReturnValue))
                {
                    if (this.OutputParameters == null) this.OutputParameters = new List<IDataParameter>();
                    this.OutputParameters.RemoveAll(it => it.ParameterName == sqlParameter.ParameterName);
                    this.OutputParameters.Add(sqlParameter);
                }
                ++index;
            }
            return result;
        }
        public override int InsertBatch(string tableName, DataTable batchData)
        {
            int result = 0;
            SqlBulkCopy bulkCopy = null;
            try
            {
                CheckConnection();
                if (this.Transaction == null)
                {
                    bulkCopy = new SqlBulkCopy((SqlConnection)this.Connection);
                }
                else
                {
                    bulkCopy = new SqlBulkCopy((SqlConnection)this.Connection, SqlBulkCopyOptions.KeepIdentity, //SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.CheckConstraints,
                                   (SqlTransaction)this.Transaction);
                }
                bulkCopy.BulkCopyTimeout = 1200;
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BatchSize = batchData.Rows.Count;
                if (batchData != null && batchData.Rows.Count != 0)
                    bulkCopy.WriteToServer(batchData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                bulkCopy.Close();
            }
            return result;
        }
        public override DataTable GetColumnNameAndTypeFromTable(string tableName)
        {
            string sqlId = string.Format(DBConstant.GET_TABLE_COLUMNS,tableName);
            return GetDataTable(sqlId);
        }
        public override int GetSequeceNextValue(string tableName)
        {
            string sqlId = string.Format(DBConstant.GET_NEXT_SEQUECE_VALUE,tableName);
            return GetInt(sqlId);
        }
    }
}
