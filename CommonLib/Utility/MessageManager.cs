using System;
using System.Collections.Generic;
using System.Text;

namespace Meeting.Base.CommonLib.Utility
{
    public class MessageManager
    {
        public static string GetMesaage(string id, params object[] paramArr)
        {
            return string.Format(GetMessageById(id), paramArr);
        }
        private static string GetMessageById(string id)
        {
            return string.Empty;
        }
    }
}
