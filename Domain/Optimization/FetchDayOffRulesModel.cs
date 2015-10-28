namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchDayOffRulesModel : IFetchDayOffRulesModel
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly DayOffRulesMapper _dayOffRulesMapper;

		public FetchDayOffRulesModel(IDayOffRulesRepository dayOffRulesRepository, DayOffRulesMapper dayOffRulesMapper)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
			_dayOffRulesMapper = dayOffRulesMapper;
		}

		public DayOffRulesModel FetchDefaultRules()
		{
			var defaultRules = _dayOffRulesRepository.Default();
			return _dayOffRulesMapper.ToModel(defaultRules);
		}
	}
}