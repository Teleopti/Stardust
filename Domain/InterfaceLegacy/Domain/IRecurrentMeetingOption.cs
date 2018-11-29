using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Defines the functionality of a meeting recurrent option
    /// </summary>
    public interface IRecurrentMeetingOption : IAggregateEntity, ICloneableEntity<IRecurrentMeetingOption>
    {
        /// <summary>
        /// Gets the meeting days.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        IList<DateOnly> GetMeetingDays(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Gets or sets the increment count (i.e. every x days/weeks/months).
        /// </summary>
        /// <value>The increment count.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-13
        /// </remarks>
        int IncrementCount { get; set; }

        /////// <summary>
        /////// Adds the extra clone data.
        /////// </summary>
        /////// <param name="obj">The obj.</param>
        ////void AddExtraCloneData(IRecurrentMeetingOption obj);
    }
}
