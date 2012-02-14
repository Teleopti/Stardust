using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    /// <summary>
    /// Caclculates statistical info for SkillStaffPeriod list
    /// </summary>
    public class SkillStaffPeriodStatisticsForSkillIntraday
    {
        private readonly IEnumerable<ISkillStaffPeriod> _periods;
        private IDeviationStatisticsCalculator _deviationStatisticsCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillStaffPeriodStatisticsForSkillIntraday"/> class.
        /// </summary>
        /// <param name="periods">The periods.</param>
        public SkillStaffPeriodStatisticsForSkillIntraday(IEnumerable<ISkillStaffPeriod> periods)
        {
            _periods = periods;
            _deviationStatisticsCalculator = new DeviationStatisticsCalculator();
            InitializeStatisticCalculator();
        }

        /// <summary>
        /// Gets the deviation statistics calculator.
        /// </summary>
        /// <value>The deviation statistics calculator.</value>
        public IDeviationStatisticsCalculator StatisticsCalculator
        {
            get { return _deviationStatisticsCalculator; }
            set { _deviationStatisticsCalculator = value; }
        }

        /// <summary>
        /// Analyzes the skill staff period and fills in the statistical properties.
        /// </summary>
        public void Analyze()
        {
            StatisticsCalculator.AnalyzeData();
        }

        /// <summary>
        /// Initializes the statistic calculator.
        /// </summary>
        protected void InitializeStatisticCalculator()
        {
            foreach (ISkillStaffPeriod skillStaffPeriod in _periods)
            {
                double expectedValue = CreateExpectedValue(skillStaffPeriod );
                if (expectedValue > 0)
                {
                    double realValue = CreateRealValue(skillStaffPeriod);
                    StatisticsCalculator.AddItem(expectedValue, realValue);
                }
            }
        }

        /// <summary>
        /// Creates the value for calculator.
        /// </summary>
        /// <param name="skillStaffPeriod">The skill staff period.</param>
        /// <returns></returns>
        protected static double CreateExpectedValue(ISkillStaffPeriod skillStaffPeriod)
        {
            return skillStaffPeriod.FStaff;
        }

        /// <summary>
        /// Creates the real value.
        /// </summary>
        /// <param name="skillStaffPeriod">The skill staff period.</param>
        /// <returns></returns>
        protected static double CreateRealValue(ISkillStaffPeriod skillStaffPeriod)
        {
            return skillStaffPeriod.CalculatedResource;
        }

    }
}