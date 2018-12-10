namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Contains details for doing a swap
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-09-01
    /// </remarks>
    public interface ISwapDetail
    {
        /// <summary>
        /// Gets the person from.
        /// </summary>
        /// <value>The person from.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        IPerson PersonFrom { get;}

        /// <summary>
        /// Gets the date from.
        /// </summary>
        /// <value>The date from.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        DateOnly DateFrom { get; }

        /// <summary>
        /// Gets the person to.
        /// </summary>
        /// <value>The person to.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        IPerson PersonTo { get; }

        /// <summary>
        /// Gets the date to.
        /// </summary>
        /// <value>The date to.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        DateOnly DateTo { get; }
    }
}
