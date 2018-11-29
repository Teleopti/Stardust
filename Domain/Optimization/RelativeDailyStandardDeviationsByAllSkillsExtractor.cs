using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Calculates the daily intraday relative standard deviations filtered by the
    /// persons active skills on the given day.
    /// </summary>
    public class RelativeDailyStandardDeviationsByAllSkillsExtractor : IScheduleResultDataExtractor
    {
	    private readonly IEnumerable<DateOnly> _dates;
	    private readonly SchedulingOptions _schedulingOptions;
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly TimeZoneInfo _userTimeZoneInfo;

        public RelativeDailyStandardDeviationsByAllSkillsExtractor(IEnumerable<DateOnly> dates, SchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder, TimeZoneInfo userTimeZoneInfo)
        {
	        _dates = dates;
	        _schedulingOptions = schedulingOptions;
	        _schedulingResultStateHolder = schedulingResultStateHolder;
	        _userTimeZoneInfo = userTimeZoneInfo;
        }

        public IList<double?> Values()
        {
            return _dates.Select(DayValue).ToArray();
        }

        public double? DayValue(DateOnly scheduleDay)
        {
            IList<double> intradayRelativePersonnelDeficits = GetIntradayRelativePersonnelDeficits(scheduleDay);

            double? result = null;

            if (intradayRelativePersonnelDeficits.Any())
            {
                result = Calculation.Variances.StandardDeviation(intradayRelativePersonnelDeficits);
            }
            return result;
        }

        // todo: move to extractor methods
        private IList<double> GetIntradayRelativePersonnelDeficits(DateOnly scheduleDay)
        {
	        var dateTimePeriod = scheduleDay.ToDateTimePeriod(_userTimeZoneInfo);

	        var allSkills =
		        _schedulingResultStateHolder.Skills.Where(
			        skill => skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).ToArray();
            var skillStaffPeriods =
								_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(allSkills, dateTimePeriod);
           
            bool useMinPersonnel = _schedulingOptions.UseMinimumStaffing;
            bool useMaxPersonnel = _schedulingOptions.UseMaximumStaffing;

            return SkillStaffPeriodsRelativeDifferenceHours(skillStaffPeriods, useMinPersonnel, useMaxPersonnel);
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
        public static IList<double> SkillStaffPeriodsRelativeDifferenceHours(IEnumerable<ISkillStaffPeriod> skillStaffPeriods, bool considerMinStaffing, bool considerMaxStaffing)
        {
            if (considerMinStaffing && considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            if (considerMinStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceMinStaffBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            if (considerMaxStaffing)
                return skillStaffPeriods.Select(s => s.RelativeDifferenceMaxStaffBoosted() * s.Period.ElapsedTime().TotalHours).ToList();
            return skillStaffPeriods.Select(s => s.RelativeDifference * s.Period.ElapsedTime().TotalHours).ToList();
        }
    }
}