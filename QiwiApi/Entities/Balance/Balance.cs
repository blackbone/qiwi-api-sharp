namespace QiwiApiSharp.Entities
{
    public class Balance
    {
        public string alias;
        public string fsAlias;
        public string title;
        public bool? hasBalance;
        public BalanceType type;
        public CurrencyAmount balance;
    }
}