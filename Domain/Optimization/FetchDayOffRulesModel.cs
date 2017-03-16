using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FetchDayOffRulesModel : IFetchDayOffRulesModel
	{
		private readonly IDayOffRulesRepository _dayOffRulesRepository;
		private readonly DayOffRulesMapper _dayOffRulesMapper;
		private readonly IAgentGroupRepository _agentGroupRepository;

		public FetchDayOffRulesModel(IDayOffRulesRepository dayOffRulesRepository, DayOffRulesMapper dayOffRulesMapper, IAgentGroupRepository agentGroupRepository)
		{
			_dayOffRulesRepository = dayOffRulesRepository;
			_dayOffRulesMapper = dayOffRulesMapper;
			_agentGroupRepository = agentGroupRepository;
		}

		public IEnumerable<DayOffRulesModel> FetchAllWithoutAgentGroup()
		{
			var all = _dayOffRulesRepository.LoadAllWithoutAgentGroup();

			if (!all.Any(x => x.Default))
				all.Add(DayOffRules.CreateDefault());

			var result = all.Select(dayOffRules => _dayOffRulesMapper.ToModel(dayOffRules)).ToList();
			return result;
		}

		public DayOffRulesModel Fetch(Guid id)
		{
			var dayOffRules = _dayOffRulesRepository.Get(id);
			if (dayOffRules == null)
				throw new ArgumentException($"Cannot find DayOffRules with Id {id}");

			return _dayOffRulesMapper.ToModel(dayOffRules);
		}

		public IEnumerable<DayOffRulesModel> FetchAllForAgentGroup(Guid agentGroupId)
		{
			var agentGroup = _agentGroupRepository.Get(agentGroupId);
			var all = _dayOffRulesRepository.LoadAllByAgentGroup(agentGroup);

			var result = all.Select(dayOffRules => _dayOffRulesMapper.ToModel(dayOffRules)).ToList();
			return result;
		}
	}
}