using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class RequestFactory : IRequestFactory
	{
	    private readonly IPersonRepository _personRepository;
	    private readonly IScheduleRepository _scheduleRepository;
	    private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
	    private readonly ISkillRepository _skillRepository;
	    private readonly IWorkloadRepository _workloadRepository;
	    private readonly ISkillDayLoadHelper _skillDayLoadHelper;
	    private readonly IPeopleAndSkillLoaderDecider _peopleAndSkillLoaderDecider;
	    private readonly ISwapAndModifyService _swapAndModifyService;
	    private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;

	    public RequestFactory(IPersonRepository personRepository, IScheduleRepository scheduleRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, ISkillRepository skillRepository, IWorkloadRepository workloadRepository, ISkillDayLoadHelper skillDayLoadHelper, IPeopleAndSkillLoaderDecider peopleAndSkillLoaderDecider, ISwapAndModifyService swapAndModifyService, IPersonRequestCheckAuthorization personRequestCheckAuthorization)
		{
		    _personRepository = personRepository;
	        _scheduleRepository = scheduleRepository;
	        _personAbsenceAccountRepository = personAbsenceAccountRepository;
	        _skillRepository = skillRepository;
	        _workloadRepository = workloadRepository;
	        _skillDayLoadHelper = skillDayLoadHelper;
	        _peopleAndSkillLoaderDecider = peopleAndSkillLoaderDecider;
	        _swapAndModifyService = swapAndModifyService;
	        _personRequestCheckAuthorization = personRequestCheckAuthorization;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IRequestApprovalService GetRequestApprovalService(INewBusinessRuleCollection allNewRules, ISchedulingResultStateHolder schedulingResultStateHolder, IScenario scenario)
		{
			return new RequestApprovalServiceScheduler(schedulingResultStateHolder.Schedules, 
													   scenario, _swapAndModifyService,  allNewRules, new EmptyScheduleDayChangeCallback());
		}

		public IPersonAccountProjectionService GetPersonAccountProjectionService(IAccount account, IScheduleRange range)
		{
			return new PersonAccountProjectionService(account, range);
		}

		public ILoadSchedulingStateHolderForResourceCalculation GetSchedulingLoader(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
		    return new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository,
		                                                               _skillRepository, _workloadRepository,
		                                                               _scheduleRepository, schedulingResultStateHolder,
		                                                               _peopleAndSkillLoaderDecider,
		                                                               _skillDayLoadHelper);
		}

		public IShiftTradeRequestStatusChecker GetShiftTradeRequestStatusChecker(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return new ShiftTradeRequestStatusCheckerWithSchedule(schedulingResultStateHolder.Schedules, _personRequestCheckAuthorization);
		}
	}
}