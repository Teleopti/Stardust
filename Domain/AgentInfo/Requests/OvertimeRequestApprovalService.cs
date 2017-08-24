using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly ISkillOpenHourFilter _skillOpenHourFilter;
		private readonly IRequestAddOverTimeActivityHandler _addOverTimeActivityHandler;

		public OvertimeRequestApprovalService(
			IOvertimeRequestUnderStaffingSkillProvider overtimeRequestUnderStaffingSkillProvider,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider,
			ISkillOpenHourFilter skillOpenHourFilter, IRequestAddOverTimeActivityHandler addOverTimeActivityHandler)
		{
			_overtimeRequestUnderStaffingSkillProvider = overtimeRequestUnderStaffingSkillProvider;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_skillOpenHourFilter = skillOpenHourFilter;
			_addOverTimeActivityHandler = addOverTimeActivityHandler;
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
				return getBusinessRuleResponses(Resources.NoAvailableSkillForOvertime, period, person);
			}

			skills = _skillOpenHourFilter.Filter(period, skills).ToList();
			if (!skills.Any())
			{
				return getBusinessRuleResponses(Resources.PeriodIsOutOfSkillOpenHours, period, person);
			}

			var seriousUnderstaffingSkills = _overtimeRequestUnderStaffingSkillProvider.GetSeriousUnderstaffingSkills(period, skills);
			if (seriousUnderstaffingSkills.Count == 0)
			{
				return getBusinessRuleResponses(Resources.NoUnderStaffingSkill, period, person);
			}

			_addOverTimeActivityHandler.Handle(seriousUnderstaffingSkills.First().Activity.Id.GetValueOrDefault(), overtimeRequest);

			return new List<IBusinessRuleResponse>();
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
