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
using Meeting.Services;

namespace Meeting.Controllers
{
    [Route("api/meeting")]
    [ApiController]
    public class DispatchController : ControllerBase
    {
        // GET api/<DispatchController>/5/d
        [HttpGet("{baseUri}/{serviceName}")]
        public ActionResult Get()
        {
            string baseUri = RouteData.Values["baseUri"].ToString();
            string serviceName = RouteData.Values["serviceName"].ToString();
            return ServiceProcess(baseUri, serviceName, Request.Query["paramJson"]);
        }

        // POST api/<DispatchController>
        [HttpPost("{baseUri}/{serviceName}")]
        public ActionResult Post([FromBody] dynamic param)
        {
            string baseUri = RouteData.Values["baseUri"].ToString();
            string serviceName = RouteData.Values["serviceName"].ToString();
            return ServiceProcess(baseUri, serviceName, param.ToString());
        }

        protected ActionResult ServiceProcess(string baseUri, string serviceName, string param)
        {
            object result;
            string serviceTypeStr = ConfigManager.GetService(serviceName);
            try
            {
                Assembly assembly = Assembly.GetCallingAssembly();
                Type serviceType = assembly.GetType(serviceTypeStr);
                BaseService service = Activator.CreateInstance(serviceType) as BaseService;
                ServiceContext context = new ServiceContext();
                context.UserId = User.Identity.Name;
                service.SetContext(context);
                result = service.Process(param);
            }
            catch(Exception ex)
            {
                JObject errObj = new JObject();
                errObj.Add("ErrType", "UnknownException");
                errObj.Add("ErrMessage", ex.StackTrace);
                return new BadRequestObjectResult(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
            return new OkObjectResult(result);
        }
    }
}
