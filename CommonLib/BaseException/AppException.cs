using System;
using System.Collections.Generic;
using System.Text;

namespace Meeting.Base.CommonLib.BaseException
{
    public class AppException : Exception
    {
        public AppException()
            :base()
        {

        }
        public AppException(string message)
            : base(message)
        {
        }

        public AppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
