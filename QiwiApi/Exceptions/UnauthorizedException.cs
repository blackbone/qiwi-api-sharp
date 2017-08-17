using System;

namespace QiwiApiSharp.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public override string Message
        {
            get { return "Unauthorized access exception (401). Token is invalid or has been expired."; }
        }
    }
}