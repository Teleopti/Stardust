namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchDayOffSettingsModel : IFetchDayOffSettingsModel
	{
		private readonly IDayOffSettingsRepository _dayOffSettingsRepository;

		public FetchDayOffSettingsModel(IDayOffSettingsRepository dayOffSettingsRepository)
		{
			_dayOffSettingsRepository = dayOffSettingsRepository;
		}

		public DayOffSettingsModel FetchAll()
		{
			var ret = new DayOffSettingsModel();
			foreach (var setting in _dayOffSettingsRepository.LoadAll())
			{
				ret.DayOffSettingModel.Add(new DayOffSettingModel
				{
					MinConsecutiveWorkdays = setting.ConsecutiveWorkdays.Minimum,
					MaxConsecutiveWorkdays = setting.ConsecutiveWorkdays.Maximum,
					MinDayOffsPerWeek = setting.DayOffsPerWeek.Minimum,
					MaxDayOffsPerWeek = setting.DayOffsPerWeek.Maximum,
					MinConsecutiveDayOffs = setting.ConsecutiveDayOffs.Minimum,
					MaxConsecutiveDayOffs = setting.ConsecutiveDayOffs.Maximum,
					Id = setting.Id.Value,
					Default = setting.Default
				});
			}
			return ret;
		}
	}
}