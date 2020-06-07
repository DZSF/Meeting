using System;
using System.Collections.Generic;
using System.Text;
using Intel.NsgAuto.WaferCost.Base.CommonLib.BaseException;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.ServiceIF
{
    public abstract class BaseService : IService
    {
        protected ServiceContext context = new ServiceContext();
        public void SetContext(ServiceContext ctx)
        {
            context = ctx;
        }
        public string Process(string param)
        {
            string result = string.Empty;
            JObject errObj = new JObject();
            try
            {
                DoCheck(param);
                result = DoProcess(param);
            }
            catch (AppException appEx)
            {
                errObj.Add("ErrType", "AppException");
                errObj.Add("ErrMessage", appEx.Message);
            }
            catch (SysException sysEx)
            {
                errObj.Add("ErrType", "SysException");
                errObj.Add("ErrMessage", sysEx.Message);
            }
            catch (Exception ex)
            {
                errObj.Add("ErrType", "UnknownException");
                errObj.Add("ErrMessage", ex.StackTrace);
            }
            if (errObj.HasValues)
            {
                result = JsonConvert.SerializeObject(errObj);
            }
            return result;
        }
        protected virtual void DoCheck(string param)
        {

        }
        protected abstract string DoProcess(string param);
    }
}
