using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Meeting.Base.CommonLib.Utility;
using Meeting.Base.CommonLib.ServiceIF;
using Meeting.Base.CommonLib.Database;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Data;

namespace Meeting.Services
{
    public class GetScheduleList : BaseService
    {
        protected override object DoProcess(string param)
        {
            JObject paramObj = JsonConvert.DeserializeObject<dynamic>(param);
            string date = paramObj.GetValue("date").ToObject<string>();

            string sql = Sql.sqlGetScheduleList.Replace("@Date", date);
            IAdo ado = context.GetAdp(false);
            DataTable data = ado.GetDataTable(sql);

            Dictionary<string, List<GetScheduleListDto>> scheduleDic = new Dictionary<string, List<GetScheduleListDto>>();
            if (data.Rows.Count > 0)
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];
                    string room = row["Room"].ToString();
                    if (scheduleDic.GetValueOrDefault(room) == null)
                    {
                        scheduleDic[room] = new List<GetScheduleListDto>();
                    }

                    GetScheduleListDto schedule = new GetScheduleListDto {
                        ID = UtilConvert.ObjToInt(row["ID"]),
                        User = row["User"].ToString(),
                        Start = row["Start"].ToString(),
                        End = row["End"].ToString(),
                    };
                    scheduleDic[room].Add(schedule);
                }
            }
            return scheduleDic;
        }
    }

    public class GetInitial : BaseService
    {
        protected override object DoProcess(string param)
        {
            string username = context.UserId is null ? "zh.yuanyuan" : context.UserId;
            return new Dictionary<string, object> { { "user", username } };
        }
    }

    public class SetSchedule : BaseService
    {
        protected override object DoProcess(string param)
        {
            JObject paramObj = JsonConvert.DeserializeObject<dynamic>(param);
            //JObject paramObj = JObject.Parse(data.ToString());
            string room = paramObj.GetValue("room").ToObject<string>();
            string user = paramObj.GetValue("user").ToObject<string>();
            string start = paramObj.GetValue("start").ToObject<string>();
            string end = paramObj.GetValue("end").ToObject<string>();
            string date = paramObj.GetValue("date").ToObject<string>();

            string sql = Sql.sqlGetScheduleList.Replace("@Date", date).Replace("@Room", room);
            IAdo ado = context.GetAdp(false);
            DataTable data = ado.GetDataTable(sql);

            bool canSchedule = true;
            if (data.Rows.Count > 0)
            {
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    DataRow row = data.Rows[i];
                    string startTime = row["Start"].ToString();
                    string endTime = row["Start"].ToString();
                    if (string.Compare(start, startTime) < 0) {
                        if (string.Compare(end, startTime) > 0)
                        {
                            canSchedule = false;
                        }
                        break;
                    } else if (string.Compare(start, endTime) < 0) {
                        canSchedule = false;
                        break;
                    } else {
                        continue;
                    }
                }
            }

            bool code = false;
            if (canSchedule) {
                string sql_set = Sql.sqlSetSchedule.
                    Replace("@User", user).Replace("@Room", room).
                    Replace("@Start", date + ' ' + start).Replace("@End", date + ' ' + end);
                int set_result = ado.ExecuteCommand(sql_set);
                if (set_result == 1) {
                    code = true;
                }
            }
            return code;
        }
    }
}
