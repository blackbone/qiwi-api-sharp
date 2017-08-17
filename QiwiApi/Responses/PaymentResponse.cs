using System.Collections.Generic;
using QiwiApiSharp.Entities;

namespace QiwiApiSharp
{
    public class PaymentResponse
    {
        public string id;
        public CurrencyAmount sum;
        public Dictionary<string, string> fields;
        public string source;
        public Transaction transaction;
    }
}