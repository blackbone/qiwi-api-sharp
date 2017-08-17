using System;

namespace QiwiApiSharp.Entities
{
    public class AuthInfo
    {
        public string boundEmail;
        public string ip;
        public DateTime? lastLoginDate;
        public MobilePinInfo mobilePinInfo;
        public PassInfo passInfo;
        public long? personId;
        public PinInfo pinInfo;
        public DateTime? registrationDate;
    }
}