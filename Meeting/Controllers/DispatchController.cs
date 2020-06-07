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
using Intel.NsgAuto.WaferCost.ACID.Services;

namespace Intel.NsgAuto.WaferCost.ACID.Controllers
{
    [Route("api/meeting")]
    [ApiController]
    public class DispatchController : ControllerBase
    {
        // GET api/<DispatchController>/5/d
        [HttpGet("{baseUri}/{serviceName}")]
        public string Get()
        {
            string baseUri = RouteData.Values["baseUri"].ToString();
            string serviceName = RouteData.Values["serviceName"].ToString();
            string param = JsonConvert.SerializeObject(Request.Query.Keys);
            return ServiceProcess(baseUri, serviceName, param);
        }

        // POST api/<DispatchController>
        [HttpPost("{baseUri}/{serviceName}")]
        public string Post([FromBody] dynamic param)
        {
            string baseUri = RouteData.Values["baseUri"].ToString();
            string serviceName = RouteData.Values["serviceName"].ToString();
            return ServiceProcess(baseUri, serviceName, param.ToString());
        }

        // PUT api/<DispatchController>/5
        [HttpPut("{baseUri}/{serviceName}")]
        public string Put(string baseUri, string serviceName, string param)
        {
            return ServiceProcess(baseUri, serviceName, param);
        }

        // DELETE api/<DispatchController>/5
        [HttpDelete("{baseUri}/{serviceName}")]
        public void Delete(string baseUri, string serviceName, string param)
        {
            ServiceProcess(baseUri, serviceName, param);
        }
        protected string ServiceProcess(string baseUri, string serviceName, string param)
        {
            string result = string.Empty;
            string serviceTypeStr = ConfigManager.GetService(serviceName);
            try
            {
                Assembly assembly = Assembly.GetCallingAssembly();
                Type serviceType = assembly.GetType(serviceTypeStr);
                BaseService service = Activator.CreateInstance(serviceType) as BaseService;
                ServiceContext context = new ServiceContext();
                context.UserId = User.Identity.Name;
                //context.Request = Request;
                service.SetContext(context);
                result = service.Process(param);
            }
            catch(Exception ex)
            {
                JObject errObj = new JObject();
                errObj.Add("ErrType", "UnknownException");
                errObj.Add("ErrMessage", ex.StackTrace);
                result = JsonConvert.SerializeObject(errObj);
            }
            return result;
        }
    }
}
