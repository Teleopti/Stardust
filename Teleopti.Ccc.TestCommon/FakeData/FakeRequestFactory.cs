using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.Services;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeRequestFactory : IRequestFactory
	{
		IScheduleDictionary _scheduleDictionary;
		private IPersonRequestRepository _personRequestRepository;

		public void SetScheduleDictionary(IScheduleDictionary scheduleDictionary)
		{
			_scheduleDictionary = scheduleDictionary;
		}

		public void SetPersonRequestRepository(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}

		public IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario, ISchedulingResultStateHolder schedulingResultStateHolder, IPersonRequest personRequest)
		{
			var approvalService = new ShiftTradeRequestApprovalService(_scheduleDictionary, 
				new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack(), new FakeTimeZoneGuard()), allNewRules, new PersonRequestAuthorizationCheckerForTest(), _personRequestRepository);
	        return approvalService;
		}

		public IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker(
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return new ShiftTradeRequestStatusCheckerForTestDoesNothing();
		}
	}
}
