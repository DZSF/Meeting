using System;
using System.Collections.Generic;
using System.Text;
//using Microsoft.AspNetCore.Http;
using Meeting.Base.CommonLib.Database;
using Meeting.Base.CommonLib.Database.SQLServer;
using Meeting.Base.CommonLib.Database.MySQL;

namespace Meeting.Base.CommonLib.ServiceIF
{
    public class ServiceContext
    {
        public string UserId { get; set; }
        //public HttpRequest Request { get; set; }
        public ServiceContext()
        {
            UserId = string.Empty;
        }
        public IAdo GetAdp(bool procedure = true)
        {
            IAdo ado = new MySQLAdo();
            return procedure ? ado.UseStoredProcedure() : ado;
        }
    }
}
