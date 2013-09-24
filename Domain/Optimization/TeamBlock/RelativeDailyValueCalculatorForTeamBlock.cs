using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
    public interface IRelativeDailyValueCalculatorForTeamBlock
    {
        IList<double?> Values(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences advancedPreferences);
        double? DayValue(DateOnly scheduleDay, IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences advancedPreferences);
    }

    public class RelativeDailyValueCalculatorForTeamBlock : IRelativeDailyValueCalculatorForTeamBlock
    {
        public IList<double?> Values(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences advancedPreferences)
        {
            IList<double?> ret = new List<double?>();

            foreach (IScheduleDayPro scheduleDayPro in scheduleMatrix.EffectivePeriodDays)
            {
                double? value = DayValue(scheduleDayPro.Day, scheduleMatrix, advancedPreferences);
                ret.Add(value);
            }

            return ret;
        }
        public double? DayValue(DateOnly scheduleDay, IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences advancedPreferences)
        {
            IList<double> intradayRelativePersonnelDeficits =
                GetIntradayRelativePersonnelDeficits(scheduleDay,scheduleMatrix,advancedPreferences );

            ITeamBlockTargetValueCalculator calculator = new TeamBlockTargetValueCalculator();

            double? result = null;

            foreach (double personnelDeficit in intradayRelativePersonnelDeficits)
            {
                calculator.AddItem(personnelDeficit);
            }
            if (calculator.Count > 0)
            {
                calculator.Analyze();
                switch (advancedPreferences.TargetValueCalculation)
                {
                    case TargetValueOptions.StandardDeviation:
                        result = calculator.StandardDeviation;
                        break;

                    case TargetValueOptions.RootMeanSquare:
                        result = calculator.RootMeanSquare;
                        break;

                    case TargetValueOptions.Teleopti:
                        result = calculator.Teleopti;
                        break;

                }

            }
            return result;
        }

        private IList<double> GetIntradayRelativePersonnelDeficits(DateOnly scheduleDay, IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences advancedPreferences)
        {
            IEnumerable<ISkill> personsActiveSkills = ExtractPersonalSkillList(scheduleDay,scheduleMatrix);

            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1),
                TeleoptiPrincipal.Current.Regional.TimeZone);

            IList<ISkillStaffPeriod> personsSkillStaffPeriods =
                scheduleMatrix.SchedulingStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(personsActiveSkills, dateTimePeriod);

            bool useMinPersonnel = advancedPreferences.UseMinimumStaffing;
            bool useMaxPersonnel = advancedPreferences.UseMaximumStaffing;

            return SkillStaffPeriodsRelativeDifference(personsSkillStaffPeriods, useMinPersonnel, useMaxPersonnel);
        }

        //todo: move to extractor methods
        /// <summary>
        /// Gets the intraday relative differences in hours between forecasted and scheduled resources in the specified
        /// <see cref="ISkillStaffPeriod"/> list.
        /// </summary>
        /// <param name="skillStaffPeriods">The skill staff periods.</param>
        /// <param name="considerMinStaffing">if set to <c>true</c> then consider the preset minimum staffing.</param>
        /// <param name="considerMaxStaffing">if set to <c>true</c> then consider the preset maximum staffing.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is used for getting the difference diference in hours for a day, preceding by a method
        /// that gets the <see cref="ISkillStaffPeriod"/> list for a given day. This method is called by
        /// that <see cref="ISkillStaffPeriod"/> list as parameter.
        /// todo: move to extractor methods
        /// </remarks>
        public static IList<double> SkillStaffPeriodsRelativeDifference(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing)
        {
            if (considerMinStaffing && considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceBoosted()).ToList();
            if (considerMinStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceMinStaffBoosted()).ToList();
            if (considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceMaxStaffBoosted()).ToList();
            return skillStaffPeriods.Select(s => s.RelativeDifference).ToList();
        }

        private IEnumerable<ISkill> ExtractPersonalSkillList(DateOnly scheduleDate, IScheduleMatrixPro scheduleMatrix)
        {
            return scheduleMatrix.Person.Period(scheduleDate).PersonSkillCollection.Select(s => s.Skill).ToList();
        }
    }
}