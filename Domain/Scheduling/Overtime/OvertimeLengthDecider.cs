using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IOvertimeLengthDecider
    {
		IList<DateTimePeriod> Decide(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay, IActivity activity, MinMax<TimeSpan> duration, MinMax<TimeSpan> specifiedPeriod, bool onlyAvailableAgents);
    }

    public class OvertimeLengthDecider : IOvertimeLengthDecider
    {
        private readonly OvertimePeriodValueMapper _overtimePeriodValueMapper;
        private readonly ISkillResolutionProvider _skillResolutionProvider;
        private readonly IOvertimeSkillStaffPeriodToSkillIntervalDataMapper _overtimeSkillStaffPeriodToSkillIntervalDataMapper;
        private readonly IOvertimeSkillIntervalDataDivider _overtimeSkillIntervalDataDivider;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly ICalculateBestOvertime _calculateBestOvertime;
        private readonly IOvertimeSkillIntervalDataAggregator _overtimeSkillIntervalDataAggregator;


        public OvertimeLengthDecider(ISkillResolutionProvider skillResolutionProvider,
                                     IOvertimeSkillStaffPeriodToSkillIntervalDataMapper overtimeSkillStaffPeriodToSkillIntervalDataMapper,
                                     IOvertimeSkillIntervalDataDivider overtimeSkillIntervalDataDivider,
                                     ISchedulingResultStateHolder schedulingResultStateHolder, ICalculateBestOvertime calculateBestOvertime, OvertimePeriodValueMapper overtimePeriodValueMapper, 
                                     IOvertimeSkillIntervalDataAggregator overtimeSkillIntervalDataAggregator
			)
        {
            _overtimePeriodValueMapper = overtimePeriodValueMapper;
            _overtimeSkillIntervalDataAggregator = overtimeSkillIntervalDataAggregator;
	        _skillResolutionProvider = skillResolutionProvider;
            _overtimeSkillStaffPeriodToSkillIntervalDataMapper = overtimeSkillStaffPeriodToSkillIntervalDataMapper;
            _overtimeSkillIntervalDataDivider = overtimeSkillIntervalDataDivider;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _calculateBestOvertime = calculateBestOvertime;
        }

		public IList<DateTimePeriod> Decide(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay, IActivity activity, MinMax<TimeSpan> duration, MinMax<TimeSpan> specifiedPeriod, bool onlyAvailableAgents)
        {
			IList<DateTimePeriod> result = new List<DateTimePeriod>();

            var skills = aggregateSkills(person, dateOnly).Where(x => x.Activity == activity).ToList();
            if (skills.Count == 0) return result;
            var minimumResolution = _skillResolutionProvider.MinimumResolution(skills);
			var nextDayDateOnly = dateOnly.AddDays(1);
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly, nextDayDateOnly });
            if (skillDays == null) return result;
            
            var overtimeSkillIntervalDataList = createOvertimeSkillIntervalDataList(skills, skillDays, minimumResolution);
			var overtimeSkillIntervalDataAggregatedList = _overtimeSkillIntervalDataAggregator.AggregateOvertimeSkillIntervalData(overtimeSkillIntervalDataList);
			var mappedAggregatedList = _overtimePeriodValueMapper.Map(overtimeSkillIntervalDataAggregatedList);

			result = _calculateBestOvertime.GetBestOvertime(duration, specifiedPeriod, mappedAggregatedList, scheduleDay,
				minimumResolution, onlyAvailableAgents, overtimeSkillIntervalDataAggregatedList);
	        return result;
        }

	    private IList<IList<IOvertimeSkillIntervalData>> createOvertimeSkillIntervalDataList(List<ISkill> skills, IList<ISkillDay> skillDays, int minimumResolution)
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

	    private static IEnumerable<ISkill> aggregateSkills(IPerson person, DateOnly dateOnly)
        {
            var ret = new List<ISkill>();
            var personPeriod = person.Period(dateOnly);

            foreach (var personSkill in personPeriod.PersonSkillCollection)
            {
                if (!ret.Contains(personSkill.Skill))
                    ret.Add(personSkill.Skill);
            }
            return ret;
        }
    }
}