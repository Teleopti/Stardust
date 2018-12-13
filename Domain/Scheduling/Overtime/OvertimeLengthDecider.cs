using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class OvertimeLengthDecider
    {
	    private readonly IOvertimeSkillStaffPeriodToSkillIntervalDataMapper _overtimeSkillStaffPeriodToSkillIntervalDataMapper;
        private readonly IOvertimeSkillIntervalDataDivider _overtimeSkillIntervalDataDivider;
        private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
        private readonly CalculateBestOvertimeBeforeOrAfter _calculateBestOvertime;
        private readonly IOvertimeSkillIntervalDataAggregator _overtimeSkillIntervalDataAggregator;
	    private readonly PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider _personSkillsForScheduleDaysOvertimeProvider;


	    public OvertimeLengthDecider(IOvertimeSkillStaffPeriodToSkillIntervalDataMapper overtimeSkillStaffPeriodToSkillIntervalDataMapper,
                                     IOvertimeSkillIntervalDataDivider overtimeSkillIntervalDataDivider,
                                     Func<ISchedulingResultStateHolder> schedulingResultStateHolder, CalculateBestOvertimeBeforeOrAfter calculateBestOvertime, 
                                     IOvertimeSkillIntervalDataAggregator overtimeSkillIntervalDataAggregator,
																		 PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider personSkillsForScheduleDaysOvertimeProvider
			)
        {
	        _overtimeSkillIntervalDataAggregator = overtimeSkillIntervalDataAggregator;
	        _personSkillsForScheduleDaysOvertimeProvider = personSkillsForScheduleDaysOvertimeProvider;
	        _overtimeSkillStaffPeriodToSkillIntervalDataMapper = overtimeSkillStaffPeriodToSkillIntervalDataMapper;
            _overtimeSkillIntervalDataDivider = overtimeSkillIntervalDataDivider;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _calculateBestOvertime = calculateBestOvertime;
        }

		public IEnumerable<DateTimePeriod> Decide(IOvertimePreferences overtimePreferences, IScheduleRange scheduleRange, DateOnly dateOnly, MinMax<TimeSpan> duration, MinMax<TimeSpan> specifiedPeriod, bool onlyAvailableAgents)
        {
            var skills = _personSkillsForScheduleDaysOvertimeProvider.Execute(overtimePreferences, scheduleRange.Person.Period(dateOnly)).ToList();
            if (skills.Count == 0) return Enumerable.Empty<DateTimePeriod>();
	        var scheduleDayTemp = scheduleRange.ScheduledDay(dateOnly);
			var minimumResolution = GetMinimumResolution(skills, duration, scheduleDayTemp);
			var skillDays = _schedulingResultStateHolder().SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(-1), dateOnly, dateOnly.AddDays(1) });
            if (skillDays == null) return Enumerable.Empty<DateTimePeriod>();  
            var overtimeSkillIntervalDataList = createOvertimeSkillIntervalDataList(skills, skillDays, minimumResolution);
	        var overtimeSkillIntervalDataAggregatedList = _overtimeSkillIntervalDataAggregator.AggregateOvertimeSkillIntervalData(overtimeSkillIntervalDataList);

			return _calculateBestOvertime.GetBestOvertime(duration, specifiedPeriod, scheduleRange, dateOnly, minimumResolution, onlyAvailableAgents, overtimeSkillIntervalDataAggregatedList);
        }

	    public static int GetMinimumResolution(IList<ISkill> skills, MinMax<TimeSpan> duration, IScheduleDay scheduleDay)
	    {
		    var minimumResolution = skills.Min(x => x.DefaultResolution);
		    var projection = scheduleDay.ProjectionService().CreateProjection();
			var shiftPeriod = projection.Period().GetValueOrDefault();
		    var shiftEndMinute = shiftPeriod.EndDateTime.Minute;
		    var shiftStartMinute = shiftPeriod.StartDateTime.Minute;

			if (minimumResolution > duration.Minimum.TotalMinutes)
				minimumResolution = 15;

		    if (minimumResolution > shiftEndMinute && shiftEndMinute != 0)
			    minimumResolution = shiftEndMinute;

		    if (minimumResolution > shiftStartMinute && shiftStartMinute != 0)
			    minimumResolution = shiftStartMinute;

		    if ((int) duration.Minimum.TotalMinutes % minimumResolution != 0)
			    minimumResolution = 5;
		   
		    return minimumResolution;
	    }

	    private IList<IList<IOvertimeSkillIntervalData>> createOvertimeSkillIntervalDataList(List<ISkill> skills, IEnumerable<ISkillDay> skillDays, int minimumResolution)
	    {
		    IList<IList<IOvertimeSkillIntervalData>> nestedList = new List<IList<IOvertimeSkillIntervalData>>();
		    foreach (var personsActiveSkill in skills)
		    {
			    var filteredSkillDays = skillDays.Where(x => x.Skill == personsActiveSkill).ToList();
			    if (filteredSkillDays.Count == 0) continue;
			    var mappedData =
				    _overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(
					    filteredSkillDays.SelectMany(x => x.SkillStaffPeriodCollection));
			    if (mappedData.Count > 0)
				    mappedData = _overtimeSkillIntervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);
			    if (mappedData.Count > 0)
				    nestedList.Add(mappedData);
		    }
		    return nestedList;
	    }
    }
}