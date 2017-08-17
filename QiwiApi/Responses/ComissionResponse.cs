using System.Collections.Generic;
using QiwiApiSharp.Entities;

namespace QiwiApiSharp
{
    public class CommissionResponse
    {
        public CommissionResponseContent content;
    }

    public class CommissionResponseContent
    {
        public CommissionResponseTerms terms;
    }

    public class CommissionResponseTerms
    {
        public CommissionResponseComission commission;
    }

    public class CommissionResponseComission
    {
        public List<CommissionRange> ranges;
    }
}