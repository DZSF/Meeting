using System;
using System.Collections.Generic;
using System.Text;

namespace Intel.NsgAuto.WaferCost.Base.CommonLib.BaseException
{
    public class SysException : Exception
    {
        public SysException()
            : base()
        {

        }
        public SysException(string message)
            : base(message)
        {
        }

        public SysException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
