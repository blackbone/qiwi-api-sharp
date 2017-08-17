using System.Collections.Generic;
using QiwiApiSharp.Entities;

namespace QiwiApiSharp
{
    public class PaymentStatisticsResponse
    {
        public List<CurrencyAmount> incomingTotal;
        public List<CurrencyAmount> outgoingTotal;
    }
}