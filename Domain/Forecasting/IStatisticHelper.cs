using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public interface IStatisticHelper
    {
        /// <summary>
        /// Occurs when [status changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-13
        /// </remarks>
        event EventHandler<StatusChangedEventArgs> StatusChanged;

	    /// <summary>
	    /// Gets the workload days with statistics.
	    /// </summary>
	    /// <param name="period">The period.</param>
	    /// <param name="workload">The workload.</param>
	    /// <param name="existingValidatedVolumeDays">The existing validated volume days.</param>
	    /// <returns></returns>
	    /// <remarks>
	    /// Created by: robink
	    /// Created date: 2008-04-02
	    /// </remarks>
	    IList<ITaskOwner> GetWorkloadDaysWithValidatedStatistics(DateOnlyPeriod period, IWorkload workload, IEnumerable<IValidatedVolumeDay> existingValidatedVolumeDays);

        /// <summary>
        /// Loads and match statistic data.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="longterm">if set to <c>true</c> [longterm].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        IList<ISkillDay> LoadStatisticData(
            DateOnlyPeriod period,
            ISkill skill,
            IScenario scenario,
            bool longterm);

        /// <summary>
        /// Loads and match statistic data.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        IList<IWorkloadDayBase> LoadStatisticData(
            DateOnlyPeriod period,
            IWorkload workload);
    }
}