namespace QiwiApiSharp.Enumerations
{
    /// <summary>
    /// Transaction provider identifiers.
    /// </summary>
    public enum Provider
    {
        /// <summary>
        /// Qiwi wallet.
        /// </summary>
        QiwiWallet = 99,
        /// <summary>
        /// Visa card emitted in Russian Federation.
        /// </summary>
        VisaRus = 1963,
        /// <summary>
        /// MasterCard emitted in Russian Federation.
        /// </summary>
        MasterCardRus = 21013,
        /// <summary>
        /// Visa card emitted in Azerbaijan, Armenia, Belarus, Georgia, Kazakhstan, Kyrgyzstan, Moldova, Tajikistan, Turkmenistan, Ukraine, Uzbekistan;
        /// </summary>
        VisaCIS = 1960,
        /// <summary>
        /// MasterCard emitted in Azerbaijan, Armenia, Belarus, Georgia, Kazakhstan, Kyrgyzstan, Moldova, Tajikistan, Turkmenistan, Ukraine, Uzbekistan;
        /// </summary>
        MasterCardCIS = 21012,
        /// <summary>
        /// MIR payment system emitted card.
        /// </summary>
        MIR = 31652,
        /// <summary>
        /// Tinkoff bank card.
        /// </summary>
        Tinkoff = 466,
        /// <summary>
        /// AlfaBank card.
        /// </summary>
        AlfaBank = 464,
        /// <summary>
        /// PromsvyazBank card.
        /// </summary>
        PromsvyazBank = 821,
        /// <summary>
        /// Russian standard card.
        /// </summary>
        RussianStandard = 815,
        /// <summary>
        /// Mobile phone provider. Is generic by phone number and not used by api.
        /// </summary>
        MobilePhone = -1
    }
}