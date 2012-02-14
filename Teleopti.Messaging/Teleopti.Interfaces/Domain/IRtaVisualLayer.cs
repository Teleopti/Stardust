namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for visual RTA layers
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-12-16
    /// </remarks>
    public interface IRtaVisualLayer : IVisualLayer
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is logged out.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is logged out; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-12-16
        /// </remarks>
        bool IsLoggedOut { get; set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-06-26
        /// </remarks>
        IRtaState State { get; }
    }
}