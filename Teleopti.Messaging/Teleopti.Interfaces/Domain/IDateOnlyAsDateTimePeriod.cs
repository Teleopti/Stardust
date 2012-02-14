namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds a date only and a iccctimezoneinfor
    /// to get a local date time
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2010-05-18
    /// </remarks>
    public interface IDateOnlyAsDateTimePeriod
    {
        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-05-18
        /// </remarks>
        DateOnly DateOnly { get; }

        /// <summary>
        /// Gets the period for the dateonly.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2010-05-18
        /// </remarks>
        DateTimePeriod Period();
    }
}