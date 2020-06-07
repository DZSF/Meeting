using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using Intel.NsgAuto.WaferCost.Base.CommonLib.Utility;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.Database.Oracle
{
    public class OracleAdo : BaseAdo
    {
        public OracleAdo()
        {
        }
        public override IDbConnection Connection
        {
            get
            {
                try
                {
                    if (base._DbConnection == null)
                    {
                        base._DbConnection = new OracleConnection(ConfigManager.GetDBConnectionString());
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
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
            ((OracleConnection)this.Connection).BeginTransaction();
        }

        public override void BeginTran(IsolationLevel iso, string transactionName)
        {
            ((OracleConnection)this.Connection).BeginTransaction(iso);
        }

        public override IDataAdapter GetAdapter()
        {
            return new OracleDataAdapter();
        }

        public override DbCommand GetCommand(string sql, CommonDBParameter[] pars)
        {
            OracleCommand sqlCommand = new OracleCommand(sql, (OracleConnection)this.Connection);
            sqlCommand.BindByName = true;
            sqlCommand.CommandType = this.CommandType;
            sqlCommand.CommandTimeout = this.CommandTimeOut;
            if (this.Transaction != null)
            {
                sqlCommand.Transaction = (OracleTransaction)this.Transaction;
            }
            if (pars.HasValue())
            {
                IDataParameter[] ipars = ToIDbDataParameter(pars);
                sqlCommand.Parameters.AddRange((OracleParameter[])ipars);
            }
            CheckConnection();
            return sqlCommand;
        }

        public override void SetCommandToAdapter(IDataAdapter adapter, DbCommand command)
        {
            ((OracleDataAdapter)adapter).SelectCommand = (OracleCommand)command;
        }

        public override IDataParameter[] ToIDbDataParameter(params CommonDBParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            OracleParameter[] result = new OracleParameter[parameters.Length];
            int index = 0;
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                var sqlParameter = new OracleParameter();
                sqlParameter.Size = parameter.Size == -1 ? 0 : parameter.Size;
                sqlParameter.ParameterName = parameter.ParameterName;
                if (sqlParameter.ParameterName[0] == '@')
                {
                    sqlParameter.ParameterName = ':' + sqlParameter.ParameterName.Substring(1, sqlParameter.ParameterName.Length - 1);
                }
                if (this.CommandType == CommandType.StoredProcedure)
                {
                    sqlParameter.ParameterName = sqlParameter.ParameterName.TrimStart(':');
                }
                if (parameter.IsRefCursor)
                {
                    sqlParameter.OracleDbType = OracleDbType.RefCursor;
                }
                if (sqlParameter.DbType == System.Data.DbType.Guid)
                {
                    sqlParameter.DbType = System.Data.DbType.String;
                    sqlParameter.Value = sqlParameter.Value.ObjToString();
                }
                else if (parameter.DbType == System.Data.DbType.Boolean)
                {
                    sqlParameter.DbType = System.Data.DbType.Int16;
                    if (parameter.Value == DBNull.Value)
                    {
                        parameter.Value = 0;
                    }
                    else
                    {
                        sqlParameter.Value = (bool)parameter.Value ? 1 : 0;
                    }
                }
                else
                {
                    if (parameter.Value != null && parameter.Value.GetType() == DBConstant.GuidType)
                    {
                        parameter.Value = parameter.Value.ToString();
                    }
                    sqlParameter.Value = parameter.Value;
                }
                if (parameter.Direction != 0)
                    sqlParameter.Direction = parameter.Direction;
                result[index] = sqlParameter;
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
    }
}
