using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationPreferencesFactory
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;

		public OptimizationPreferencesFactory(IDayOffRulesRepository dayOffRulesRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
		}

		public OptimizationPreferences Create()
		{
			var defaultRules = _dayOffRulesRepository.Default();
			return new OptimizationPreferences
			{
				DaysOff = new DaysOffPreferences
				{
					ConsecutiveDaysOffValue = defaultRules.ConsecutiveDayOffs,
					UseConsecutiveDaysOff = true,
					ConsecutiveWorkdaysValue = defaultRules.ConsecutiveWorkdays,
					UseConsecutiveWorkdays = true,
					ConsiderWeekAfter = true,
					ConsiderWeekBefore = true,
					DaysOffPerWeekValue = defaultRules.DayOffsPerWeek,
					UseDaysOffPerWeek = true
				},
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepDaysOff = true }
			};
		} 
	}
}