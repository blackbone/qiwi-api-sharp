using QiwiApiSharp.Entities;

namespace QiwiApiSharp
{
    public class PaymentStatisticsResponse
    {
        public CurrencyAmount[] incomingTotal;
        public CurrencyAmount[] outgoingTotal;
    }
}