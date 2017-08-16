using System;

namespace QiwiApiSharp.Entities
{
    public class PassInfo
    {
        public bool? passwordUsed;
        public DateTime? lastPassChange;
        public DateTime? nextPassChange;
    }
}