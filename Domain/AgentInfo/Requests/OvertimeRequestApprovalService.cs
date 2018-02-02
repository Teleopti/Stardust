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
		private readonly ISkill[] _validatedSkills;
		private readonly INow _now;

		public OvertimeRequestApprovalService(
			IOvertimeRequestUnderStaffingSkillProvider overtimeRequestUnderStaffingSkillProvider,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider, ICommandDispatcher commandDispatcher, ISkill[] validatedSkills, INow now)
		{
			_overtimeRequestUnderStaffingSkillProvider = overtimeRequestUnderStaffingSkillProvider;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_commandDispatcher = commandDispatcher;
			_validatedSkills = validatedSkills;
			_now = now;
		}

		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			var overtimeRequest = request as IOvertimeRequest;
			if (overtimeRequest == null)
			{
				throw new InvalidCastException("Request type should be OvertimeRequest!");
			}

			if (_validatedSkills != null && _validatedSkills.Length > 0)
			{
				addOvertimeActivity(_validatedSkills.First().Activity.Id.GetValueOrDefault(), overtimeRequest);

				return new List<IBusinessRuleResponse>();
			}
			var period = overtimeRequest.Period;
			var person = overtimeRequest.Person;
			var skills = _overtimeRequestSkillProvider.GetAvailableSkills(request.Person, period).ToList();
			if (!skills.Any())
			{
				return getBusinessRuleResponses(Resources.NoAvailableSkillForOvertime, period, person);
			}

			Guid activityId;
			var seriousUnderstaffingSkills = _overtimeRequestUnderStaffingSkillProvider.GetSeriousUnderstaffingSkills(period, skills);
			if (seriousUnderstaffingSkills.Count == 0)
			{
				activityId = skills.FirstOrDefault().Activity.Id.GetValueOrDefault();
			}
			else
			{
				activityId = seriousUnderstaffingSkills.First().Activity.Id.GetValueOrDefault();
			}

			addOvertimeActivity(activityId, overtimeRequest);

			return new List<IBusinessRuleResponse>();
		}
		
		public void addOvertimeActivity(Guid activityId, IOvertimeRequest overtimeRequest)
		{
			var agentDateTime = TimeZoneHelper.ConvertFromUtc(overtimeRequest.Period.StartDateTime, overtimeRequest.Person.PermissionInformation.DefaultTimeZone());
			_commandDispatcher.Execute(new AddOvertimeActivityCommand
			{
				ActivityId = activityId,
				Date = new DateOnly(agentDateTime),
				MultiplicatorDefinitionSetId = overtimeRequest.MultiplicatorDefinitionSet.Id.GetValueOrDefault(),
				Period = overtimeRequest.Period,
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

		private IOvertimeRequestOpenPeriod getOvertimeRequestOpenPeriod(IPersonRequest personRequest)
		{
			if (personRequest.Person.WorkflowControlSet == null)
				return null;
			return personRequest.Person.WorkflowControlSet.GetMergedOvertimeRequestOpenPeriod(personRequest.Request as IOvertimeRequest,
				new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), personRequest.Person.PermissionInformation.DefaultTimeZone())));
		}
	}
}
