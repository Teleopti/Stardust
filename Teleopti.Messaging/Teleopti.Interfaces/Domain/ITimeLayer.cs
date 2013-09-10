namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Interface for TimePeriod based layer
    ///</summary>
    ///<typeparam name="T"></typeparam>
    public interface ITimeLayer<T>
    {
        /// <summary>
        /// Gets the pay load.
        /// </summary>
        /// <value>The pay load.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        T Payload { get; }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-22
        /// </remarks>
        TimePeriod Period { get; }
    }

}
