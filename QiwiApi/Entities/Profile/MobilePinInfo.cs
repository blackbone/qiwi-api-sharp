using System;

namespace QiwiApiSharp.Entities
{
    public class MobilePinInfo
    {
        public DateTime? lastMobilePinChange;
        public DateTime? nextMobilePinChange;
        public bool? mobilePinUsed;
    }
}