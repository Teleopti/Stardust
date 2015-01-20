using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
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
	    private readonly ISkillIntervalDataOpenHour _skillIntervalDataOpenHour;
	    private readonly IOvertimeSkillIntervalDataToSkillIntervalDataMapper _skillIntervalDataMapper; 


        public OvertimeLengthDecider(ISkillResolutionProvider skillResolutionProvider,
                                     IOvertimeSkillStaffPeriodToSkillIntervalDataMapper overtimeSkillStaffPeriodToSkillIntervalDataMapper,
                                     IOvertimeSkillIntervalDataDivider overtimeSkillIntervalDataDivider,
                                     ISchedulingResultStateHolder schedulingResultStateHolder, ICalculateBestOvertime calculateBestOvertime, OvertimePeriodValueMapper overtimePeriodValueMapper, 
                                     IOvertimeSkillIntervalDataAggregator overtimeSkillIntervalDataAggregator,
									 ISkillIntervalDataOpenHour skillIntervalDataOpenHour,
									 IOvertimeSkillIntervalDataToSkillIntervalDataMapper skillIntervalDataMapper
			)
        {
            _overtimePeriodValueMapper = overtimePeriodValueMapper;
            _overtimeSkillIntervalDataAggregator = overtimeSkillIntervalDataAggregator;
	        _skillIntervalDataOpenHour = skillIntervalDataOpenHour;
	        _skillIntervalDataMapper = skillIntervalDataMapper;
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

			
			var skillIntervalDataList = 
				overtimeSkillIntervalDataAggregatedList.Select(i => _skillIntervalDataMapper.Map(i)).ToList();

			var openHoursList = getOpenHours(dateOnly, skillIntervalDataList);

			result = _calculateBestOvertime.GetBestOvertime(duration, specifiedPeriod, mappedAggregatedList, scheduleDay, minimumResolution, onlyAvailableAgents, openHoursList);
	        return result;
        }

	    private IEnumerable<DateTimePeriod> getOpenHours(DateOnly dateOnly, IList<ISkillIntervalData> skillIntervalDataList)
	    {
		    var openHoursList = new List<DateTimePeriod>();
		    var openHours = _skillIntervalDataOpenHour.GetOpenHours(skillIntervalDataList, dateOnly);
		    if (openHours.HasValue)
		    {
			    var period =
				    new DateTimePeriod(dateOnly.Date.ToLocalTime().Add(openHours.Value.StartTime).ToUniversalTime(),
					    dateOnly.Date.ToLocalTime().Add(openHours.Value.EndTime).ToUniversalTime());
			    openHoursList.Add(period);
		    }
		    var nextDayDateOnly = dateOnly.AddDays(1);
		    var openHoursNextDay = _skillIntervalDataOpenHour.GetOpenHours(skillIntervalDataList, nextDayDateOnly);
		    if (openHoursNextDay.HasValue)
		    {
			    var periodNextDay =
				    new DateTimePeriod(dateOnly.Date.ToLocalTime().Add(openHoursNextDay.Value.StartTime).ToUniversalTime(),
					    dateOnly.Date.Add(openHoursNextDay.Value.EndTime).ToUniversalTime());
			    openHoursList.Add(periodNextDay);
		    }
		    return openHoursList;
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