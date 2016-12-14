﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IOvertimeLengthDecider
    {
		IEnumerable<DateTimePeriod> Decide(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay, IActivity activity, MinMax<TimeSpan> duration, MinMax<TimeSpan> specifiedPeriod, bool onlyAvailableAgents);
    }

    public class OvertimeLengthDecider : IOvertimeLengthDecider
    {
        private readonly OvertimePeriodValueMapper _overtimePeriodValueMapper;
        private readonly ISkillResolutionProvider _skillResolutionProvider;
        private readonly IOvertimeSkillStaffPeriodToSkillIntervalDataMapper _overtimeSkillStaffPeriodToSkillIntervalDataMapper;
        private readonly IOvertimeSkillIntervalDataDivider _overtimeSkillIntervalDataDivider;
        private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
        private readonly ICalculateBestOvertime _calculateBestOvertime;
        private readonly IOvertimeSkillIntervalDataAggregator _overtimeSkillIntervalDataAggregator;
	    private readonly IPersonSkillsForScheduleDaysOvertimeProvider _personSkillsForScheduleDaysOvertimeProvider;


	    public OvertimeLengthDecider(ISkillResolutionProvider skillResolutionProvider,
                                     IOvertimeSkillStaffPeriodToSkillIntervalDataMapper overtimeSkillStaffPeriodToSkillIntervalDataMapper,
                                     IOvertimeSkillIntervalDataDivider overtimeSkillIntervalDataDivider,
                                     Func<ISchedulingResultStateHolder> schedulingResultStateHolder, ICalculateBestOvertime calculateBestOvertime, OvertimePeriodValueMapper overtimePeriodValueMapper, 
                                     IOvertimeSkillIntervalDataAggregator overtimeSkillIntervalDataAggregator,
																		 IPersonSkillsForScheduleDaysOvertimeProvider personSkillsForScheduleDaysOvertimeProvider
			)
        {
            _overtimePeriodValueMapper = overtimePeriodValueMapper;
            _overtimeSkillIntervalDataAggregator = overtimeSkillIntervalDataAggregator;
	        _personSkillsForScheduleDaysOvertimeProvider = personSkillsForScheduleDaysOvertimeProvider;
	        _skillResolutionProvider = skillResolutionProvider;
            _overtimeSkillStaffPeriodToSkillIntervalDataMapper = overtimeSkillStaffPeriodToSkillIntervalDataMapper;
            _overtimeSkillIntervalDataDivider = overtimeSkillIntervalDataDivider;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _calculateBestOvertime = calculateBestOvertime;
        }

		public IEnumerable<DateTimePeriod> Decide(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay, IActivity activity, MinMax<TimeSpan> duration, MinMax<TimeSpan> specifiedPeriod, bool onlyAvailableAgents)
        {
            var skills = _personSkillsForScheduleDaysOvertimeProvider.Execute(person.Period(dateOnly), activity).ToList();
            if (skills.Count == 0) return Enumerable.Empty<DateTimePeriod>();
            var minimumResolution = _skillResolutionProvider.MinimumResolution(skills);
            var skillDays = _schedulingResultStateHolder().SkillDaysOnDateOnly(new List<DateOnly> { dateOnly.AddDays(-1), dateOnly, dateOnly.AddDays(1) });
            if (skillDays == null) return Enumerable.Empty<DateTimePeriod>();
            
            var overtimeSkillIntervalDataList = createOvertimeSkillIntervalDataList(skills, skillDays, minimumResolution);
			var overtimeSkillIntervalDataAggregatedList = _overtimeSkillIntervalDataAggregator.AggregateOvertimeSkillIntervalData(overtimeSkillIntervalDataList);
			var mappedAggregatedList = _overtimePeriodValueMapper.Map(overtimeSkillIntervalDataAggregatedList);

			return _calculateBestOvertime.GetBestOvertime(duration, specifiedPeriod, mappedAggregatedList, scheduleDay,
				minimumResolution, onlyAvailableAgents, overtimeSkillIntervalDataAggregatedList);
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