using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.ComponentModel;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.Utility
{
    public class ConfigManager
    {
        private static XElement config = null;
        private static object _lock = new object();
        private static Dictionary<string, string> allServices = new Dictionary<string, string>();
        private ConfigManager()
        {
        }
        private static XElement GetConfigurationInstance()
        {
            lock (_lock)
            {
                if (config == null)
                {
                    string configFileName = "./Config/SericesConfig.xml";
                    try
                    {
                        config = XElement.Load(configFileName);
                    }
                    catch (Exception ex)
                    {
                        config = null;
                        throw ex;
                    }
                }
            }
            return config;
        }
        public static void ConfigReload()
        {
            lock (_lock)
            {
                if (config != null)
                {
                    config = null;
                    GetConfigurationInstance();
                }
            }
        }
        public static string GetParamer(string key)
        {
            string val = string.Empty;
            try
            {
                XElement ele = GetConfigurationInstance().XPathSelectElement(key);
                if (ele != null)
                {
                    val = ele.Value.ToString();
                }
                return val;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public static T GetParamer<T>(string key, T defaultVal)
        {
            T retVal = defaultVal;
            try
            {
                XElement ele = GetConfigurationInstance().XPathSelectElement(key);
                if (ele != null)
                {
                    string val = ele.Value.ToString();
                    TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                    retVal = (T)converter.ConvertFrom(val);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retVal;
        }
        
        public static string GetDBConnectionString(int linkId = 0)
        {
            try
            {
                string dbConStr = "DBConnectionString";
                if (linkId > 0)
                {
                    dbConStr = string.Format("DBConnectionString_{0}", linkId);
                }
                XElement ele = GetConfigurationInstance().XPathSelectElement(dbConStr);
                if (ele != null)
                {
                    return ele.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return string.Empty;
        }
        public static string GetService(string name)
        {
            string type = string.Empty;
            if (allServices.Count == 0)
            {
                GetAllService();
            }
            if (allServices.ContainsKey(name))
            {
                type = allServices[name];
            }
            return type;
        }
        public static Dictionary<string, string> GetAllService()
        {
            try
            {
                lock (_lock)
                {
                    if (allServices.Count == 0)
                    {
                        XElement servicesRoot = GetConfigurationInstance().XPathSelectElement("Services");
                        if (servicesRoot != null)
                        {
                            IEnumerable<XElement> servicesElement = servicesRoot.XPathSelectElements("Service");
                            if (servicesElement != null)
                            {
                                foreach (XElement serviceElement in servicesElement)
                                {
                                    string name = serviceElement.XPathSelectElement("ServiceName").Value;
                                    string type = serviceElement.XPathSelectElement("ServiceType").Value;
                                    allServices.Add(name, type);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return allServices;
        }
    }
}
