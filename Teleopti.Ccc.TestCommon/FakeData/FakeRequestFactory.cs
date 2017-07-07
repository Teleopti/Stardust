using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.Services;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeRequestFactory : IRequestFactory
	{
		IScheduleDictionary _scheduleDictionary = null;

		public void SetScheduleDictionary(IScheduleDictionary scheduleDictionary)
		{
			_scheduleDictionary = scheduleDictionary;
		}

		public IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario, ISchedulingResultStateHolder schedulingResultStateHolder, IPersonRequest personRequest)
		{
			var approvalService = new ShiftTradeRequestApprovalService(_scheduleDictionary, 
				new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack()), allNewRules, null);
	        return approvalService;
		}

		public IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker(
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return new ShiftTradeRequestStatusCheckerForTestDoesNothing();
		}
	}
}
