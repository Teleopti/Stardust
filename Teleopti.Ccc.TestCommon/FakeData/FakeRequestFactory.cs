using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeRequestFactory : IRequestFactory
	{
        IRequestApprovalServiceFactory approvalServiceFactory = new FakeRequestApprovalServiceFactory();

		IScheduleDictionary _scheduleDictionary = null;

		public void SetScheduleDictionary(IScheduleDictionary scheduleDictionary)
		{
			_scheduleDictionary = scheduleDictionary;
		}

		public IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario, ISchedulingResultStateHolder schedulingResultStateHolder, IPersonRequest personRequest)
		{
			var approvalService = new RequestApprovalServiceScheduler(_scheduleDictionary, 
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
