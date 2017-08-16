using QiwiApiSharp.Entities;

namespace QiwiApiSharp
{
    public class PaymentStatisticsResponse
    {
        public Sum[] incomingTotal;
        public Sum[] outgoingTotal;
    }
}