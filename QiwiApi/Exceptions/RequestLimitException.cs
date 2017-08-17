using System;

namespace QiwiApiSharp.Exceptions
{
    public class RequestLimitException : Exception
    {
        public override string Message
        {
            get { return "Request limit exception (423). Service is temporary unavailable."; }
        }
    }
}