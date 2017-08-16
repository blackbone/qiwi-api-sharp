using System;
using QiwiApiSharp.Entities;

namespace QiwiApiSharp
{
    public class PaymentHistoryResponse
    {
        public Payment[] data;
        public long? nextTxnId;
        public DateTime? nextTxnDate;
    }
}