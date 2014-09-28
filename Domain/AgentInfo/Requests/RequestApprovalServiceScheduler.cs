using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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
        private readonly IGlobalSettingDataRepository _globalSettingsDataRepository;
        
        public RequestApprovalServiceScheduler(IScheduleDictionary scheduleDictionary,
                                                    IScenario scenario,
                                                    ISwapAndModifyService swapAndModifyService,
                                                    INewBusinessRuleCollection newBusinessRules,
                                                    IScheduleDayChangeCallback scheduleDayChangeCallback,
                                                    IGlobalSettingDataRepository globalSettingsDataRepository
                                                )
        {
            _scenario = scenario;
            _swapAndModifyService = swapAndModifyService;
            _scheduleDictionary = scheduleDictionary;
            _newBusinessRules = newBusinessRules;
            _scheduleDayChangeCallback = scheduleDayChangeCallback;
            _globalSettingsDataRepository = globalSettingsDataRepository;
        
        }

        public IEnumerable<IBusinessRuleResponse> ApproveAbsence(IAbsence absence, DateTimePeriod period, IPerson person)
        {
            IScheduleRange totalScheduleRange = _scheduleDictionary[person];
            IScheduleDay dayScheduleForAbsenceReqStart =
                totalScheduleRange.ScheduledDay(
                    new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));
            IScheduleDay dayScheduleForAbsenceReqEnd =
                totalScheduleRange.ScheduledDay(
                    new DateOnly(period.EndDateTimeLocal(person.PermissionInformation.DefaultTimeZone())));

            IList<IBusinessRuleResponse> ret = new List<IBusinessRuleResponse>();

            var fullDayTimeSpanStart = new TimeSpan (0, 0, 0);
            var fullDayTimeSpanEnd = new TimeSpan (23, 59, 0);
            var absencePeriodUserTime =
                new DateTimePeriod (
                    DateTime.SpecifyKind (
                        TimeZoneHelper.ConvertFromUtc (period.StartDateTime, person.PermissionInformation.DefaultTimeZone()),
                        DateTimeKind.Utc),
                    DateTime.SpecifyKind (
                        TimeZoneHelper.ConvertFromUtc(period.EndDateTime, person.PermissionInformation.DefaultTimeZone()),
                        DateTimeKind.Utc));

            bool isFullDayAbsenceRequest = (absencePeriodUserTime.StartDateTime.TimeOfDay == fullDayTimeSpanStart &&
                                            absencePeriodUserTime.EndDateTime.TimeOfDay == fullDayTimeSpanEnd);
            if (isFullDayAbsenceRequest)
            {
                var fullDayAbsenceRequestStartTimeSetting = _globalSettingsDataRepository.FindValueByKey("FullDayAbsenceRequestStartTime", new TimeSpanSetting(new TimeSpan(0, 0, 0)));
                var fullDayAbsenceRequestEndTimeSetting = _globalSettingsDataRepository.FindValueByKey("FullDayAbsenceRequestEndTime", new TimeSpanSetting(new TimeSpan(23, 59, 0)));

                var settingStartTime = fullDayAbsenceRequestStartTimeSetting.TimeSpanValue;
                var settingEndTime = fullDayAbsenceRequestEndTimeSetting.TimeSpanValue;

                var startDate =new DateTime (   absencePeriodUserTime.StartDateTime.Year, 
                                                absencePeriodUserTime.StartDateTime.Month,
                                                absencePeriodUserTime.StartDateTime.Day, 
                                                settingStartTime.Hours, settingStartTime.Minutes, settingStartTime.Seconds);
                var endDate =new DateTime (     absencePeriodUserTime.EndDateTime.Year, 
                                                absencePeriodUserTime.EndDateTime.Month,
                                                absencePeriodUserTime.EndDateTime.Day, 
                                                settingEndTime.Hours, settingEndTime.Minutes, settingEndTime.Seconds);

				if (dayScheduleForAbsenceReqStart.IsScheduled() && dayScheduleForAbsenceReqStart.PersonAssignment() != null)
				{
					var dayScheduleStartTimeForAbsenceReqStart =
						dayScheduleForAbsenceReqStart.PersonAssignment()
							.Period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
					startDate = startDate < dayScheduleStartTimeForAbsenceReqStart ? startDate : dayScheduleStartTimeForAbsenceReqStart;
				}

				if (dayScheduleForAbsenceReqEnd.IsScheduled() && dayScheduleForAbsenceReqEnd.PersonAssignment() != null)
				{
					var dayScheduleEndTimeForAbsenceReqEnd = dayScheduleForAbsenceReqEnd.PersonAssignment()
							.Period.EndDateTimeLocal(person.PermissionInformation.DefaultTimeZone());
					endDate = endDate > dayScheduleEndTimeForAbsenceReqEnd ? endDate : dayScheduleEndTimeForAbsenceReqEnd;
				}

                period =
                   new DateTimePeriod (
                        DateTime.SpecifyKind (
                            TimeZoneHelper.ConvertToUtc ( startDate, person.PermissionInformation.DefaultTimeZone()),
                            DateTimeKind.Utc),
                        DateTime.SpecifyKind (
                            TimeZoneHelper.ConvertToUtc(endDate, person.PermissionInformation.DefaultTimeZone()),
                            DateTimeKind.Utc));
               
            }
            
            if (dayScheduleForAbsenceReqStart.FullAccess)
            {
                var layer = new AbsenceLayer(absence, period);
                var personAbsence = new PersonAbsence(person, _scenario, layer);

                dayScheduleForAbsenceReqStart.Add(personAbsence);
                
                var result = _scheduleDictionary.Modify(ScheduleModifier.Request, dayScheduleForAbsenceReqStart, _newBusinessRules,_scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
                if (!result.IsEmpty())
                {
					// Why this call again? None is overridden before
                    result = _scheduleDictionary.Modify(ScheduleModifier.Request, dayScheduleForAbsenceReqStart, _newBusinessRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
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
