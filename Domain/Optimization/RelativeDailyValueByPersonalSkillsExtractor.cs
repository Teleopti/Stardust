using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Calculates the daily intraday relative standard deviations filtered by the
    /// persons active skills on the given day.
    /// </summary>
    public class RelativeDailyValueByPersonalSkillsExtractor : IScheduleResultDailyValueCalculator
    {
        private readonly IAdvancedPreferences _advancedPreferences;
        private readonly IScheduleMatrixPro _scheduleMatrix;

        public RelativeDailyValueByPersonalSkillsExtractor(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences advancedPreferences)
        {
            _scheduleMatrix = scheduleMatrix;
            _advancedPreferences = advancedPreferences;
        }

        public IList<double?> Values()
        {
            IList<double?> ret = new List<double?>();

            foreach (IScheduleDayPro scheduleDayPro in _scheduleMatrix.EffectivePeriodDays)
            {
                double? value = DayValue(scheduleDayPro.Day);
                ret.Add(value);
            }

            return ret;
        }

        public double? DayValue(DateOnly scheduleDay)
        {
            IList<double> intradayRelativePersonnelDeficits =
                GetIntradayRelativePersonnelDeficits(scheduleDay);

            double? result = null;

            if (intradayRelativePersonnelDeficits.Any())
            {
				switch (_advancedPreferences.TargetValueCalculation)
				{
					case TargetValueOptions.StandardDeviation:
						result = Calculation.Variances.StandardDeviation(intradayRelativePersonnelDeficits);
						break;

					case TargetValueOptions.RootMeanSquare:
						result = Calculation.Variances.RMS(intradayRelativePersonnelDeficits);
						break;

					case TargetValueOptions.Teleopti:
						result = Calculation.Variances.Teleopti(intradayRelativePersonnelDeficits);
						break;

				}

            }
            return result;
        }

        // todo: move to extractor methods
        private IList<double> GetIntradayRelativePersonnelDeficits(DateOnly scheduleDay)
        {
            IEnumerable<ISkill> personsActiveSkills = ExtractPersonalSkillList(scheduleDay);

            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
               scheduleDay.Date, scheduleDay.Date.AddDays(1),
               TeleoptiPrincipal.Current.Regional.TimeZone);
           
            IList<ISkillStaffPeriod> personsSkillStaffPeriods =
                _scheduleMatrix.SchedulingStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(personsActiveSkills, dateTimePeriod);
           
            bool useMinPersonnel = _advancedPreferences.UseMinimumStaffing;
            bool useMaxPersonnel = _advancedPreferences.UseMaximumStaffing;

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
                return skillStaffPeriods.Select(s => s.RelativeDifferenceMinStaffBoosted() ).ToList();
            if (considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceMaxStaffBoosted()).ToList();
            return skillStaffPeriods.Select(s => s.RelativeDifference).ToList();
        }

        private IEnumerable<ISkill> ExtractPersonalSkillList(DateOnly scheduleDate)
        {
            return _scheduleMatrix.Person.Period(scheduleDate).PersonSkillCollection.Select(s => s.Skill).ToList();
        }
    }
}