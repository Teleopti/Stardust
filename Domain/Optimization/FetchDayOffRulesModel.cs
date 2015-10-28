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
			var defaultRules = _dayOffRulesRepository.Default();
			return new DayOffRulesModel
				{
					MinConsecutiveWorkdays = defaultRules.ConsecutiveWorkdays.Minimum,
					MaxConsecutiveWorkdays = defaultRules.ConsecutiveWorkdays.Maximum,
					MinDayOffsPerWeek = defaultRules.DayOffsPerWeek.Minimum,
					MaxDayOffsPerWeek = defaultRules.DayOffsPerWeek.Maximum,
					MinConsecutiveDayOffs = defaultRules.ConsecutiveDayOffs.Minimum,
					MaxConsecutiveDayOffs = defaultRules.ConsecutiveDayOffs.Maximum,
					Id = defaultRules.Id ?? Guid.Empty,
					Default = defaultRules.Default
				};
		}
	}
}