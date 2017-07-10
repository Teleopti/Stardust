using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestApprovalService : IRequestApprovalService
	{
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public OvertimeRequestApprovalService(IScheduleDictionary scheduleDictionary, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scheduleDictionary = scheduleDictionary;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			var overtimeRequest = request as IOvertimeRequest;
			if (overtimeRequest == null)
			{
				throw new InvalidCastException("Request type should be OvertimeRequest!");
			}

			var person = overtimeRequest.Person;
			var period = overtimeRequest.Period;
			var definitionSet = overtimeRequest.MultiplicatorDefinitionSet;
			var scheduleDateOnly = new DateOnly(period.StartDateTimeLocal(person.PermissionInformation.DefaultTimeZone()));

			var scheduleRange = _scheduleDictionary[person];
			var scheduleDay = scheduleRange.ScheduledDay(scheduleDateOnly);

			// todo should get correct skill not the first one
			var activity = person.Period(scheduleDateOnly).PersonSkillCollection.First().Skill.Activity;

			scheduleDay.CreateAndAddOvertime(activity, period, definitionSet);

			_scheduleDictionary.Modify(scheduleDay, _scheduleDayChangeCallback);

			return new List<IBusinessRuleResponse>();
		}
	}
}
