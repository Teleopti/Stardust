namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface to implement which contains properties that the day is open for Incoming work or not.
    /// </summary>
    /// <remarks>
    /// Created by: Talham
    /// Created date: 2012-02-28
    /// </remarks>
    public interface IOpenForWork
    {
        /// <summary>
        /// Gets or sets that the day is open, Here we consider only open working hours.
        /// </summary>
        /// <value> ture = open working hours or day is open. false= close working hours or day is closed.</value>
        /// <remarks>
        /// Created by: talham
        /// Created date: 2012-02-28
        /// </remarks>
        bool IsOpen { get; set; }
        /// <summary>
        /// Gets or sets that the day is open for incoming work such as Email, Fax anything other than telephony.
        /// </summary>
        /// <value> ture = open for incoming work. false= close for incoming work.</value>
        /// <remarks>
        /// Created by: talham
        /// Created date: 2012-02-28
        /// </remarks>
        bool IsOpenForIncomingWork { get; set; }
    }
}