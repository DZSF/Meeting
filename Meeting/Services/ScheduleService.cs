using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Intel.NsgAuto.WaferCost.Base.CommonLib.Utility;
using Intel.NsgAuto.WaferCost.Base.CommonLib.ServiceIF;
using Intel.NsgAuto.WaferCost.Base.CommonLib.Database;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Intel.NsgAuto.WaferCost.ACID.Services
{
    public class GetScheduleList : BaseService
    {
        public GetScheduleList()
        {
        }

        protected override void DoCheck(string param)
        {
            
        }

        protected override string DoProcess(string param)
        {
            string user = context.UserId;

            Dictionary<string, string[]> scheduleDic = new Dictionary<string, string[]>();
            string[] arrayA = { "1/zhangyuanyuan@1-2,16-18", "2/Jack@3-5" };
            scheduleDic.Add("510", arrayA);
            string[] arrayB = { "1/zhangyuanyuan@5-10", "3/Seal@2-3,20-23" };
            scheduleDic.Add("520", arrayB);
            return JsonConvert.SerializeObject(scheduleDic);
        }
    }

    public class GetUser : BaseService
    {
        protected override string DoProcess(string param)
        {
            //this.ConnectDB();
            return JsonConvert.SerializeObject(context.UserId is null ? "TestUser" : context.UserId);
        }

        private void ConnectDB() {
            IAdo ado = context.GetAdp();
            ado.Connection.Open();
            DbCommand command = ado.GetCommand("select * from Room", null);
            DbDataReader rdr = command.ExecuteReader();
            rdr.Close();
            command.Dispose();
            ado.Connection.Close();
        }
    }

    public class SetSchedule : BaseService
    {
        public SetSchedule()
        {
        }

        protected override void DoCheck(string param)
        {

        }

        protected override string DoProcess(string param)
        {
            JObject dataObj = JsonConvert.DeserializeObject<dynamic>(param);
            //JObject dataObj = JObject.Parse(data.ToString());
            string room = dataObj.GetValue("room").ToObject<string>();
            string user = dataObj.GetValue("user").ToObject<string>();
            int start = dataObj.GetValue("start").ToObject<int>();
            int end = dataObj.GetValue("end").ToObject<int>();
            //DateTime date = dataObj.GetValue("user").ToObject<DateTime>();
            // get from db according to room & date
            Dictionary<string, int> dic1 = new Dictionary<string, int>
            {
                {"start", 0 }, {"end", 1 }
            };
            Dictionary<string, int> dic2 = new Dictionary<string, int>
            {
                {"start", 10 }, {"end", 13 }
            };
            Dictionary<string, int>[] listOfTimeInterval = { dic1, dic2 };
            List<int> listOfExistTime = new List<int>();
            foreach (Dictionary<string, int> time in listOfTimeInterval)
            {
                for (int i = time["start"]; i < time["end"]; i++)
                {
                    listOfExistTime.Add(i);
                }
            }
            // [0, 10, 11, 12]
            List<int> listOfBookTime = new List<int>();
            for (int i = start; i < end; i++)
            {
                listOfBookTime.Add(i);
            }
            bool code = false;
            if (listOfBookTime.Except(listOfExistTime).ToList().Count == listOfBookTime.Count)
            {
                code = true;
                // insert to db
            }
            return JsonConvert.SerializeObject(code);
        }
    }
}
