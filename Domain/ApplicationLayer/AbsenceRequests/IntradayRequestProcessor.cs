﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	public class IntradayRequestProcessor : IIntradayRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayRequestProcessor));

		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public IntradayRequestProcessor(ICommandDispatcher commandDispatcher, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_commandDispatcher = commandDispatcher;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		public void Process(IPersonRequest personRequest)
		{
			personRequest.Pending();
			var cascadingPersonSkills = personRequest.Person.Period(DateOnly.Today).CascadingSkills();

			var lowestIndex = cascadingPersonSkills.Min(x => x.Skill.CascadingIndex);
			var primarySkills = cascadingPersonSkills.Where(x => x.Skill.CascadingIndex == lowestIndex);
			
			//this could be improved to do one db call if there are many skills
			//var changes = _scheduleForecastSkillReadModelRepository.GetReadModelChanges(personRequest.Request.Period);

			foreach (var primarySkill in primarySkills)
			{
				//var skillStaffingIntervals =
				//	_scheduleForecastSkillReadModelRepository.GetBySkill(primarySkill.Skill.Id.GetValueOrDefault(),
				//		personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime);
				var skillStaffingIntervals = getSkillStaffIntervals(primarySkill.Skill.Id.GetValueOrDefault(), personRequest.Request.Period);
				//skillStaffingIntervals = applyChanges(skillStaffingIntervals,
				//	changes.Where(x => x.SkillId == primarySkill.Skill.Id.GetValueOrDefault()));

				var underStaffingDetails = getUnderStaffedPeriods(skillStaffingIntervals);

				if (underStaffingDetails.UnderstaffingTimes.Any())
				{
					var denyReason = GetUnderStaffingHourString(underStaffingDetails, personRequest);
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), denyReason);
					return;
				}

			}
			sendApproveCommand(personRequest.Id.GetValueOrDefault());
			
		}

		private IEnumerable<SkillStaffingInterval> getSkillStaffIntervals(Guid skillId, DateTimePeriod requestPeriod)
		{
			var skillStaffingIntervals =
				_scheduleForecastSkillReadModelRepository.GetBySkill(skillId, requestPeriod.StartDateTime, requestPeriod.EndDateTime);
			var mergedStaffingIntervals = new List<SkillStaffingInterval>();
			var intervalChanges =  _scheduleForecastSkillReadModelRepository.GetReadModelChanges(requestPeriod).Where(x => x.SkillId == skillId);
			if (intervalChanges.Any())
			{
				skillStaffingIntervals.ForEach(interval =>
				{
					var changes =
						intervalChanges.Where(x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime);
					if (changes.Any())
					{
						interval.StaffingLevel += changes.Sum((x => x.StaffingLevel));
					}
					mergedStaffingIntervals.Add(interval);
				});
			}
			else
			{
				mergedStaffingIntervals = skillStaffingIntervals.ToList();
			}
			return mergedStaffingIntervals;
		}


		//private IEnumerable<SkillStaffingInterval> applyChanges(IEnumerable<SkillStaffingInterval> skillStaffingIntervals, IEnumerable<StaffingIntervalChange> intervalChanges)
		//{
		//	if (intervalChanges.Any())
		//	{
		//		var mergedStaffingIntervals = new List<SkillStaffingInterval>();
		//		skillStaffingIntervals.ForEach(interval =>
		//		{
		//			var changes =
		//				intervalChanges.Where(x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime);
		//			if (changes.Any())
		//			{
		//				interval.StaffingLevel += changes.Sum((x => x.StaffingLevel));
		//			}
		//			mergedStaffingIntervals.Add(interval);
		//		});
		//		return mergedStaffingIntervals;
		//	}
		//	return skillStaffingIntervals;
		//}

		private UnderstaffingDetails getUnderStaffedPeriods(IEnumerable<SkillStaffingInterval> skillStaffingIntervals)
		{
			var result = new UnderstaffingDetails();
			foreach (var interval in skillStaffingIntervals.Where(x => x.StaffingLevel < x.ForecastWithShrinkage + 1))
			{
				result.AddUnderstaffingTime(new TimePeriod(interval.StartDateTime.Hour, interval.StartDateTime.Minute, interval.EndDateTime.Hour, interval.EndDateTime.Minute));
			}
			return result;
		}

		public string GetUnderStaffingHourString(UnderstaffingDetails underStaffingDetails, IPersonRequest personRequest)
		{
			var culture = personRequest.Person.PermissionInformation.Culture();
			var uiCulture = personRequest.Person.PermissionInformation.UICulture();
			var timeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();
			var dateTime = personRequest.Request.Period.StartDateTimeLocal(timeZone);

			var errorMessageBuilder = new StringBuilder();
				var understaffingHoursValidationError = string.Format(uiCulture,
					Resources.ResourceManager.GetString("InsufficientStaffingHours", uiCulture),
					dateTime.ToString("d", culture));
				var insufficientHours = string.Join(", ",
					underStaffingDetails.UnderstaffingTimes.Select(t => t.ToShortTimeString(culture)).Take(5)); //5 = max items
				errorMessageBuilder.AppendLine(string.Format("{0}{1}{2}", understaffingHoursValidationError, insufficientHours,
					Environment.NewLine));
			return errorMessageBuilder.ToString();
		}

		private void sendDenyCommand(Guid personRequestId, string denyReason)
		{
			var command = new DenyRequestCommand()
			{
				PersonRequestId = personRequestId,
				DenyReason = denyReason,
				IsAlreadyAbsent = false
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages != null)
			{
				logger.Warn(command.ErrorMessages);
			}
		}

		private void sendApproveCommand(Guid personRequestId)
		{
			var command = new ApproveRequestCommand()
			{
				PersonRequestId = personRequestId,
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages != null)
			{
				logger.Warn(command.ErrorMessages);
			}
		}

	}
}
