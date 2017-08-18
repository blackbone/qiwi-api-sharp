namespace QiwiApiSharp.Enumerations
{
    /// <summary>
    /// Payment transaction status.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Transaction were accepted.
        /// </summary>
        Accepted,
        /// <summary>
        /// Transaction is waiting for processing.
        /// </summary>
        WAITING,
        /// <summary>
        /// Transaction processing success.
        /// </summary>
        SUCCESS,
        /// <summary>
        /// Transaction processing error.
        /// </summary>
        ERROR
    }
}