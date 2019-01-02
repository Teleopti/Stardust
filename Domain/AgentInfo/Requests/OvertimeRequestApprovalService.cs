using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestApprovalService : IRequestApprovalService
	{
		private readonly IOvertimeRequestUnderStaffingSkillProvider _overtimeRequestUnderStaffingSkillProvider;
		private readonly IOvertimeRequestSkillProvider _overtimeRequestSkillProvider;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IDictionary<DateTimePeriod, IList<ISkill>> _validatedSkillDictionary;
		private readonly ISkillOpenHourFilter _skillOpenHourFilter;
		private readonly IOvertimeActivityBelongsToDateProvider _overtimeActivityBelongsToDateProvider;

		public OvertimeRequestApprovalService(
			IOvertimeRequestUnderStaffingSkillProvider overtimeRequestUnderStaffingSkillProvider,
			IOvertimeRequestSkillProvider overtimeRequestSkillProvider, ICommandDispatcher commandDispatcher,
			IDictionary<DateTimePeriod, IList<ISkill>> validatedSkillDictionary, ISkillOpenHourFilter skillOpenHourFilter, IOvertimeActivityBelongsToDateProvider overtimeActivityBelongsToDateProvider)
		{
			_overtimeRequestUnderStaffingSkillProvider = overtimeRequestUnderStaffingSkillProvider;
			_overtimeRequestSkillProvider = overtimeRequestSkillProvider;
			_commandDispatcher = commandDispatcher;
			_validatedSkillDictionary = validatedSkillDictionary;
			_skillOpenHourFilter = skillOpenHourFilter;
			_overtimeActivityBelongsToDateProvider = overtimeActivityBelongsToDateProvider;
		}

		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			if (!(request is IOvertimeRequest overtimeRequest))
			{
				throw new InvalidCastException("Request type should be OvertimeRequest!");
			}

			if (_validatedSkillDictionary != null && _validatedSkillDictionary.Count > 0)
			{
				mergeAndAddOvertimeActivities(_validatedSkillDictionary, overtimeRequest);
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
				mergeAndAddOvertimeActivities(seriousUnderstaffingSkillDictionary, overtimeRequest);
			}

			return new List<IBusinessRuleResponse>();
		}

		private void mergeAndAddOvertimeActivities(IDictionary<DateTimePeriod, IList<ISkill>> seriousUnderstaffingSkillDictionary,
			IOvertimeRequest overtimeRequest)
		{
			IDictionary<DateTimePeriod, Guid> dictionary = new Dictionary<DateTimePeriod, Guid>();
			DateTimePeriod? lastPeriod = null;
			Guid? lastActivityId = null;
			foreach (var seriousUnderstaffingSkillItem in seriousUnderstaffingSkillDictionary)
			{
				if (seriousUnderstaffingSkillItem.Value.Any())
				{
					var currentActivityId = seriousUnderstaffingSkillItem.Value.First().Activity.Id.GetValueOrDefault();
					var currentPeriod = seriousUnderstaffingSkillItem.Key;
					if (!lastPeriod.HasValue)
					{
						lastPeriod = seriousUnderstaffingSkillItem.Key;
						lastActivityId = currentActivityId;
						dictionary.Add(lastPeriod.Value,lastActivityId.Value);
					}
					else
					{
						if (lastActivityId.Value == currentActivityId &&
							lastPeriod.Value.EndDateTime.Equals(currentPeriod.StartDateTime))
						{
							dictionary.Remove(lastPeriod.Value);
							lastPeriod = lastPeriod.Value.ChangeEndTime(currentPeriod.ElapsedTime());
							dictionary.Add(lastPeriod.Value, lastActivityId.Value);
						}
						else
						{
							dictionary.Add(currentPeriod, currentActivityId);
						}
					}
				}
			}

			foreach (var dic in dictionary)
			{
				addOvertimeActivity(dic.Value,dic.Key, overtimeRequest);
			}
		}

		private void addOvertimeActivity(Guid activityId, DateTimePeriod period, IOvertimeRequest overtimeRequest)
		{
			var belongsToDate = _overtimeActivityBelongsToDateProvider.GetBelongsToDate(overtimeRequest.Person, period);
			_commandDispatcher.Execute(new AddOvertimeActivityCommand
			{
				ActivityId = activityId,
				Date = belongsToDate,
				MultiplicatorDefinitionSetId = overtimeRequest.MultiplicatorDefinitionSet.Id.GetValueOrDefault(),
				Period = period,
				Person = overtimeRequest.Person,
				AllowDisconnected = true
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
