using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeBusinessRuleProvider:IBusinessRuleProvider
	{
		private INewBusinessRuleCollection _businessRuleCollection;
		private bool _shouldDeny;

		public void SetBusinessRules(INewBusinessRuleCollection businessRuleCollection)
		{
			_businessRuleCollection = businessRuleCollection;
		}

		public void SetShouldDeny(bool shouldDeny)
		{
			_shouldDeny = shouldDeny;
		}

		public INewBusinessRuleCollection GetAllBusinessRules(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return _businessRuleCollection;
		}

		public INewBusinessRuleCollection GetBusinessRulesForShiftTradeRequest(
			ISchedulingResultStateHolder schedulingResultStateHolder, bool enableSiteOpenHoursRule)
		{
			return _businessRuleCollection;
		}

		public INewBusinessRuleCollection GetAllEnabledBusinessRulesForShiftTradeRequest(
			ISchedulingResultStateHolder schedulingResultStateHolder, bool enableSiteOpenHoursRule)
		{
			return _businessRuleCollection;
		}

		public bool ShouldDeny(INewBusinessRuleCollection enabledRules, IList<IBusinessRuleResponse> ruleResponses)
		{
			return _shouldDeny;
		}
	}
}