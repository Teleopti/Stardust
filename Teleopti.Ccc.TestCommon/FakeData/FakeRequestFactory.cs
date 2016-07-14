
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeRequestFactory : IRequestFactory
	{
        IRequestApprovalServiceFactory approvalServiceFactory = new FakeRequestApprovalServiceFactory();

        public IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, IScenario scenario, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
            return approvalServiceFactory.MakeRequestApprovalServiceScheduler(new FakeScheduleDictionary(), new Scenario("test"),new Person() );
		}

		public IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker(
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return new ShiftTradeRequestStatusCheckerForTestDoesNothing();
		}

	    public void setRequestApprovalService(IRequestApprovalService approvalService)
	    {
            ((FakeRequestApprovalServiceFactory)approvalServiceFactory).SetApproveService(approvalService);
        }
    }
}
