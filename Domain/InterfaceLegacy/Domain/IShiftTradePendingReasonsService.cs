using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IShiftTradePendingReasonsService
	{
		void SetBrokenBusinessRulesFieldOnPersonRequest(IEnumerable<IBusinessRuleResponse> ruleRepsonses, IPersonRequest personRequest);
	}
}