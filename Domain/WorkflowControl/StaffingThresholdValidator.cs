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
    public class StaffingThresholdValidator : IAbsenceRequestValidator
	{
		private const int maxUnderStaffingItemCount = 5;
        public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

        public string InvalidReason
        {
            get { return "RequestDenyReasonSkillThreshold"; }
        }

        public string DisplayText
        {
            get { return Resources.Intraday; }
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
                
                foreach (DateOnly dateOnly in datesToResourceCalculate.DayCollection())
                {
                    requiredForHandlingAbsenceRequest.ResourceOptimizationHelper.ResourceCalculateDate(dateOnly, true);
                }
                
                var calculatedPeriod = datesToResourceCalculate.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
                var absenceLayers = scheduleDay.ProjectionService().CreateProjection().FilterLayers(absenceRequest.Absence);

                foreach (var absenceLayer in absenceLayers)
                {
                    var sharedPeriod = calculatedPeriod.Intersection(absenceLayer.Period);
	                if (!sharedPeriod.HasValue ) continue;

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

		                var validatedUnderStaffingResult = ValidateUnderstaffing(skill, skillStaffPeriodList, timeZone, result);
		                if (!validatedUnderStaffingResult.IsValid)
		                {
			                result.AddUnderstaffingDay(date);
		                }

		                var validatedSeriousUnderStaffingResult = ValidateSeriousUnderstaffing(skill,skillStaffPeriodList, timeZone, result);
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
                return new ValidatedRequest {IsValid = true, ValidationErrors = string.Empty};
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

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing"),
	     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "underStaffing"),
	     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"),
	     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
	    public string GetUnderStaffingDateString(UnderstaffingDetails underStaffing, CultureInfo culture, CultureInfo uiCulture)
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

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeZone"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "underStaffing"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public string GetUnderStaffingHourString(UnderstaffingDetails underStaffing, CultureInfo culture, CultureInfo uiCulture, TimeZoneInfo timeZone, DateTime dateTime )
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

        public static IValidatedRequest ValidateSeriousUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZone, UnderstaffingDetails result)
    	{
    		if (skillStaffPeriodList == null) throw new ArgumentNullException("skillStaffPeriodList");
    		var intervalHasSeriousUnderstaffing = new IntervalHasSeriousUnderstaffing(skill);
            var seriousUnderStaffPeriods = skillStaffPeriodList.Where(intervalHasSeriousUnderstaffing.IsSatisfiedBy).ToArray();
            
            if (seriousUnderStaffPeriods.Any())
            {
                seriousUnderStaffPeriods.Select(s => s.Period.TimePeriod(timeZone)).ForEach(result.AddSeriousUnderstaffingTime);
                return new ValidatedRequest {IsValid = false};
            }

            return new ValidatedRequest {IsValid = true};
    	}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IValidatedRequest ValidateUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList, TimeZoneInfo timeZone, UnderstaffingDetails result)
    	{
    		if (skillStaffPeriodList == null) throw new ArgumentNullException("skillStaffPeriodList");

		    var validatedRequest = new ValidatedRequest();
			var intervalHasUnderstaffing = new IntervalHasUnderstaffing(skill);
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
            return new StaffingThresholdValidator();
        }

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