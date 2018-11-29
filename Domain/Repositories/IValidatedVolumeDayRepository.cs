using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    /// <summary>
    /// Interface for validated volume day repository
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-04-02
    /// </remarks>
    public interface IValidatedVolumeDayRepository : IRepository<IValidatedVolumeDay>
    {
        /// <summary>
        /// Finds the last validated day.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        DateOnly FindLastValidatedDay(IWorkload workload);

        /// <summary>
        /// Finds the range.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        ICollection<IValidatedVolumeDay> FindRange(DateOnlyPeriod period, IWorkload workload);

        /// <summary>
        /// Matches the days.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <param name="taskOwnerList">The task owner list.</param>
        /// <param name="existingValidatedVolumeDays">The existing validated volume days.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        IList<ITaskOwner> MatchDays(IWorkload workload, IEnumerable<ITaskOwner> taskOwnerList,
                                    IEnumerable<IValidatedVolumeDay> existingValidatedVolumeDays);

        IValidatedVolumeDay FindLatestUpdated(ISkill skill);
    }
}
