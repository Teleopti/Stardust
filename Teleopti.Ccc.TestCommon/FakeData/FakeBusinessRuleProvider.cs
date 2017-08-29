using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeBusinessRuleProvider:IBusinessRuleProvider
	{
		private INewBusinessRuleCollection _businessRuleCollection;
		private IBusinessRuleResponse _deniableResponse;

		public void SetBusinessRules(INewBusinessRuleCollection businessRuleCollection)
		{
			_businessRuleCollection = businessRuleCollection;
		}

		public void SetDeniableResponse(IBusinessRuleResponse deniableResponse)
		{
			_deniableResponse = deniableResponse;
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

		public IBusinessRuleResponse GetFirstDeniableResponse(INewBusinessRuleCollection enabledRules, IList<IBusinessRuleResponse> ruleResponses)
		{
			return _deniableResponse;
		}
	}
}