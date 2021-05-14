namespace DMQ.MessageComponents.CourierActivities
{
    public interface IPaymentLog
    {
        /// <summary>
        ///     Authorized code after payment is processed.
        /// </summary>
        string AuthorizedCode { get; set; }
    }
}
