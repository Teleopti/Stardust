using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestApprovalService : IRequestApprovalService
	{
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IOvertimeRequestUnderStaffingSkillProvider _overtimeRequestUnderStaffingSkillProvider;
		private readonly IOvertimeRequestSkillProvider _overtimeRequestSkillProvider;
		private readonly ISkillOpenHourFilter _skillOpenHourFilter;

		public OvertimeRequestApprovalService(IScheduleDictionary scheduleDictionary,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IOvertimeRequestUnderStaffingSkillProvider overtimeRequestUnderStaffingSkillProvider,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider,
			ISkillOpenHourFilter skillOpenHourFilter)
		{
			_scheduleDictionary = scheduleDictionary;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_overtimeRequestUnderStaffingSkillProvider = overtimeRequestUnderStaffingSkillProvider;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_skillOpenHourFilter = skillOpenHourFilter;
		}

		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			var overtimeRequest = request as IOvertimeRequest;
			if (overtimeRequest == null)
			{
				throw new InvalidCastException("Request type should be OvertimeRequest!");
			}

			var period = overtimeRequest.Period;
			var person = overtimeRequest.Person;
			var skills = _overtimeRequestSkillProvider.GetAvailableSkills(period).ToList();
			if (!skills.Any())
			{
				return new IBusinessRuleResponse[]
				{
					new BusinessRuleResponse(null, "There is no available skill for overtime", true, true, period
						, person, period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), string.Empty)
				};
			}

			skills = _skillOpenHourFilter.Filter(period, skills).ToList();
			if (!skills.Any())
			{
				return new IBusinessRuleResponse[]
				{
					new BusinessRuleResponse(null, "There is no open skill in request period", true, true, period
						, person, period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), string.Empty)
				};
			}

			var skill = _overtimeRequestUnderStaffingSkillProvider.GetUnderStaffingSkill(period, skills);
			if (skill == null)
			{
				return new IBusinessRuleResponse[]
				{
					new BusinessRuleResponse(null, "There is no critical underStaffing skill for overtime request", true, true, period
						, person, period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), string.Empty)
				};
			}

			var definitionSet = overtimeRequest.MultiplicatorDefinitionSet;
			var scheduleDateOnly = new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone()));
			var scheduleRange = _scheduleDictionary[person];
			var scheduleDay = scheduleRange.ScheduledDay(scheduleDateOnly);
			scheduleDay.CreateAndAddOvertime(skill.Activity, period, definitionSet);
			_scheduleDictionary.Modify(scheduleDay, _scheduleDayChangeCallback);

			return new List<IBusinessRuleResponse>();
		}
	}
}
