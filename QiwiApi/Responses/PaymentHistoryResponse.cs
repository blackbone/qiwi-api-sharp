using System;
using System.Collections.Generic;
using QiwiApiSharp.Entities;

namespace QiwiApiSharp
{
    public class PaymentHistoryResponse
    {
        public List<Payment> data;
        public long? nextTxnId;
        public DateTime? nextTxnDate;
    }
}