using System;

namespace QiwiApiSharp.Exceptions
{
    public class NotInitializedException : Exception
    {
        public override string Message
        {
            get { return "QiwiApi has not been initalized. Call 'QiwiApi.Initialize(*token*) first."; }
        }
    }
}