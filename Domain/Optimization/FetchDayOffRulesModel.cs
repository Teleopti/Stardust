using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchDayOffRulesModel : IFetchDayOffRulesModel
	{
		private readonly IDayOffSettingsRepository _dayOffSettingsRepository;

		public FetchDayOffRulesModel(IDayOffSettingsRepository dayOffSettingsRepository)
		{
			_dayOffSettingsRepository = dayOffSettingsRepository;
		}

		public DayOffSettingModel FetchDefaultRules()
		{
			var setting = _dayOffSettingsRepository.Default();
			return new DayOffSettingModel
				{
					MinConsecutiveWorkdays = setting.ConsecutiveWorkdays.Minimum,
					MaxConsecutiveWorkdays = setting.ConsecutiveWorkdays.Maximum,
					MinDayOffsPerWeek = setting.DayOffsPerWeek.Minimum,
					MaxDayOffsPerWeek = setting.DayOffsPerWeek.Maximum,
					MinConsecutiveDayOffs = setting.ConsecutiveDayOffs.Minimum,
					MaxConsecutiveDayOffs = setting.ConsecutiveDayOffs.Maximum,
					Id = setting.Id ?? Guid.Empty,
					Default = setting.Default
				};
		}
	}
}