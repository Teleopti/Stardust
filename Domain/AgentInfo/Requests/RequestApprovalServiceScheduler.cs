using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    public class RequestApprovalServiceScheduler : IRequestApprovalService
    {
        private readonly IScenario _scenario;
        private readonly ISwapAndModifyService _swapAndModifyService;
        private readonly IScheduleDictionary _scheduleDictionary;
        private readonly INewBusinessRuleCollection _newBusinessRules;
        private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

        public RequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary,
                                                    IScenario scenario,
                                                    ISwapAndModifyService swapAndModifyService,
                                                    INewBusinessRuleCollection newBusinessRules,
            IScheduleDayChangeCallback scheduleDayChangeCallback)
        {
            _scenario = scenario;
            _swapAndModifyService = swapAndModifyService;
            _scheduleDictionary = scheduleDictionary;
            _newBusinessRules = newBusinessRules;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
        }

        public IEnumerable<IBusinessRuleResponse> ApproveAbsence(IAbsence absence, DateTimePeriod period, IPerson person)
        {
            IScheduleRange totalScheduleRange = _scheduleDictionary[person];
            IScheduleDay daySchedule =
                totalScheduleRange.ScheduledDay(
                    new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));
            IList<IBusinessRuleResponse> ret = new List<IBusinessRuleResponse>();

            if (daySchedule.FullAccess)
            {
                var layer = new AbsenceLayer(absence, period);
                var personAbsence = new PersonAbsence(person, _scenario, layer);

                daySchedule.Add(personAbsence);
                
                var result = _scheduleDictionary.Modify(ScheduleModifier.Request, daySchedule, _newBusinessRules,_scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
                if (!result.IsEmpty())
                {
					// Why this call again? None is overridden before
                    result = _scheduleDictionary.Modify(ScheduleModifier.Request, daySchedule, _newBusinessRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
                }

                foreach (var response in result)
                {
                    if(!response.Overridden)
                        ret.Add(response);
                }
                return ret;
            } 
            // this can probably not happen
            // Anyway, not full access is not an error that can be overridden
            return new List<IBusinessRuleResponse>();
        }

        public IEnumerable<IBusinessRuleResponse> ApproveShiftTrade(IShiftTradeRequest shiftTradeRequest)
        {
            return _swapAndModifyService.SwapShiftTradeSwapDetails(shiftTradeRequest.ShiftTradeSwapDetails,
                                                                  _scheduleDictionary,
                                                                   _newBusinessRules, new ScheduleTagSetter(NullScheduleTag.Instance));
        }
    }
}
