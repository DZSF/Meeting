using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.Database
{
    public class DBConstant
    {
        private DBConstant()
        {
        }
        internal static Type IntType = typeof(int);
        internal static Type LongType = typeof(long);
        internal static Type GuidType = typeof(Guid);
        internal static Type BoolType = typeof(bool);
        internal static Type BoolTypeNull = typeof(bool?);
        internal static Type ByteType = typeof(Byte);
        internal static Type ObjType = typeof(object);
        internal static Type DobType = typeof(double);
        internal static Type FloatType = typeof(float);
        internal static Type ShortType = typeof(short);
        internal static Type DecType = typeof(decimal);
        internal static Type StringType = typeof(string);
        internal static Type DateType = typeof(DateTime);
        internal static Type DateTimeOffsetType = typeof(DateTimeOffset);
        internal static Type TimeSpanType = typeof(TimeSpan);
        internal static Type ByteArrayType = typeof(byte[]);
        internal static Type Dicii = typeof(KeyValuePair<int, int>);
        internal static Type DicIS = typeof(KeyValuePair<int, string>);
        internal static Type DicSi = typeof(KeyValuePair<string, int>);
        internal static Type DicSS = typeof(KeyValuePair<string, string>);
        internal static Type DicOO = typeof(KeyValuePair<object, object>);
        internal static Type DicSo = typeof(KeyValuePair<string, object>);
        internal static Type DicArraySS = typeof(Dictionary<string, string>);
        internal static Type DicArraySO = typeof(Dictionary<string, object>);
        //constant
        // get columns from table
        public static string GET_TABLE_COLUMNS = @"select col.name as colName,st.name as colType from sys.columns col inner join systypes st on col.system_type_id = st.xtype where object_id=object_id('{0}') order by col.column_id";
        // get next sequence value
        public static string GET_NEXT_SEQUECE_VALUE = "SELECT NEXT VALUE FOR {0}_Seq";
        // get max datetime from the table
        public static string GET_MAX_DATATIME_TABLE = @"SELECT TOP 1 RUN_TIME FROM {0} WHERE TOOL_ID = '{1}' ORDER BY RUN_TIME DESC";
        // get max id message for ER
        public static string GET_HEAD_MESSAGE_ER = @"SELECT COUNT(HEAD_MSG) FROM T_SCANNER_ER_EVENT_LOG WHERE HEAD_MSG='{0}'";
        // check duplicate data
        public static string CHECK_DUPLICATE_DATA_SQL = @"SELECT TOOL_ID FROM {0} WHERE TOOL_ID = '{1}' AND RUN_TIME = '{2}'";
        // check duplicate data by reticle ID
        public static string CHECK_DUPLICATE_DATA_RETICLE_SQL = @"SELECT TOOL_ID FROM {0} WHERE TOOL_ID = '{1}' AND RUN_TIME = '{2}' AND RETICLE_ID ='{3}'";
    }
}
