using System;
using System.Collections.Generic;
using QiwiApiSharp.Enumerations;

namespace QiwiApiSharp.Entities
{
    public class Payment
    {
        public long? txnId;
        public long? personId;
        public DateTime? date;
        public int? errorCode;
        public string error;
        public PaymentStatus? status;
        public string type;
        public string statusText;
        public string trmTxnId;
        public string account;
        public Sum sum;
        public Sum comission;
        public Sum total;
        public PaymentProvider provider;
        public string comment;
        public decimal? currencyRate;
        public Dictionary<string, object> extras;
        public bool? chequeReady;
        public bool? bankDocumentAvailable;
        public bool? bankDocumentReady;
        public bool? repeatPaymentEnabled;
    }
}