using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class FixedRuleSetBagExtractor : IRuleSetBagExtractor
	{
		private readonly IRuleSetBag _ruleSetBag;

		public FixedRuleSetBagExtractor(IRuleSetBag ruleSetBag)
		{
			_ruleSetBag = ruleSetBag;
		}

		public IEnumerable<IRuleSetBag> GetRuleSetBag(ITeamBlockInfo teamBlockInfo)
		{
			return new[] {_ruleSetBag};
		}

		public IRuleSetBag GetRuleSetBagForTeamMember(IPerson person, DateOnly dateOnly)
		{
			return _ruleSetBag;
		}
	}
}