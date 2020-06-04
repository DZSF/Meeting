using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Meeting.Controllers
{
    [Route("api/[controller]")]
    public class ScheduleController : Controller
    {
        [HttpGet("user")]
        public string GetUser()
        {
            return "zhangyuanyuan";
        }

        [HttpGet("schedulelist")]
        public ActionResult GetScheduleList(DateTime date)
        {
            // get from db by date
            Dictionary<string, string[]> scheduleDic = new Dictionary<string, string[]>();
            string[] arrayA = { "1/zhangyuanyuan@1-2,16-18", "2/Jack@3-5" };
            scheduleDic.Add("510", arrayA);
            string[] arrayB = { "1/zhangyuanyuan@5-10", "3/Seal@2-3,20-23" };
            scheduleDic.Add("520", arrayB);
            return new OkObjectResult( scheduleDic );
        }

        [HttpPost("book")]
        public ActionResult BookConference([FromBody] string room, string user, int start, int end, DateTime date)
        {
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
            foreach (Dictionary<string, int> time in listOfTimeInterval) {
                for (int i = time["start"]; i < time["end"]; i++) {
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
            if (listOfBookTime.Except(listOfExistTime).ToList().Count == listOfBookTime.Count) {
                code = true;
                // insert to db
            }
            return new OkObjectResult(code);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
