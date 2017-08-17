using System;

namespace QiwiApiSharp.Exceptions
{
    public class WalletNotFoundException : Exception
    {
        public override string Message
        {
            get { return "Wallet not found exception (404). Wallet was not found."; }
        }
    }
}