using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class StaffingThresholdWithShrinkageValidator : IAbsenceRequestValidator
	{
		private const int maxUnderStaffingItemCount = 5;
		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

		public string InvalidReason
		{
			get { return "RequestDenyReasonSkillThreshold"; }
		}

		public string DisplayText
		{
			get { return Resources.IntradayWithShrinkage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private UnderstaffingDetails getUnderStaffingDays(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			InParameter.NotNull("SchedulingResultStateHolder", requiredForHandlingAbsenceRequest.SchedulingResultStateHolder);
			InParameter.NotNull("ResourceOptimizationHelper", requiredForHandlingAbsenceRequest.ResourceOptimizationHelper);

			var result = new UnderstaffingDetails();
			var personSkillProvider = new PersonSkillProvider();
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var localPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);
			var schedules = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[absenceRequest.Person].ScheduledDayCollection(localPeriod);

			foreach (var scheduleDay in schedules)
			{
				var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
				var skills = personSkillProvider.SkillsOnPersonDate(absenceRequest.Person, date);
				if (!IsSkillOpenForDateOnly(date, skills.Skills))
					continue;

				//As the resource calculation currently always being made from the viewpoint timezone, this is what we need here!
				var dayPeriod = scheduleDay.DateOnlyAsPeriod.Period();
				var datesToResourceCalculate = dayPeriod.ToDateOnlyPeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);

				var resCalcData = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.ToResourceOptimizationData(true, false);
				foreach (DateOnly dateOnly in datesToResourceCalculate.DayCollection())
				{
					requiredForHandlingAbsenceRequest.ResourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
				}

				var calculatedPeriod = datesToResourceCalculate.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
				var absenceLayers = scheduleDay.ProjectionService().CreateProjection().FilterLayers(absenceRequest.Absence);

				foreach (var absenceLayer in absenceLayers)
				{
					var sharedPeriod = calculatedPeriod.Intersection(absenceLayer.Period);
					if (!sharedPeriod.HasValue) continue;

					var sharedRequestPeriod = absenceRequest.Period.Intersection(sharedPeriod.Value);
					if (!sharedRequestPeriod.HasValue) continue;

					foreach (var skill in skills.Skills)
					{
						if (skill == null) continue;
						var skillStaffPeriodList = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, sharedRequestPeriod.Value);

						if (skillStaffPeriodList == null || skillStaffPeriodList.Count == 0)
						{
							continue;
						}

						var validatedUnderStaffingResult = validateUnderstaffing(skill, skillStaffPeriodList, timeZone, result);
						if (!validatedUnderStaffingResult.IsValid)
						{
							result.AddUnderstaffingDay(date);
						}

						var validatedSeriousUnderStaffingResult = validateSeriousUnderstaffing(skill, skillStaffPeriodList, timeZone, result);
						if (!validatedSeriousUnderStaffingResult.IsValid)
						{
							result.AddSeriousUnderstaffingDay(date);
						}
					}
				}
			}

			return result;
		}

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var culture = absenceRequest.Person.PermissionInformation.Culture();
			var uiCulture = absenceRequest.Person.PermissionInformation.UICulture();
			var numberOfRequestedDays = absenceRequest.Period.ToDateOnlyPeriod(timeZone).DayCollection().Count;
			var underStaffingResultDict = getUnderStaffingDays(absenceRequest, requiredForHandlingAbsenceRequest);

			if (underStaffingResultDict.IsNotUnderstaffed())
			{
				return new ValidatedRequest { IsValid = true, ValidationErrors = string.Empty };
			}
			string validationError = numberOfRequestedDays > 1
												  ? getUnderStaffingDateString(underStaffingResultDict, culture, uiCulture)
												  : getUnderStaffingHourString(underStaffingResultDict, culture, uiCulture, absenceRequest.Period.StartDateTimeLocal(timeZone));
			return new ValidatedRequest
			{
				IsValid = false,
				ValidationErrors = validationError
			};
		}

		private string getUnderStaffingDateString(UnderstaffingDetails underStaffing, CultureInfo culture, CultureInfo uiCulture)
		{
			var errorMessageBuilder = new StringBuilder();

			if (underStaffing.UnderstaffingDays.Any())
			{
				var underStaffingValidationError = Resources.ResourceManager.GetString("InsufficientStaffingDays", uiCulture);
				var inSufficientDates = string.Join(", ",
					underStaffing.UnderstaffingDays.Select(d => d.ToShortDateString(culture)).Take(maxUnderStaffingItemCount));
				errorMessageBuilder.AppendLine(string.Format("{0}{1}{2}", underStaffingValidationError, inSufficientDates,
					Environment.NewLine));
			}

			if (underStaffing.SeriousUnderstaffingDays.Any())
			{
				var criticalUnderStaffingValidationError = Resources.ResourceManager.GetString("SeriousUnderstaffing", uiCulture);
				var criticalUnderStaffingDates = string.Join(", ",
					underStaffing.SeriousUnderstaffingDays.Select(d => d.ToShortDateString(culture)).Take(maxUnderStaffingItemCount));
				errorMessageBuilder.AppendLine(string.Format("{0}{1}{2}", criticalUnderStaffingValidationError,
					criticalUnderStaffingDates, Environment.NewLine));
			}

			return errorMessageBuilder.ToString();
		}

		private string getUnderStaffingHourString(UnderstaffingDetails underStaffing, CultureInfo culture, CultureInfo uiCulture, DateTime dateTime)
		{
			var errorMessageBuilder = new StringBuilder();
			if (underStaffing.UnderstaffingTimes.Any())
			{
				var understaffingHoursValidationError = string.Format(uiCulture,
					Resources.ResourceManager.GetString("InsufficientStaffingHours", uiCulture),
					dateTime.ToString("d", culture));
				var insufficientHours = string.Join(", ",
					underStaffing.UnderstaffingTimes.Select(t => t.ToShortTimeString(culture)).Take(maxUnderStaffingItemCount));
				errorMessageBuilder.AppendLine(string.Format("{0}{1}{2}", understaffingHoursValidationError, insufficientHours,
					Environment.NewLine));
			}

			if (underStaffing.SeriousUnderstaffingTimes.Any())
			{
				var criticalUnderstaffingHoursValidationError = string.Format(uiCulture,
					Resources.ResourceManager.GetString("SeriousUnderStaffingHours", uiCulture),
					dateTime.ToString("d", culture));
				var criticalUnderstaffingHours = string.Join(", ",
					underStaffing.SeriousUnderstaffingTimes.Select(t => t.ToShortTimeString(culture))
						.Take(maxUnderStaffingItemCount));
				errorMessageBuilder.AppendLine(string.Format("{0}{1}{2}", criticalUnderstaffingHoursValidationError,
					criticalUnderstaffingHours, Environment.NewLine));
			}

			return errorMessageBuilder.ToString();
		}

		private static IValidatedRequest validateSeriousUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZone, UnderstaffingDetails result)
		{
			var intervalHasSeriousUnderstaffing = new IntervalShrinkageHasSeriousUnderstaffing(skill);
			var seriousUnderStaffPeriods = skillStaffPeriodList.Where(intervalHasSeriousUnderstaffing.IsSatisfiedBy).ToArray();

			if (seriousUnderStaffPeriods.Any())
			{
				seriousUnderStaffPeriods.Select(s => s.Period.TimePeriod(timeZone)).ForEach(result.AddSeriousUnderstaffingTime);
				return new ValidatedRequest { IsValid = false };
			}

			return new ValidatedRequest { IsValid = true };
		}

		private static IValidatedRequest validateUnderstaffing(ISkill skill, IList<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZone, UnderstaffingDetails result)
		{
			var validatedRequest = new ValidatedRequest();
			var intervalHasUnderstaffing = new IntervalShrinkageHasUnderstaffing(skill);
			var exceededUnderstaffingList = skillStaffPeriodList.Where(intervalHasUnderstaffing.IsSatisfiedBy).ToList();
			var exceededRate = exceededUnderstaffingList.Sum(t => t.Period.ElapsedTime().TotalMinutes) / skillStaffPeriodList.Sum(t => t.Period.ElapsedTime().TotalMinutes);
			var isWithinUnderStaffingLimit = (1 - exceededRate) >= skill.StaffingThresholds.UnderstaffingFor.Value;

			validatedRequest.IsValid = isWithinUnderStaffingLimit;
			if (!isWithinUnderStaffingLimit)
				exceededUnderstaffingList.Select(s => s.Period.TimePeriod(timeZone)).ForEach(result.AddUnderstaffingTime);

			return validatedRequest;
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new StaffingThresholdWithShrinkageValidator();
		}

		public override bool Equals(object obj)
		{
			var validator = obj as StaffingThresholdWithShrinkageValidator;
			return validator != null;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				result = (result * 397) ^ (BudgetGroupHeadCountSpecification != null ? BudgetGroupHeadCountSpecification.GetHashCode() : 0);
				return result;
			}
		}

		protected static bool IsSkillOpenForDateOnly(DateOnly date, IEnumerable<ISkill> skills)
		{
			return skills.Any(s => s.WorkloadCollection.Any(w => w.TemplateWeekCollection.Any(t => t.Key == (int)date.DayOfWeek && t.Value.OpenForWork.IsOpen)));
		}
	}
}