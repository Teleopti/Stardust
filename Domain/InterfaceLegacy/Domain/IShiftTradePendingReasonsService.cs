using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IShiftTradePendingReasonsService
	{
		void SetBrokenBusinessRulesFieldOnPersonRequest(IEnumerable<IBusinessRuleResponse> ruleRepsonses, IPersonRequest personRequest);
	}
}