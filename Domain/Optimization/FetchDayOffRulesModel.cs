using System;
using System.Collections.Generic;
using System.Linq;

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

		public IEnumerable<DayOffRulesModel> FetchAll()
		{
			var all = _dayOffRulesRepository.LoadAll();

			var result = all.Select(dayOffRules => _dayOffRulesMapper.ToModel(dayOffRules)).ToList();

			if(!all.Any(x => x.Default))
				result.Add(_dayOffRulesMapper.ToModel(DayOffRules.CreateDefault()));

			return result;
		}

		public DayOffRulesModel Fetch(Guid id)
		{
			var dayOffRules = _dayOffRulesRepository.Get(id);
			if (dayOffRules == null)
				throw new ArgumentException(string.Format("Cannot find DayOffRules with Id {0}", id));

			return _dayOffRulesMapper.ToModel(dayOffRules);
		}
	}
}