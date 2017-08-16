using System;

namespace QiwiApiSharp.Enumerations
{
    [Flags]
    public enum Operation
    {
        IN = 1,
        OUT = 2,
        QIWI_CARD = 4,
        ALL = IN | OUT | QIWI_CARD
    }
}