using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationPreferencesFactory
	{
		private readonly IDayOffSettingsRepository _dayOffSettingsRepository;

		public OptimizationPreferencesFactory(IDayOffSettingsRepository dayOffSettingsRepository)
		{
			_dayOffSettingsRepository = dayOffSettingsRepository;
		}

		public OptimizationPreferences Create()
		{
			var workrules = _dayOffSettingsRepository.LoadAll().Single();
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