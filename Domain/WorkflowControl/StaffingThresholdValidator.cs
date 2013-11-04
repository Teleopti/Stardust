using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class StaffingThresholdValidator : IAbsenceRequestValidator
    {
        public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

        public string InvalidReason
        {
            get { return "RequestDenyReasonSkillThreshold"; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.Intraday; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private UnderstaffingDetails getUnderStaffingDays(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            InParameter.NotNull("SchedulingResultStateHolder", requiredForHandlingAbsenceRequest.SchedulingResultStateHolder);
            InParameter.NotNull("ResourceOptimizationHelper", requiredForHandlingAbsenceRequest.ResourceOptimizationHelper);

            var result = new UnderstaffingDetails();
            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var localPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);

            foreach (DateOnly dateTime in localPeriod.DayCollection())
            {
                //As the resource calculation currently always being made from the viewpoint timezone, this is what we need here!
                var dayPeriod = new DateOnlyPeriod(dateTime, dateTime).ToDateTimePeriod(timeZone);
                var datesToResourceCalculate = dayPeriod.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
                
                foreach (DateOnly dateOnly in datesToResourceCalculate.DayCollection())
                {
                    requiredForHandlingAbsenceRequest.ResourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
                }
                
                var calculatedPeriod = datesToResourceCalculate.ToDateTimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
                var scheduleDay = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[absenceRequest.Person].ScheduledDay(dateTime);
                var absenceLayers = scheduleDay.ProjectionService().CreateProjection().FilterLayers(absenceRequest.Absence);

                foreach (var absenceLayer in absenceLayers)
                {
                    var sharedPeriod = calculatedPeriod.Intersection(absenceLayer.Period);
                    if (sharedPeriod.HasValue && absenceRequest.Period.Contains(sharedPeriod.Value))
                    {
                        foreach (var skill in requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Skills)
                        {
                            var skillStaffPeriodList = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, sharedPeriod.Value);
                            if (skillStaffPeriodList == null || skillStaffPeriodList.Count == 0)
                            {
                                return result;
                            }

                            if (skill == null) continue;

                            var validatedUnderStaffingResult = ValidateUnderstaffing(skill, skillStaffPeriodList, absenceRequest.Person, result);

                            if (!validatedUnderStaffingResult.IsValid)
                            {
                                result.AddUnderstaffingDay(dateTime);
                            }

                            var validatedSeriousUnderStaffingResult = ValidateSeriousUnderstaffing(skill,skillStaffPeriodList, absenceRequest.Person, result);

                            if (!validatedSeriousUnderStaffingResult.IsValid)
                            {
                                result.AddSeriousUnderstaffingDay(dateTime);
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private bool isUnderStaffing(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            InParameter.NotNull("SchedulingResultStateHolder", requiredForHandlingAbsenceRequest.SchedulingResultStateHolder);
            InParameter.NotNull("ResourceOptimizationHelper", requiredForHandlingAbsenceRequest.ResourceOptimizationHelper);
            
            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var localPeriod = absenceRequest.Period.ToDateOnlyPeriod(timeZone);

            foreach (DateOnly dateTime in localPeriod.DayCollection())
            {
                //As the resource calculation currently always being made from the viewpoint timezone, this is what we need here!
                var dayPeriod = new DateOnlyPeriod(dateTime, dateTime).ToDateTimePeriod(timeZone);
                var datesToResourceCalculate = dayPeriod.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
                foreach (DateOnly dateOnly in datesToResourceCalculate.DayCollection())
                {
                    requiredForHandlingAbsenceRequest.ResourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
                }
                var calculatedPeriod = datesToResourceCalculate.ToDateTimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
                var scheduleDay = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Schedules[absenceRequest.Person].ScheduledDay(dateTime);
                var absenceLayers = scheduleDay.ProjectionService().CreateProjection().FilterLayers(absenceRequest.Absence);

                var skillCollection = absenceRequest.Person.Period(dateTime).PersonSkillCollection.Select(p =>p.Skill);

                if (!IsSkillOpenForDateOnly(dateTime, skillCollection))
                    continue; 

                foreach (var absenceLayer in absenceLayers)
                {
                    var sharedPeriod = calculatedPeriod.Intersection(absenceLayer.Period);
                    if (sharedPeriod.HasValue && absenceRequest.Period.Contains(sharedPeriod.Value))
                    {
                        foreach (var skill in requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Skills)
                        {
                            var skillStaffPeriodList = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, sharedPeriod.Value);
                            if (skillStaffPeriodList == null || skillStaffPeriodList.Count == 0)
                                return true;
                            if (skill == null) continue;

                            var validatedUnderStaffingResult = ValidateUnderstaffing(skill, skillStaffPeriodList, absenceRequest.Person, new UnderstaffingDetails());
                            
                            if (!validatedUnderStaffingResult.IsValid)
                            {
                                return false;
                            }

                            var validatedSeriousUnderStaffingResult = ValidateSeriousUnderstaffing(skill, skillStaffPeriodList, absenceRequest.Person, new UnderstaffingDetails());
                            
                            if (!validatedSeriousUnderStaffingResult.IsValid)
                            {
                               return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            var isUnderStaffed = isUnderStaffing(absenceRequest, requiredForHandlingAbsenceRequest);

            if (!isUnderStaffed)
            {
                var result = GetValidationErrors(absenceRequest, requiredForHandlingAbsenceRequest);
                return new ValidatedRequest { IsValid = false, ValidationErrors = result.ValidationErrors};
            }

            return new ValidatedRequest
                                        {
                                            IsValid = true, 
                                            ValidationErrors = string.Empty
                                        };
        }

        public IValidatedRequest GetValidationErrors(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var culture = absenceRequest.Person.PermissionInformation.Culture();
            var uiCulture = absenceRequest.Person.PermissionInformation.UICulture();
            var numberOfRequestedDays = absenceRequest.Period.ToDateOnlyPeriod(timeZone).DayCollection().Count;

            if (numberOfRequestedDays > 1)
            {
                var underStaffingResultDict = getUnderStaffingDays(absenceRequest, requiredForHandlingAbsenceRequest);
                var underStaffingDateValidationError = GetUnderStaffingDateString(underStaffingResultDict, culture, uiCulture);

                return new ValidatedRequest
                {
                    IsValid = false,
                    ValidationErrors = underStaffingDateValidationError
                };
            }
            else
            {
               var underStaffingResultDict = getUnderStaffingDays(absenceRequest, requiredForHandlingAbsenceRequest);
                var underStaffingHourValidationError = GetUnderStaffingHourString(underStaffingResultDict, culture, uiCulture,
                                                                                  timeZone,
                                                                                  absenceRequest.Period
                                                                                                .StartDateTimeLocal(
                                                                                                    timeZone));


                
                return new ValidatedRequest
                {
                    IsValid = false,
                    ValidationErrors = underStaffingHourValidationError
                };
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "underStaffing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public string GetUnderStaffingDateString(UnderstaffingDetails underStaffing, CultureInfo culture, CultureInfo uiCulture)
        {
            var inSufficientDates = string.Join(", ", underStaffing.UnderstaffingDays.Select(d => d.ToShortDateString(culture)).Take(5));
            var criticalUnderStaffingDates = string.Join(", ", underStaffing.SeriousUnderstaffingDays.Select(d => d.ToShortDateString(culture)).Take(5));
            var underStaffingValidationError = UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingDays", uiCulture);
            var criticalUnderStaffingValidationError = UserTexts.Resources.ResourceManager.GetString("SeriousUnderstaffing", uiCulture);

            string validationError = string.Empty;
            if (!string.IsNullOrEmpty(inSufficientDates))
                validationError += underStaffingValidationError + inSufficientDates + Environment.NewLine;

            if (!string.IsNullOrEmpty(criticalUnderStaffingDates))
                validationError += criticalUnderStaffingValidationError + criticalUnderStaffingDates + Environment.NewLine;
            
            return validationError;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeZone"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "underStaffing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public string GetUnderStaffingHourString(UnderstaffingDetails underStaffing, CultureInfo culture, CultureInfo uiCulture, TimeZoneInfo timeZone, DateTime dateTime )
        {
            var validationError = string.Empty;
            var understaffingHoursValidationError = string.Format(uiCulture, UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingHours", uiCulture),
                                                                    dateTime.ToString("d", culture));
            var criticalUnderstaffingHoursValidationError = string.Format(uiCulture, UserTexts.Resources.ResourceManager.GetString("SeriousUnderStaffingHours", uiCulture),
                              dateTime.ToString("d", culture));

            var insufficientHours = string.Join(", ", underStaffing.UnderstaffingTimes.Select(t => t.ToShortTimeString(culture)).Take(5));
            if (!string.IsNullOrEmpty(insufficientHours))
            {
                validationError += understaffingHoursValidationError + insufficientHours + Environment.NewLine;
            }

            var criticalUnderstaffingHours = string.Join(", ", underStaffing.SeriousUnderstaffingTimes.Select(t => t.ToShortTimeString(culture)).Take(5));
            if (!string.IsNullOrEmpty(criticalUnderstaffingHours))
            {
                validationError += criticalUnderstaffingHoursValidationError + criticalUnderstaffingHours + Environment.NewLine;
            }

            return validationError;
        }

        public static IValidatedRequest ValidateSeriousUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList, IPerson requestingAgent, UnderstaffingDetails result)
    	{
    		if (skillStaffPeriodList == null) throw new ArgumentNullException("skillStaffPeriodList");
    		var intervalHasSeriousUnderstaffing = new IntervalHasSeriousUnderstaffing(skill);
            var seriousUnderStaffPeriods = skillStaffPeriodList.Where(intervalHasSeriousUnderstaffing.IsSatisfiedBy).ToArray();
            var timeZone = requestingAgent.PermissionInformation.DefaultTimeZone();

            if (seriousUnderStaffPeriods.Any())
            {
                seriousUnderStaffPeriods.Select(s => s.Period.TimePeriod(timeZone)).ForEach(result.AddSeriousUnderstaffingTime);
                return new ValidatedRequest {IsValid = false};
            }

            return new ValidatedRequest {IsValid = true};
    	}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IValidatedRequest ValidateUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList, IPerson requestingAgent, UnderstaffingDetails result)
    	{
    		if (skillStaffPeriodList == null) throw new ArgumentNullException("skillStaffPeriodList");

		    var timeZone = requestingAgent.PermissionInformation.DefaultTimeZone();
		    var validatedRequest = new ValidatedRequest();
			var intervalHasUnderstaffing = new IntervalHasUnderstaffing(skill);
    		var exceededUnderstaffingList = skillStaffPeriodList.Where(intervalHasUnderstaffing.IsSatisfiedBy).ToList();
            var exceededRate = exceededUnderstaffingList.Sum(t => t.Period.ElapsedTime().TotalMinutes) / skillStaffPeriodList.Sum(t => t.Period.ElapsedTime().TotalMinutes);
            var isWithinUnderStaffingLimit = (1 - exceededRate) >= skill.StaffingThresholds.UnderstaffingFor.Value;

            validatedRequest.IsValid = isWithinUnderStaffingLimit;
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