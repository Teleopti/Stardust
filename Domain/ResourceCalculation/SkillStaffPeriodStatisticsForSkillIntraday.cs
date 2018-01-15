using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Caclculates statistical info for SkillStaffPeriod list
    /// </summary>
    public class SkillStaffPeriodStatisticsForSkillIntraday
    {
	    /// <summary>
        /// Initializes a new instance of the <see cref="SkillStaffPeriodStatisticsForSkillIntraday"/> class.
        /// </summary>
        /// <param name="periods">The periods.</param>
        public SkillStaffPeriodStatisticsForSkillIntraday(IEnumerable<ISkillStaffPeriod> periods)
		{
			StatisticsCalculator = new DeviationStatisticsCalculator(periods.Where(p => p.FStaff > 0)
				.Select(p => new DeviationStatisticData(p.FStaff, p.CalculatedResource)));
		}

	    /// <summary>
	    /// Gets the deviation statistics calculator.
	    /// </summary>
	    /// <value>The deviation statistics calculator.</value>
	    public IDeviationStatisticsCalculator StatisticsCalculator { get; }
    }
}