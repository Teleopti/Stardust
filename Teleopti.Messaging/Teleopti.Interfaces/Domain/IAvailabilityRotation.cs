using System.Collections.Generic;


namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAvailabilityRotation : IAggregateRoot, IChangeInfo
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets the availability days.
        /// </summary>
        /// <value>The availability days.</value>
        IList<IAvailabilityDay> AvailabilityDays { get; }
        /// <summary>
        /// Gets the days count.
        /// </summary>
        /// <value>The days count.</value>
        int DaysCount { get; }
        /// <summary>
        /// Finds the availability day.
        /// </summary>
        /// <param name="dayCount">The day count.</param>
        /// <returns></returns>
        IAvailabilityDay FindAvailabilityDay(int dayCount);
        /// <summary>
        /// Adds the days.
        /// </summary>
        /// <param name="dayCount">The day count.</param>
        void AddDays(int dayCount);
        /// <summary>
        /// Removes the days.
        /// </summary>
        /// <param name="dayCount">The day count.</param>
        void RemoveDays(int dayCount);
        /// <summary>
        /// Gets a value indicating whether this instance is choosable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Choosable")]
        bool IsChoosable { get; }
    }
}
