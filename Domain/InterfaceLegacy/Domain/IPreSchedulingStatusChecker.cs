using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface IPreSchedulingStatusChecker
    {
		bool CheckStatus(IScheduleDay schedulePart, SchedulingOptions schedulingOptions);
        /// <summary>
        /// Gets the person period.
        /// </summary>
        /// <value>The person period.</value>
        IPersonPeriod PersonPeriod { get; }
        /// <summary>
        /// Gets the schedule period.
        /// </summary>
        /// <value>The schedule period.</value>
        IVirtualSchedulePeriod SchedulePeriod { get; }
        /// <summary>
        /// Gets the schedule day in UTC.
        /// </summary>
        /// <value>The schedule day UTC.</value>
        DateTime ScheduleDayUtc { get; }
        /// <summary>
        /// Gets the schedule date only.
        /// </summary>
        /// <value>The schedule date only.</value>
        DateOnly ScheduleDateOnly { get; }
        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        IPerson Person { get; }
    }
}
