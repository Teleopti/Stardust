using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Helper for deciding what to read from repository and what to read from loaded data
    /// </summary>
    public interface IPersonAccountProjectionService
    {
        /// <summary>
        /// Periods  to load from Repository
        /// </summary>
        /// <returns></returns>
        IList<DateTimePeriod> PeriodsToLoad();


        /// <summary>
        /// Periods to read from the loaded schedule.
        /// </summary>
        /// <returns></returns>
        DateTimePeriod? PeriodToReadFromSchedule();

        /// <summary>
        /// Alls the visual layers from loaded data and repository.
        /// </summary>
        /// <param name="storage">The repository.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        IList<IScheduleDay> CreateProjection(IScheduleStorage storage, IScenario scenario);
    }
}
