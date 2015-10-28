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
			var workrules = _dayOffRulesRepository.Default();
			return new OptimizationPreferences
			{
				DaysOff = new DaysOffPreferences
				{
					ConsecutiveDaysOffValue = workrules.ConsecutiveDayOffs,
					UseConsecutiveDaysOff = true,
					ConsecutiveWorkdaysValue = workrules.ConsecutiveWorkdays,
					UseConsecutiveWorkdays = true,
					ConsiderWeekAfter = true,
					ConsiderWeekBefore = true,
					DaysOffPerWeekValue = workrules.DayOffsPerWeek,
					UseDaysOffPerWeek = true
				},
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepDaysOff = true }
			};
		} 
	}
}