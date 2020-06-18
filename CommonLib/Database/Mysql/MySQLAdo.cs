using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
//using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using Meeting.Base.CommonLib.Utility;

namespace Meeting.Base.CommonLib.Database.MySQL
{
    public class MySQLAdo : BaseAdo
    {
        protected int _linkId;

        public MySQLAdo(int linkId = 0) 
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
                        base._DbConnection = new MySqlConnection(ConfigManager.GetDBConnectionString(_linkId));
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
        
        public override void BeginTran(string transactionName)
        {
            CheckConnection();
            base.Transaction = ((MySqlConnection)this.Connection).BeginTransaction();
        }

        public override void BeginTran(IsolationLevel iso, string transactionName)
        {
            CheckConnection();
            base.Transaction = ((MySqlConnection)this.Connection).BeginTransaction(iso);
        }
        public override IDataAdapter GetAdapter()
        {
            return new MySqlDataAdapter();
        }
        public override DbCommand GetCommand(string sql, CommonDBParameter[] parameters)
        {
            MySqlCommand mysqlCommand = new MySqlCommand(sql, (MySqlConnection)this.Connection);
            mysqlCommand.CommandType = this.CommandType;
            mysqlCommand.CommandTimeout = this.CommandTimeOut;
            if (this.Transaction != null)
            {
                mysqlCommand.Transaction = (MySqlTransaction)this.Transaction;
            }
            if (parameters.HasValue())
            {
                MySqlParameter[] ipars = GetMySqlParameter(parameters);
                mysqlCommand.Parameters.AddRange(ipars);
            }
            CheckConnection();
            return mysqlCommand;
        }
        public override void SetCommandToAdapter(IDataAdapter dataAdapter, DbCommand command)
        {
            ((MySqlDataAdapter)dataAdapter).SelectCommand = (MySqlCommand)command;
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
            MySqlParameter[] result = new MySqlParameter[parameters.Length];
            int index = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                var mysqlParameter = new MySqlParameter();
                mysqlParameter.ParameterName = parameter.ParameterName;
                mysqlParameter.Size = parameter.Size;
                mysqlParameter.Value = parameter.Value;
                mysqlParameter.DbType = parameter.DbType;
                mysqlParameter.Direction = parameter.Direction;
                result[index] = mysqlParameter;
                if (mysqlParameter.Direction.IsIn(ParameterDirection.Output, ParameterDirection.InputOutput,ParameterDirection.ReturnValue))
                {
                    if (this.OutputParameters == null) this.OutputParameters = new List<IDataParameter>();
                    this.OutputParameters.RemoveAll(it => it.ParameterName == mysqlParameter.ParameterName);
                    this.OutputParameters.Add(mysqlParameter);
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
        public MySqlParameter[] GetMySqlParameter(params CommonDBParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            MySqlParameter[] result = new MySqlParameter[parameters.Length];
            int index = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                var mysqlParameter = new MySqlParameter();
                mysqlParameter.ParameterName = parameter.ParameterName;
                mysqlParameter.Size = parameter.Size;
                mysqlParameter.Value = parameter.Value;
                mysqlParameter.DbType = parameter.DbType;
                if (mysqlParameter.Value!=null&& mysqlParameter.Value != DBNull.Value && mysqlParameter.DbType == System.Data.DbType.DateTime)
                {
                    var date = Convert.ToDateTime(mysqlParameter.Value);
                    if (date==DateTime.MinValue)
                    {
                        mysqlParameter.Value = Convert.ToDateTime("1753/01/01");
                    }
                }
                mysqlParameter.Direction = parameter.Direction;
                result[index] = mysqlParameter;
                if (parameter.TypeName.HasValue()) {
                    //mysqlParameter.TypeName = parameter.TypeName;
                    mysqlParameter.MySqlDbType = MySqlDbType.String;
                    mysqlParameter.DbType = DbType.Object;
                }
                if (mysqlParameter.Direction.IsIn(ParameterDirection.Output, ParameterDirection.InputOutput, ParameterDirection.ReturnValue))
                {
                    if (this.OutputParameters == null) this.OutputParameters = new List<IDataParameter>();
                    this.OutputParameters.RemoveAll(it => it.ParameterName == mysqlParameter.ParameterName);
                    this.OutputParameters.Add(mysqlParameter);
                }
                ++index;
            }
            return result;
        }
        public override int InsertBatch(string tableName, DataTable batchData)
        {
            int result = 0; // insert count
            MySqlBulkLoader bulkCopy = null;
            try
            {
                CheckConnection();
                bulkCopy = new MySqlBulkLoader((MySqlConnection)this.Connection) {
                    FieldTerminator = ",",
                    FieldQuotationCharacter = '"',
                    EscapeCharacter = '"',
                    LineTerminator = "\r\n",
                    FileName = tableName + ".csv",
                    NumberOfLinesToSkip = 0,
                    TableName = tableName,
                };
                bulkCopy.Timeout = 1200;
                result = bulkCopy.Load();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                //bulkCopy.Close();
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
