using System;

namespace QiwiApiSharp.Exceptions
{
    public class RequestException : Exception
    {
        private string _message;
        public override string Message
        {
            get { return _message; }
        }

        public RequestException(string message)
        {
            _message = message;
        }
    }
}