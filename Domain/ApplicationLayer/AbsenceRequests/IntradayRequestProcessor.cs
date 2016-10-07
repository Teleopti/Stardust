using System;
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
		private readonly ResourceAllocator _resourceAllocator;

		public IntradayRequestProcessor(ICommandDispatcher commandDispatcher, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, ResourceAllocator resourceAllocator)
		{
			_commandDispatcher = commandDispatcher;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_resourceAllocator = resourceAllocator;
		}

		public void Process(IPersonRequest personRequest, DateTime startTime)
		{
			personRequest.Pending();
			var cascadingPersonSkills = personRequest.Person.Period(new DateOnly(startTime)).CascadingSkills();
			var lowestIndex = cascadingPersonSkills.Min(x => x.Skill.CascadingIndex);
			
			var primaryAndUnSortedSkills =
				personRequest.Person.Period(new DateOnly(startTime)).PersonSkillCollection.Where(x => (x.Skill.CascadingIndex == lowestIndex) || !x.Skill.CascadingIndex.HasValue );
			foreach (var skill in primaryAndUnSortedSkills)
			{
			    var skillStaffingIntervals = _scheduleForecastSkillReadModelRepository.ReadMergedStaffingAndChanges(skill.Skill.Id.GetValueOrDefault(), personRequest.Request.Period);
				var underStaffingDetails = getUnderStaffedPeriods(skillStaffingIntervals, personRequest.Person.PermissionInformation.DefaultTimeZone());
				if (underStaffingDetails.UnderstaffingTimes.Any())
				{
					var denyReason = GetUnderStaffingHourString(underStaffingDetails, personRequest);
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), denyReason);
					return;
				}
				//approve an absence request if its outside the opening hours 
			}
			updateResources(personRequest, startTime);
			sendApproveCommand(personRequest.Id.GetValueOrDefault());
			
		}

		private void updateResources(IPersonRequest personRequest, DateTime startDate)
		{
			var staffingIntervalChanges = _resourceAllocator.AllocateResource(personRequest, startDate);
			foreach (var staffingIntervalChange in staffingIntervalChanges)
			{
				_scheduleForecastSkillReadModelRepository.PersistChange(staffingIntervalChange);
			}
		}
		

		private UnderstaffingDetails getUnderStaffedPeriods(IEnumerable<SkillStaffingInterval> skillStaffingIntervals, TimeZoneInfo defaultTimeZone)
		{
			var result = new UnderstaffingDetails();
			foreach (var interval in skillStaffingIntervals.Where(x => x.StaffingLevel < x.ForecastWithShrinkage + 1))
			{
				var startDateTime = TimeZoneHelper.ConvertFromUtc(interval.StartDateTime, defaultTimeZone);
				var endDateTime = TimeZoneHelper.ConvertFromUtc(interval.EndDateTime, defaultTimeZone);
				result.AddUnderstaffingTime(new TimePeriod(startDateTime.Hour, startDateTime.Minute, endDateTime.Hour, endDateTime.Minute));
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
