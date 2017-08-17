using System;
using System.Collections.Generic;

namespace QiwiApiSharp.Entities
{
    public class ContractInfo
    {
        public bool? blocked;
        public long? contactId;
        public DateTime? creationDate;
        public List<object> features;
        public List<IdentificationInfo> IdentificationInfo;
    }
}