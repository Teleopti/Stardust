using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayRequestProcessor : IIntradayRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayRequestProcessor));

		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly ResourceAllocator _resourceAllocator;
		private readonly IIntradayRequestWithinOpenHourValidator _intradayRequestWithinOpenHourValidator;
		private readonly IScheduleForecastSkillReadModelValidator _scheduleForecastSkillReadModelValidator;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public IntradayRequestProcessor(ICommandDispatcher commandDispatcher, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, ResourceAllocator resourceAllocator, 
			IIntradayRequestWithinOpenHourValidator intradayRequestWithinOpenHourValidator, IScheduleForecastSkillReadModelValidator scheduleForecastSkillReadModelValidator, ICurrentBusinessUnit currentBusinessUnit)
		{
			_commandDispatcher = commandDispatcher;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_resourceAllocator = resourceAllocator;
			_intradayRequestWithinOpenHourValidator = intradayRequestWithinOpenHourValidator;
			_scheduleForecastSkillReadModelValidator = scheduleForecastSkillReadModelValidator;
			_currentBusinessUnit = currentBusinessUnit;
		}

		public void Process(IPersonRequest personRequest, DateTime startTime)
		{
			if (!_scheduleForecastSkillReadModelValidator.Validate(_currentBusinessUnit.Current().Id.GetValueOrDefault()))
			{
				sendDenyCommand(personRequest.Id.GetValueOrDefault(), Resources.DenyDueToTechnicalProblems + " Old processor.");
				return;
			}
			var personPeriod = personRequest.Person.Period(new DateOnly(startTime));
			var cascadingPersonSkills = personPeriod.CascadingSkills();
			var lowestIndex = cascadingPersonSkills.Min(x => x.Skill.CascadingIndex);
			var periods =
				personRequest.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod(personRequest.Request as IAbsenceRequest);
			var useShrinkage = periods.GetSelectedValidatorList().Any(x => x is StaffingThresholdWithShrinkageValidator);
			var primaryAndUnSortedSkills =
				personPeriod
					.PersonSkillCollection.Where(x => (x.Skill.CascadingIndex == lowestIndex) || !x.Skill.CascadingIndex.HasValue);
			if (!areAllSkillsClosed(primaryAndUnSortedSkills, personRequest.Request.Period))
			{
				foreach (var skill in primaryAndUnSortedSkills)
				{
					var skillOpenHourStatus = _intradayRequestWithinOpenHourValidator.Validate(skill.Skill,personRequest.Request.Period);
					if (skillOpenHourStatus == OpenHourStatus.MissingOpenHour || skillOpenHourStatus == OpenHourStatus.OutsideOpenHour) continue;

					var skillStaffingIntervals =
						_scheduleForecastSkillReadModelRepository.ReadMergedStaffingAndChanges(skill.Skill.Id.GetValueOrDefault(),
							personRequest.Request.Period);
					if (!skillStaffingIntervals.Any())
					{
						sendDenyCommand(personRequest.Id.GetValueOrDefault(),
							string.Format(Resources.StaffingInformationMissing, skill.Skill.Name));
						return;
					}
					if (hasZeroForecast(skillStaffingIntervals.ToList(),useShrinkage)) continue;

					var underStaffingDetails = getUnderStaffedPeriods(skillStaffingIntervals,
						personRequest.Person.PermissionInformation.DefaultTimeZone(), useShrinkage);
					if (underStaffingDetails.UnderstaffingTimes.Any())
					{
						var denyReason = GetUnderStaffingHourString(underStaffingDetails, personRequest);
						sendDenyCommand(personRequest.Id.GetValueOrDefault(), denyReason);
						return;
					}
				}
				if (sendApproveCommand(personRequest.Id.GetValueOrDefault()))
					updateResources(personRequest, startTime);
			}
			else
			{
				//absence on a day which has no opening hours or on an period where all skills are closed
				//approve and no need to update resource
				sendApproveCommand(personRequest.Id.GetValueOrDefault());
			}

		}

		private bool hasZeroForecast(List<SkillStaffingInterval> staffingIntervals, bool useShrinkage)
		{
			return staffingIntervals.Count(x => x.GetForecast(useShrinkage) > 0) == 0;
		}


		private bool areAllSkillsClosed(IEnumerable<IPersonSkill> primaryAndUnSortedSkills, DateTimePeriod requestPeriod)
		{
			foreach (var personSkill in primaryAndUnSortedSkills)
			{
				if (_intradayRequestWithinOpenHourValidator.Validate(personSkill.Skill, requestPeriod) == OpenHourStatus.WithinOpenHour)
					return false;
			}
			return true;
		}

		private void updateResources(IPersonRequest personRequest, DateTime startDate)
		{
			var staffingIntervalChanges = _resourceAllocator.AllocateResource(personRequest, startDate);
			foreach (var staffingIntervalChange in staffingIntervalChanges)
			{
				_scheduleForecastSkillReadModelRepository.PersistChange(staffingIntervalChange);
			}
		}

		private UnderstaffingDetails getUnderStaffedPeriods(IEnumerable<SkillStaffingInterval> skillStaffingIntervals, TimeZoneInfo defaultTimeZone, bool useShrinkage)
		{
			var result = new UnderstaffingDetails();
			foreach (var interval in skillStaffingIntervals.Where(x => x.StaffingLevel < x.GetForecast(useShrinkage) + 1))
			{
				var startDateTime = TimeZoneHelper.ConvertFromUtc(interval.StartDateTime, defaultTimeZone);
				var endDateTime = TimeZoneHelper.ConvertFromUtc(interval.EndDateTime, defaultTimeZone);
				var startTimeSpan = startDateTime.TimeOfDay;
				var endTimeSpan = endDateTime.Subtract(startDateTime.Date);
					
				result.AddUnderstaffingTime(new TimePeriod(startTimeSpan, endTimeSpan));
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
				underStaffingDetails.UnderstaffingTimes.Select(t => t.ToShortTimeString(culture)).Take(4)); //4 = max items, NEEDS TO BE MAX 4 WHEN DENYREASON IS NVARCHAR(300)
			errorMessageBuilder.AppendLine(string.Format("{0}{1}{2}", understaffingHoursValidationError, insufficientHours,
				Environment.NewLine));
			return errorMessageBuilder.ToString();
		}

		private bool sendDenyCommand(Guid personRequestId, string denyReason)
		{
			var command = new DenyRequestCommand()
			{
				PersonRequestId = personRequestId,
				DenyReason = denyReason
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}

			return !command.ErrorMessages.Any();
		}

		private bool sendApproveCommand(Guid personRequestId)
		{
			var command = new ApproveRequestCommand()
			{
				PersonRequestId = personRequestId,
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}

			return !command.ErrorMessages.Any();
		}

	}
}
