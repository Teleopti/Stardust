﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestApprovalService : IRequestApprovalService
	{
		private readonly IOvertimeRequestUnderStaffingSkillProvider _overtimeRequestUnderStaffingSkillProvider;
		private readonly IOvertimeRequestSkillProvider _overtimeRequestSkillProvider;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IDictionary<DateTimePeriod, IList<ISkill>> _validatedSkillDictionary;
		private readonly ISkillOpenHourFilter _skillOpenHourFilter;

		public OvertimeRequestApprovalService(
			IOvertimeRequestUnderStaffingSkillProvider overtimeRequestUnderStaffingSkillProvider,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider, ICommandDispatcher commandDispatcher, IDictionary<DateTimePeriod, IList<ISkill>> validatedSkillDictionary, ISkillOpenHourFilter skillOpenHourFilter)
		{
			_overtimeRequestUnderStaffingSkillProvider = overtimeRequestUnderStaffingSkillProvider;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_commandDispatcher = commandDispatcher;
			_validatedSkillDictionary = validatedSkillDictionary;
			_skillOpenHourFilter = skillOpenHourFilter;
		}

		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			var overtimeRequest = request as IOvertimeRequest;
			if (overtimeRequest == null)
			{
				throw new InvalidCastException("Request type should be OvertimeRequest!");
			}

			if (_validatedSkillDictionary != null && _validatedSkillDictionary.Count > 0)
			{
				addOvertimeActivities(_validatedSkillDictionary, overtimeRequest);
				return new List<IBusinessRuleResponse>();
			}

			var period = overtimeRequest.Period;
			var person = overtimeRequest.Person;
			var skills = _overtimeRequestSkillProvider.GetAvailableSkillsBySkillType(request.Person, period).ToList();
			if (!skills.Any())
			{
				return getBusinessRuleResponses(Resources.ThereIsNoAvailableSkillForOvertime, period, person);
			}

			skills = _skillOpenHourFilter.Filter(period, skills).ToList();
			if (!skills.Any())
			{
				return getBusinessRuleResponses(Resources.PeriodIsOutOfSkillOpenHours, period, person);
			}

			var seriousUnderstaffingSkillDictionary = _overtimeRequestUnderStaffingSkillProvider.GetSeriousUnderstaffingSkills(period, skills, person);
			if (seriousUnderstaffingSkillDictionary.Count == 0)
			{
				var activityId = skills.FirstOrDefault().Activity.Id.GetValueOrDefault();
				addOvertimeActivity(activityId, overtimeRequest.Period, overtimeRequest);
			}
			else
			{
				addOvertimeActivities(seriousUnderstaffingSkillDictionary, overtimeRequest);
			}

			return new List<IBusinessRuleResponse>();
		}

		private void addOvertimeActivities(IDictionary<DateTimePeriod, IList<ISkill>> seriousUnderstaffingSkillDictionary,
			IOvertimeRequest overtimeRequest)
		{
			foreach (var seriousUnderstaffingSkillItem in seriousUnderstaffingSkillDictionary)
			{
				if (seriousUnderstaffingSkillItem.Value.Any())
					addOvertimeActivity(seriousUnderstaffingSkillItem.Value.First().Activity.Id.GetValueOrDefault(),
						seriousUnderstaffingSkillItem.Key, overtimeRequest);
			}
		}

		private void addOvertimeActivity(Guid activityId, DateTimePeriod period, IOvertimeRequest overtimeRequest)
		{
			var agentDateTime = TimeZoneHelper.ConvertFromUtc(period.StartDateTime, overtimeRequest.Person.PermissionInformation.DefaultTimeZone());
			_commandDispatcher.Execute(new AddOvertimeActivityCommand
			{
				ActivityId = activityId,
				Date = new DateOnly(agentDateTime),
				MultiplicatorDefinitionSetId = overtimeRequest.MultiplicatorDefinitionSet.Id.GetValueOrDefault(),
				Period = period,
				Person = overtimeRequest.Person
			});
		}

		private static IEnumerable<IBusinessRuleResponse> getBusinessRuleResponses(string message, DateTimePeriod period, IPerson person)
		{
			return new IBusinessRuleResponse[]
			{
				new BusinessRuleResponse(null, message, true, true, period
					, person, period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), string.Empty)
			};
		}
	}
}
