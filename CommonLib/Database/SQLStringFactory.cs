using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.Database
{
    public class SQLStringFactory
    {
        private static object _lock = new object();
        private static Dictionary<string, string> sqlDic = new Dictionary<string, string>();
        private SQLStringFactory()
        {
        }
        private static string GetSQLFromFileBySQLId(string id)
        {
            StringBuilder sql = new StringBuilder();
            string path = @"Config\sql\{0}.txt";
            try
            {
                using (var txt = new FileStream(string.Format(path, id), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(txt))
                {
                    List<string> file = new List<string>();
                    while (!sr.EndOfStream)
                    {
                        sql.Append(sr.ReadLine());
                        sql.Append(" ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sql.ToString();
        }
        public static string GetSQLById(string id)
        {
            lock(_lock)
            {
                if (!sqlDic.ContainsKey(id))
                {
                    sqlDic.Add(id, GetSQLFromFileBySQLId(id));
                }
            }
            return sqlDic[id];
        }
        public static void SQLFactoryReload()
        {
            lock (_lock)
            {
                if (sqlDic != null)
                {
                    sqlDic.Clear();
                }
            }
        }
    }
}
