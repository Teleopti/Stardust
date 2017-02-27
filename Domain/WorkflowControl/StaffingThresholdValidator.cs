using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{

	public class StaffingThresholdValidatorCascadingSkills : StaffingThresholdValidator
	{
		

		public override IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequest.Person.PermissionInformation.Culture();
			var uiCulture = absenceRequest.Person.PermissionInformation.UICulture();
			var numberOfRequestedDays = absenceRequest.Period.ToDateOnlyPeriod(timeZone).DayCount();

			var underStaffingResultDict = GetUnderStaffingDays(absenceRequest, requiredForHandlingAbsenceRequest, new CascadingPersonSkillProvider());

			if (underStaffingResultDict.IsNotUnderstaffed())
			{
				return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
			}
			string validationError = numberOfRequestedDays > 1
				? GetUnderStaffingDateString(underStaffingResultDict, culture, uiCulture)
				: GetUnderStaffingHourString(underStaffingResultDict, culture, uiCulture, timeZone, absenceRequest.Period.StartDateTimeLocal(timeZone));
			return new ValidatedRequest
			{
				IsValid = false,
				ValidationErrors = validationError
			};
		}

		public override IAbsenceRequestValidator CreateInstance()
		{
			return new StaffingThresholdValidatorCascadingSkills();
		}
	}

	public class StaffingThresholdValidator :  IAbsenceRequestValidator
	{
		private static readonly ILog requestsLogger = LogManager.GetLogger("Teleopti.Requests");
		private const int maxUnderStaffingItemCount = 4;// bug #40906 needs to be max 4 
		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

		public IValidatedRequest ValidateLight(IAbsenceRequest absenceRequest, IEnumerable<IValidatePeriod> validatePeriods)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequest.Person.PermissionInformation.Culture();
			var uiCulture = absenceRequest.Person.PermissionInformation.UICulture();

			var underStaffingResultDict = GetUnderStaffingDaysLight(absenceRequest, validatePeriods);

			if (underStaffingResultDict.IsNotUnderstaffed())
			{
				return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
			}
			string validationError = GetUnderStaffingHourString(underStaffingResultDict, culture, uiCulture, timeZone, absenceRequest.Period.StartDateTimeLocal(timeZone));
			return new ValidatedRequest
			{
				IsValid = false,
				ValidationErrors = validationError
			};
		}

		public virtual IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequest.Person.PermissionInformation.Culture();
			var uiCulture = absenceRequest.Person.PermissionInformation.UICulture();
			var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);
			var numberOfRequestedDays = dateOnlyPeriod.DayCount();
			
			var underStaffingResultDict = GetUnderStaffingDays(absenceRequest, requiredForHandlingAbsenceRequest, new PersonSkillProvider());

			if (underStaffingResultDict.IsNotUnderstaffed())
			{
				return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
			}
			string validationError = numberOfRequestedDays > 1
				? GetUnderStaffingDateString(underStaffingResultDict, culture, uiCulture)
				: GetUnderStaffingHourString(underStaffingResultDict, culture, uiCulture, timeZone, absenceRequest.Period.StartDateTimeLocal(timeZone));
			return new ValidatedRequest
			{
				IsValid = false,
				ValidationErrors = validationError
			};
		}

		private Specification<IValidatePeriod> getIntervalsForUnderstaffing(ISkill skill)
		{
			return new IntervalHasUnderstaffing(skill);
		}

		private Specification<IValidatePeriod> getIntervalsForSeriousUnderstaffing(ISkill skill)
		{
			return new IntervalHasSeriousUnderstaffing(skill);
		}

		public string GetUnderStaffingDateString(UnderstaffingDetails underStaffing, CultureInfo culture, CultureInfo uiCulture)
		{
			var errorMessageBuilder = new StringBuilder();

			if (underStaffing.UnderstaffingDays.Any())
			{
				var underStaffingValidationError = Resources.ResourceManager.GetString("InsufficientStaffingDays", uiCulture);
				var inSufficientDates = string.Join(", ",
					underStaffing.UnderstaffingDays.Select(d => d.ToShortDateString(culture)).Take(maxUnderStaffingItemCount));
				errorMessageBuilder.AppendLine($"{underStaffingValidationError}{inSufficientDates}{Environment.NewLine}");
			}

			if (underStaffing.SeriousUnderstaffingDays.Any())
			{
				var criticalUnderStaffingValidationError = Resources.ResourceManager.GetString("SeriousUnderstaffing", uiCulture);
				var criticalUnderStaffingDates = string.Join(", ",
					underStaffing.SeriousUnderstaffingDays.Select(d => d.ToShortDateString(culture)).Take(maxUnderStaffingItemCount));
				errorMessageBuilder.AppendLine(
					$"{criticalUnderStaffingValidationError}{criticalUnderStaffingDates}{Environment.NewLine}");
			}

			return errorMessageBuilder.ToString();
		}
		
		public string GetUnderStaffingHourString(UnderstaffingDetails underStaffing, CultureInfo culture, CultureInfo uiCulture, TimeZoneInfo timeZone, DateTime dateTime)
		{
			var errorMessageBuilder = new StringBuilder();
			if (underStaffing.UnderstaffingTimes.Any())
			{
				var understaffingHoursValidationError = string.Format(uiCulture,
					Resources.ResourceManager.GetString("InsufficientStaffingHours", uiCulture),
					dateTime.ToString("d", culture));
				var insufficientHours = string.Join(", ",
					underStaffing.UnderstaffingTimes.Select(t => t.ToShortTimeString(culture)).Take(maxUnderStaffingItemCount));
				errorMessageBuilder.AppendLine($"{understaffingHoursValidationError}{insufficientHours}{Environment.NewLine}");
			}

			if (underStaffing.SeriousUnderstaffingTimes.Any())
			{
				var criticalUnderstaffingHoursValidationError = string.Format(uiCulture,
					Resources.ResourceManager.GetString("SeriousUnderStaffingHours", uiCulture),
					dateTime.ToString("d", culture));
				var criticalUnderstaffingHours = string.Join(", ",
					underStaffing.SeriousUnderstaffingTimes.Select(t => t.ToShortTimeString(culture))
						.Take(maxUnderStaffingItemCount));
				errorMessageBuilder.AppendLine(
					$"{criticalUnderstaffingHoursValidationError}{criticalUnderstaffingHours}{Environment.NewLine}");
			}

			return errorMessageBuilder.ToString();
		}

		public UnderstaffingDetails GetUnderStaffingDays(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, PersonSkillProvider personSkillProvider)
		{
			InParameter.NotNull(nameof(requiredForHandlingAbsenceRequest.SchedulingResultStateHolder), requiredForHandlingAbsenceRequest.SchedulingResultStateHolder);
			InParameter.NotNull(nameof(requiredForHandlingAbsenceRequest.ResourceOptimizationHelper), requiredForHandlingAbsenceRequest.ResourceOptimizationHelper);

			var result = new UnderstaffingDetails();
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var localPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);
			var schedules = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[absenceRequest.Person].ScheduledDayCollection(localPeriod);
			var skills = personSkillProvider.SkillsOnPersonDate(absenceRequest.Person, localPeriod.StartDate);

			if (!isSkillOpenForDateOnly(localPeriod.StartDate, skills.Skills))
				return result;

			var datesToResourceCalculate = absenceRequest.Period.ToDateOnlyPeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
			var calculatedPeriod = absenceRequest.Period;

			var resCalcData = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.ToResourceOptimizationData(true, false);
			foreach (var dateOnly in datesToResourceCalculate.DayCollection())
			{
				requiredForHandlingAbsenceRequest.ResourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
			}

			var absenceLayerCollection = new List<IVisualLayer>();
			foreach (var scheduleDay in schedules)
			{
				var absenceLayers = scheduleDay.ProjectionService().CreateProjection().FilterLayers(absenceRequest.Absence);
				absenceLayerCollection.AddRange(absenceLayers);
			}

			foreach (var absenceLayer in absenceLayerCollection)
			{
				var sharedPeriod = calculatedPeriod.Intersection(absenceLayer.Period);
				if (!sharedPeriod.HasValue) continue;

				var sharedRequestPeriod = absenceRequest.Period.Intersection(sharedPeriod.Value);
				if (!sharedRequestPeriod.HasValue) continue;

				foreach (var skill in skills.Skills)
				{
					if (skill == null) continue;
					var skillStaffPeriodList = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, sharedRequestPeriod.Value)?.Cast<IValidatePeriod>().ToList();

					if (skillStaffPeriodList == null || skillStaffPeriodList.Count == 0)
					{
						continue;
					}

					var validatedUnderStaffingResult = ValidateUnderstaffing(skill, skillStaffPeriodList, timeZone, result);
					if (!validatedUnderStaffingResult.IsValid)
					{
						result.AddUnderstaffingDay(localPeriod.StartDate);
					}

					var validatedSeriousUnderStaffingResult = ValidateSeriousUnderstaffing(skill, skillStaffPeriodList, timeZone, result);
					if (!validatedSeriousUnderStaffingResult.IsValid)
					{
						result.AddSeriousUnderstaffingDay(localPeriod.StartDate);
					}
				}
			}

			return result;
		}

		public UnderstaffingDetails GetUnderStaffingDaysLight(IAbsenceRequest absenceRequest, IEnumerable<IValidatePeriod> validatePeriods)
		{
			var result = new UnderstaffingDetails();
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var date = new DateOnly(absenceRequest.Period.StartDateTimeLocal(timeZone));
			var personSkillProvider = new PersonSkillProvider();
			var skills = personSkillProvider.SkillsOnPersonDate(absenceRequest.Person, date);

			if (!isSkillOpenForDateOnly(date, skills.Skills))
				return result;

			foreach (var skill in skills.Skills)
			{
				if (skill == null) continue;
				if (validatePeriods.IsEmpty()) continue;

				var validatedUnderStaffingResult = ValidateUnderstaffing(skill, validatePeriods.Where(x => ((SkillStaffingInterval)x).SkillId == skill.Id.GetValueOrDefault()), timeZone, result);
				if (!validatedUnderStaffingResult.IsValid)
				{
					requestsLogger.Debug($"Understaffed on skill: {skill.Name}, Intervals: {string.Join(",", result.UnderstaffingTimes.Select(x => x.StartTime))}");
					result.AddUnderstaffingDay(date);
				}
			}

			return result;
		}

		public IValidatedRequest ValidateSeriousUnderstaffing(ISkill skill, IEnumerable<IValidatePeriod> skillStaffPeriodList, TimeZoneInfo timeZone, UnderstaffingDetails result)
		{
			if (skillStaffPeriodList == null) throw new ArgumentNullException(nameof(skillStaffPeriodList));
			var intervalHasSeriousUnderstaffing = getIntervalsForSeriousUnderstaffing(skill);
			var seriousUnderStaffPeriods = skillStaffPeriodList.Where(intervalHasSeriousUnderstaffing.IsSatisfiedBy).ToArray();

			if (seriousUnderStaffPeriods.Any())
			{
				seriousUnderStaffPeriods.Select(s => s.DateTimePeriod.TimePeriod(timeZone)).ForEach(result.AddSeriousUnderstaffingTime);
				return new ValidatedRequest { IsValid = false };
			}

			return new ValidatedRequest { IsValid = true };
		}

		public IValidatedRequest ValidateUnderstaffing(ISkill skill, IEnumerable<IValidatePeriod> skillStaffPeriodList, TimeZoneInfo timeZone, UnderstaffingDetails result)
		{
			if (skillStaffPeriodList == null) throw new ArgumentNullException(nameof(skillStaffPeriodList));

			var validatedRequest = new ValidatedRequest();
			var intervalHasUnderstaffing = getIntervalsForUnderstaffing(skill);
			var exceededUnderstaffingList = skillStaffPeriodList.Where(intervalHasUnderstaffing.IsSatisfiedBy).ToArray();
			var exceededRate = exceededUnderstaffingList.Sum(t => t.DateTimePeriod.ElapsedTime().TotalMinutes) / skillStaffPeriodList.Sum(t => t.DateTimePeriod.ElapsedTime().TotalMinutes);
			var isWithinUnderStaffingLimit = 1d - (double.IsNaN(exceededRate) ? 0 : exceededRate) >= skill.StaffingThresholds.UnderstaffingFor.Value;

			validatedRequest.IsValid = isWithinUnderStaffingLimit;
			if (!isWithinUnderStaffingLimit)
				exceededUnderstaffingList.Select(s => s.DateTimePeriod.TimePeriod(timeZone)).ForEach(result.AddUnderstaffingTime);

			return validatedRequest;
		}


		private static bool isSkillOpenForDateOnly(DateOnly date, IEnumerable<ISkill> skills)
		{
			return skills.Any(s => s.WorkloadCollection.Any(w => w.TemplateWeekCollection.Any(t => t.Key == (int)date.DayOfWeek && t.Value.OpenForWork.IsOpen)));
		}

		public virtual IAbsenceRequestValidator CreateInstance()
		{
			return new StaffingThresholdValidator();
		}

		public string InvalidReason => "RequestDenyReasonSkillThreshold";

		public virtual string DisplayText => Resources.Intraday;

		public override bool Equals(object obj)
		{
			var validator = obj as StaffingThresholdValidator;
			return validator != null;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				result = (result * 397) ^ (BudgetGroupHeadCountSpecification?.GetHashCode() ?? 0);
				return result;
			}
		}
	}
}