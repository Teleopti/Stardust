using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public interface IRequestFactory
    {
        IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, ISchedulingResultStateHolder schedulingResultStateHolder, IScenario scenario);
        IPersonAccountProjectionService GetPersonAccountProjectionService(IAccount account, IScheduleRange range);
        ILoadSchedulingStateHolderForResourceCalculation GetSchedulingLoader(ISchedulingResultStateHolder schedulingResultStateHolder);
        IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker(ISchedulingResultStateHolder schedulingResultStateHolder);
    }
}