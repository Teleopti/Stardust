using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationPreferencesFactory
	{
		private readonly IWorkRuleSettingsRepository _workRuleSettingsRepository;

		public OptimizationPreferencesFactory(IWorkRuleSettingsRepository workRuleSettingsRepository)
		{
			_workRuleSettingsRepository = workRuleSettingsRepository;
		}

		public OptimizationPreferences Create()
		{
			var workrules = _workRuleSettingsRepository.LoadAll().Single();
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