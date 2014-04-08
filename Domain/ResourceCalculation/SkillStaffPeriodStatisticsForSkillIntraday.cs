using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Caclculates statistical info for SkillStaffPeriod list
    /// </summary>
    public class SkillStaffPeriodStatisticsForSkillIntraday
    {
        private readonly IEnumerable<ISkillStaffPeriod> _periods;

	    /// <summary>
        /// Initializes a new instance of the <see cref="SkillStaffPeriodStatisticsForSkillIntraday"/> class.
        /// </summary>
        /// <param name="periods">The periods.</param>
        public SkillStaffPeriodStatisticsForSkillIntraday(IEnumerable<ISkillStaffPeriod> periods)
        {
            _periods = periods;
            StatisticsCalculator = new DeviationStatisticsCalculator();
            InitializeStatisticCalculator();
        }

	    /// <summary>
	    /// Gets the deviation statistics calculator.
	    /// </summary>
	    /// <value>The deviation statistics calculator.</value>
	    public IDeviationStatisticsCalculator StatisticsCalculator { get; set; }

        /// <summary>
        /// Initializes the statistic calculator.
        /// </summary>
        protected void InitializeStatisticCalculator()
        {
	        var items = _periods.Select(p => new Tuple<double, double>(p.FStaff, p.CalculatedResource));
            foreach (var item in items)
            {
                if (item.Item1 <= 0) continue;
                StatisticsCalculator.AddItem(item.Item1, item.Item2);
            }
        }
    }
}