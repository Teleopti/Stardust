using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A person day off
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-06
    /// </remarks>
    public interface IPersonDayOff : IPersistableScheduleData, 
                                        IChangeInfo,
                                        IBelongsToBusinessUnit,
                                        ICloneableEntity<IPersonDayOff>
    {
        /// <summary>
        /// Information about the Day Off
        /// </summary>
        DayOff DayOff { get; }

        /// <summary>
        /// Compares a PersonDayOff to a DayOffTemplate.
        /// </summary>
        /// <param name="dayOffTemplate">The day off template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-02-06
        /// </remarks>
        bool CompareToTemplate(IDayOffTemplate dayOffTemplate);

        /// <summary>
        /// Compares to template for locking.
        /// </summary>
        /// <param name="dayOffTemplate">The day off template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-22
        /// </remarks>
        bool CompareToTemplateForLocking(IDayOffTemplate dayOffTemplate);

        /// <summary>
        /// Checksums this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-08-31
        /// </remarks>
        long Checksum();

        /// <summary>
        /// Gets the used time zone.
        /// </summary>
        /// <value>The used time zone.</value>
        TimeZoneInfo UsedTimeZone
        { get; }
    }
}