using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
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
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IScenario _scenario;
		private readonly IScheduleStorage _scheduleStorage;


		public OvertimeRequestApprovalService(
			IOvertimeRequestUnderStaffingSkillProvider overtimeRequestUnderStaffingSkillProvider,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider,
			ISkillOpenHourFilter skillOpenHourFilter, ICommandDispatcher commandDispatcher, IScheduleStorage scheduleStorage, IScenario scenario)
		{
			_overtimeRequestUnderStaffingSkillProvider = overtimeRequestUnderStaffingSkillProvider;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_skillOpenHourFilter = skillOpenHourFilter;
			_commandDispatcher = commandDispatcher;
			_scheduleStorage = scheduleStorage;
			_scenario = scenario;
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

			var withoutActivitySkills = seriousUnderstaffingSkills
				.Where(s => !activityAlreadyExists(period, person, s.Activity)).ToList();
			if (withoutActivitySkills.Count == 0)
			{
				return getBusinessRuleResponses(Resources.OvertimeRequestSameActivityInPeriod, period, person);
			}

			// todo only return the first activity of skill now
			addOvertimeActivity(withoutActivitySkills.First().Activity.Id.GetValueOrDefault(), overtimeRequest);

			return new List<IBusinessRuleResponse>();
		}

		private bool activityAlreadyExists(DateTimePeriod period, IPerson person, IActivity activity)
		{

			var dic = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), _scenario,
				new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });

			var scheduleDay = dic[person].ScheduledDay(new DateOnly(period.StartDateTime));

			var personAssignment = scheduleDay.PersonAssignment();
			if (personAssignment == null)
				return false;

			var mainActivities = personAssignment.MainActivities();
			var overtimeActivities = personAssignment.OvertimeActivities();

			return mainActivities.Any(a => a.Payload == activity && a.Period.Intersect(period))
					||
					overtimeActivities.Any(a => a.Payload == activity && a.Period.Intersect(period));
		}

		private void addOvertimeActivity(Guid activityId, IOvertimeRequest overtimeRequest)
		{
			_commandDispatcher.Execute(new AddOvertimeActivityCommand
			{
				ActivityId = activityId,
				Date = new DateOnly(overtimeRequest.Period.StartDateTime),
				MultiplicatorDefinitionSetId = overtimeRequest.MultiplicatorDefinitionSet.Id.GetValueOrDefault(),
				Period = overtimeRequest.Period,
				PersonId = overtimeRequest.Person.Id.GetValueOrDefault()
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
