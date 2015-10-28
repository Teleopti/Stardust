using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchDayOffRulesModel : IFetchDayOffRulesModel
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;

		public FetchDayOffRulesModel(IDayOffRulesRepository dayOffRulesRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
		}

		public DayOffRulesModel FetchDefaultRules()
		{
			var setting = _dayOffRulesRepository.Default();
			return new DayOffRulesModel
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