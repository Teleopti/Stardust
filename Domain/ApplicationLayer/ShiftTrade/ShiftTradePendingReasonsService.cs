using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	public class ShiftTradePendingReasonsService : IShiftTradePendingReasonsService
	{
		public void SetBrokenBusinessRulesFieldOnPersonRequest(IEnumerable<IBusinessRuleResponse> ruleRepsonses, IPersonRequest personRequest)
		{
			var ruleTypes = ruleRepsonses.Select(r => r.TypeOfRule);
			var rulesToSave = NewBusinessRuleCollection.GetFlagFromRules(ruleTypes);
			personRequest.TrySetBrokenBusinessRule(rulesToSave);
		}
	}
}
