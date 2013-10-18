using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class StaffingThresholdValidator : IAbsenceRequestValidator
    {
        public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }
        const string UnderStaffStr = "UnderStaffing";
        const string SeriousUnderStaffStr = "SeriousUnderStaffing";
        const string UnderStaffHoursStr = "UnderStaffingHours";
        const string SeriousUnderStaffHoursStr = "SeriousUnderStaffingHours";

        public string InvalidReason
        {
            get { return "RequestDenyReasonSkillThreshold"; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.Intraday; }
        }

        public IValidatedRequest Validate(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            var isUnderStaffed = isUnderStaffing(absenceRequest, requiredForHandlingAbsenceRequest);

            if (!isUnderStaffed)
            {
                var result = GetValidationErrors(absenceRequest, requiredForHandlingAbsenceRequest);
                return new ValidatedRequest { IsValid = false, ValidationErrors = result.ValidationErrors };
            }

            return new ValidatedRequest
            {
                IsValid = true,
                ValidationErrors = string.Empty
            };
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private static bool isUnderStaffing(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
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

                var skillCollection = absenceRequest.Person.Period(dateTime).PersonSkillCollection.Select(p => p.Skill);

                if (!IsSkillOpenForDateOnly(dateTime, skillCollection))
                    continue;

                foreach (var absenceLayer in absenceLayers)
                {
                    var sharedPeriod = calculatedPeriod.Intersection(absenceLayer.Period);
                    if (sharedPeriod.HasValue)
                    {
                        foreach (var skill in requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Skills)
                        {
                            var skillStaffPeriodList = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, sharedPeriod.Value);
                            if (skillStaffPeriodList == null || skillStaffPeriodList.Count == 0)
                                return true;
                            if (skill == null) continue;

                            var validatedUnderStaffingResult = ValidateUnderstaffing(skill, skillStaffPeriodList, absenceRequest.Person);

                            if (!validatedUnderStaffingResult.IsValid)
                            {
                                return false;
                            }

                            var validatedSeriousUnderStaffingResult = ValidateSeriousUnderstaffing(skill, skillStaffPeriodList, absenceRequest.Person);

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


        public IValidatedRequest GetValidationErrors(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var culture = absenceRequest.Person.PermissionInformation.Culture();
            var numberOfRequestedDays = absenceRequest.Period.ToDateOnlyPeriod(timeZone).DayCollection().Count;
            var underStaffingResultDict = getUnderStaffingDays(absenceRequest, requiredForHandlingAbsenceRequest);

            if (numberOfRequestedDays > 1)
            {
                var underStaffingDateValidationError = GetUnderStaffingDateString(underStaffingResultDict, culture);

                return new ValidatedRequest
                {
                    IsValid = false,
                    ValidationErrors = underStaffingDateValidationError
                };
            }

            var underStaffingHourValidationError = GetUnderStaffingHourString(underStaffingResultDict, culture,
                                                                              timeZone,
                                                                              absenceRequest.Period.StartDateTimeLocal(timeZone));

            return new ValidatedRequest
            {
                IsValid = false,
                ValidationErrors = underStaffingHourValidationError
            };
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private static IUnderStaffingData getUnderStaffingDays(IAbsenceRequest absenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
        {
            var underStaffingDaysDict = new Dictionary<string, IList<string>>();
            var underStaffingHoursDict = new Dictionary<string, IList<string>>();
            var underStaffDaysList = new List<string>();
            var seriousUnderStaffDaysList = new List<string>();
            var underStaffHours = string.Empty;
            var seriousUnderStaffHours = string.Empty;

            underStaffingDaysDict.Add(UnderStaffStr, new List<string>());
            underStaffingDaysDict.Add(SeriousUnderStaffStr, new List<string>());
            underStaffingHoursDict.Add(UnderStaffHoursStr, new List<string>());
            underStaffingHoursDict.Add(SeriousUnderStaffHoursStr, new List<string>());

            InParameter.NotNull("SchedulingResultStateHolder", requiredForHandlingAbsenceRequest.SchedulingResultStateHolder);
            InParameter.NotNull("ResourceOptimizationHelper", requiredForHandlingAbsenceRequest.ResourceOptimizationHelper);

            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var culture = absenceRequest.Person.PermissionInformation.Culture();
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
                    if (sharedPeriod.HasValue)
                    {
                        foreach (var skill in requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.Skills)
                        {
                            var skillStaffPeriodList = requiredForHandlingAbsenceRequest.SchedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, sharedPeriod.Value);
                            if (skillStaffPeriodList == null || skillStaffPeriodList.Count == 0)
                            {
                                return new UnderStaffingData
                                {
                                    UnderStaffingDates = underStaffingDaysDict,
                                    UnderStaffingHours = underStaffingHoursDict
                                };
                            }

                            if (skill == null) continue;

                            var validatedUnderStaffingResult = ValidateUnderstaffing(skill, skillStaffPeriodList, absenceRequest.Person);

                            if (!validatedUnderStaffingResult.IsValid)
                            {
                                underStaffDaysList.Add(string.Format(culture, dateTime.Date.ToString("d", culture)));
                                underStaffHours = validatedUnderStaffingResult.ValidationErrors;
                            }

                            var validatedSeriousUnderStaffingResult = ValidateSeriousUnderstaffing(skill,skillStaffPeriodList, absenceRequest.Person);

                            if (!validatedSeriousUnderStaffingResult.IsValid)
                            {
                                seriousUnderStaffDaysList.Add(string.Format(culture, dateTime.Date.ToString("d",culture)));
                                seriousUnderStaffHours = validatedSeriousUnderStaffingResult.ValidationErrors;
                            }
                        }
                    }
                }
            }
            
            underStaffingDaysDict[UnderStaffStr] = underStaffDaysList;
            underStaffingDaysDict[SeriousUnderStaffStr] = seriousUnderStaffDaysList;
            underStaffingHoursDict[UnderStaffHoursStr] = new List<string> {underStaffHours};
            underStaffingHoursDict[SeriousUnderStaffHoursStr] = new List<string> {seriousUnderStaffHours};

            return new UnderStaffingData
                {
                    UnderStaffingDates = underStaffingDaysDict,
                    UnderStaffingHours = underStaffingHoursDict
                };
        }

       
        public static IValidatedRequest ValidateSeriousUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList, IPerson requestingAgent)
        {
            if (skillStaffPeriodList == null) throw new ArgumentNullException("skillStaffPeriodList");
            var intervalHasSeriousUnderstaffing = new IntervalHasSeriousUnderstaffing(skill);
            var isSeriousUnderStaff = skillStaffPeriodList.Any(intervalHasSeriousUnderstaffing.IsSatisfiedBy);
            var seriousUnderStaffHours = string.Empty;

            if (isSeriousUnderStaff)
                seriousUnderStaffHours = intervalHasSeriousUnderstaffing.GetSeriousUnderstaffingHours(skillStaffPeriodList, requestingAgent);

            var validatedRequest = new ValidatedRequest();
            validatedRequest.IsValid = !isSeriousUnderStaff;
            validatedRequest.ValidationErrors = seriousUnderStaffHours;
            return validatedRequest;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static IValidatedRequest ValidateUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList, IPerson requestingAgent)
        {
            if (skillStaffPeriodList == null) throw new ArgumentNullException("skillStaffPeriodList");

            var timeZone = requestingAgent.PermissionInformation.DefaultTimeZone();
            var culture = requestingAgent.PermissionInformation.Culture();
            var validatedRequest = new ValidatedRequest();
            var intervalHasUnderstaffing = new IntervalHasUnderstaffing(skill);
            var exceededUnderstaffingList = skillStaffPeriodList.Where(intervalHasUnderstaffing.IsSatisfiedBy).ToList();
            var exceededRate = exceededUnderstaffingList.Sum(t => t.Period.ElapsedTime().TotalMinutes) / skillStaffPeriodList.Sum(t => t.Period.ElapsedTime().TotalMinutes);
            var isWithinUnderStaffingLimit = (1 - exceededRate) >= skill.StaffingThresholds.UnderstaffingFor.Value;
            var underStaffingHours = "";
            var count = 0;
            //get under staffing time interval.

            foreach (var underStaffingHoursInterval in exceededUnderstaffingList)
            {
                count++;
                if (count > 5)
                    break;

                var startTime = underStaffingHoursInterval.Period.StartDateTimeLocal(timeZone).ToString("t", culture);
                var endTime = underStaffingHoursInterval.Period.EndDateTimeLocal(timeZone).ToString("t", culture);
                underStaffingHours += startTime + "-" + endTime + ",";
            }

            if (underStaffingHours.Length > 1)
                underStaffingHours = underStaffingHours.Substring(0, underStaffingHours.Length - 1);

           
    
                
            validatedRequest.IsValid = isWithinUnderStaffingLimit;
            validatedRequest.ValidationErrors = underStaffingHours;
            return validatedRequest;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "underStaffing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public string GetUnderStaffingDateString(IUnderStaffingData underStaffing, CultureInfo culture )
        {
            var inSufficientDates = "";
            var criticalUnderStaffingDates = "";
            var validationError = "";
            var underStaffingDaysCount = 0;
            var criticalUnderStaffingDaysCount = 0;
            var underStaffingValidationError = UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingDays", culture);
            var criticalUnderStaffingValidationError = UserTexts.Resources.ResourceManager.GetString("SeriousUnderstaffing", culture);

            foreach (var insufficientStaffDay in underStaffing.UnderStaffingDates[UnderStaffStr])
            {
                underStaffingDaysCount++;
                if (underStaffingDaysCount > 5)
                    break;
                inSufficientDates += insufficientStaffDay + ",";
            }

            if (inSufficientDates.Length > 1)
            {
                inSufficientDates = inSufficientDates.Substring(0, inSufficientDates.Length - 1);
                validationError += underStaffingValidationError + inSufficientDates + Environment.NewLine;
            }

            foreach (var criticalUnderStaffingDay in underStaffing.UnderStaffingDates[SeriousUnderStaffStr])
            {
                criticalUnderStaffingDaysCount++;
                if (criticalUnderStaffingDaysCount > 5)
                    break;
                criticalUnderStaffingDates += criticalUnderStaffingDay + ",";
            }

            if (criticalUnderStaffingDates.Length > 1)
            {
                criticalUnderStaffingDates = criticalUnderStaffingDates.Substring(0, criticalUnderStaffingDates.Length - 1);
                validationError += criticalUnderStaffingValidationError + criticalUnderStaffingDates + Environment.NewLine;
            }

            return validationError;
        }


       

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "timeZone"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "UnderStaffing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "underStaffing"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public string GetUnderStaffingHourString(IUnderStaffingData underStaffing, CultureInfo culture, TimeZoneInfo timeZone, DateTime dateTime )
        {
            var inSufficientHours = string.Empty;
            var criticalUnderStaffingHours = string.Empty;
            var validationError = string.Empty;
            var underStaffingHoursValidationError = string.Format(culture, UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingHours"),
                                                                    dateTime.ToString("d", culture));
            var criticalUnderStaffingHoursValidationError = string.Format(culture, UserTexts.Resources.ResourceManager.GetString("SeriousUnderStaffingHours"),
                              dateTime.ToString("d", culture));

            foreach (var insufficientStaffHours in underStaffing.UnderStaffingHours[UnderStaffHoursStr])
            {
                inSufficientHours += insufficientStaffHours + ",";
            }

            if (inSufficientHours.Length > 1)
            {
                inSufficientHours = inSufficientHours.Substring(0, inSufficientHours.Length - 1);
                validationError += underStaffingHoursValidationError + inSufficientHours + Environment.NewLine;
            }

            foreach (var criticalUnderStaffingHour in underStaffing.UnderStaffingHours[SeriousUnderStaffHoursStr])
            {
                criticalUnderStaffingHours += criticalUnderStaffingHour + ",";
            }

            if (criticalUnderStaffingHours.Length > 1)
            {
                criticalUnderStaffingHours = criticalUnderStaffingHours.Substring(0, criticalUnderStaffingHours.Length - 1);
                validationError += criticalUnderStaffingHoursValidationError + criticalUnderStaffingHours + Environment.NewLine;
            }

            return validationError;
        }
        
            var isWithinUnderStaffingLimit = (1 - exceededRate) >= skill.StaffingThresholds.UnderstaffingFor.Value;

            validatedRequest.IsValid = isWithinUnderStaffingLimit;
            
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