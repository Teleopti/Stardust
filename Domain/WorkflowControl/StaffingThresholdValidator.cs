using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class StaffingThresholdValidator : IAbsenceRequestValidator
    {
		private static readonly ILog Logger = LogManager.GetLogger(typeof(StaffingThresholdValidator));

        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
        public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }
        public IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }

        public string InvalidReason
        {
            get { return "RequestDenyReasonSkillThreshold"; }
        }

        public string DisplayText
        {
            get { return UserTexts.Resources.Intraday; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public bool Validate(IAbsenceRequest absenceRequest)
        {
            InParameter.NotNull("SchedulingResultStateHolder", SchedulingResultStateHolder);
            InParameter.NotNull("ResourceOptimizationHelper", ResourceOptimizationHelper);

            var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
            var localPeriod =
                absenceRequest.Period.ToDateOnlyPeriod(timeZone);

            foreach (DateOnly dateTime in localPeriod.DayCollection())
            {
                //As the resource calculation currently always being made from the viewpoint timezone, this is what we need here!
                var dayPeriod = new DateOnlyPeriod(dateTime, dateTime).ToDateTimePeriod(timeZone);
                var datesToResourceCalculate = dayPeriod.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
                foreach (DateOnly dateOnly in datesToResourceCalculate.DayCollection())
                {
                    ResourceOptimizationHelper.ResourceCalculateDate(dateOnly, true, true);
                }
                var calculatedPeriod = datesToResourceCalculate.ToDateTimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
                var scheduleDay = SchedulingResultStateHolder.Schedules[absenceRequest.Person].ScheduledDay(dateTime);
                var absenceLayers = scheduleDay.ProjectionService().CreateProjection().FilterLayers(absenceRequest.Absence);
                foreach (var absenceLayer in absenceLayers)
                {
                    var sharedPeriod = calculatedPeriod.Intersection(absenceLayer.Period);
                    if (sharedPeriod.HasValue)
                    {
                        foreach (var skill in SchedulingResultStateHolder.Skills)
                        {
							var skillStaffPeriodList = SchedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, sharedPeriod.Value);
							if (skillStaffPeriodList == null || skillStaffPeriodList.Count == 0) return true;
                        	if (skill == null) continue;
                        	if (!ValidateUnderstaffing(skill, skillStaffPeriodList))
                        	{
								Logger.DebugFormat("The request failed the understaffing validation for skill {0}.", skill.Name);
                        		return false;
                        	}
                        	if (!ValidateSeriousUnderstaffing(skill, skillStaffPeriodList))
                        	{
								Logger.DebugFormat("The request failed the serious understaffing validation for skill {0}.", skill.Name);
                        		return false;
                        	}
                        }
                    }
                }
            }
            return true;
        }

    	public static bool ValidateSeriousUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList)
    	{
    		if (skillStaffPeriodList == null) throw new ArgumentNullException("skillStaffPeriodList");
    		var intervalHasSeriousUnderstaffing = new IntervalHasSeriousUnderstaffing(skill);
    		
			return !skillStaffPeriodList.Any(intervalHasSeriousUnderstaffing.IsSatisfiedBy);
    	}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static bool ValidateUnderstaffing(ISkill skill, IEnumerable<ISkillStaffPeriod> skillStaffPeriodList)
    	{
    		if (skillStaffPeriodList == null) throw new ArgumentNullException("skillStaffPeriodList");

			var intervalHasUnderstaffing = new IntervalHasUnderstaffing(skill);
    		var exceededUnderstaffingList = skillStaffPeriodList.Where(intervalHasUnderstaffing.IsSatisfiedBy).ToList();
    		var exceededRate = exceededUnderstaffingList.Sum(t => t.Period.ElapsedTime().TotalMinutes) / skillStaffPeriodList.Sum(t => t.Period.ElapsedTime().TotalMinutes);
    		
			return exceededRate <= skill.StaffingThresholds.UnderstaffingFor.Value;
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
                int result = (SchedulingResultStateHolder != null ? SchedulingResultStateHolder.GetHashCode() : 0);
                result = (result*397) ^ (PersonAccountBalanceCalculator != null ? PersonAccountBalanceCalculator.GetHashCode() : 0);
                result = (result*397) ^ (ResourceOptimizationHelper != null ? ResourceOptimizationHelper.GetHashCode() : 0);
                result = (result * 397) ^ (BudgetGroupAllowanceSpecification != null ? BudgetGroupAllowanceSpecification.GetHashCode() : 0);
                result = (result * 397) ^ (GetType().GetHashCode());
                return result;
            }
        }
    }
}