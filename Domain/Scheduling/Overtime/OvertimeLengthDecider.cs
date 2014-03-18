using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IOvertimeLengthDecider
    {
        TimeSpan Decide(IPerson person, DateOnly dateOnly, DateTime overtimeStartTime, IActivity activity,
                             MinMax<TimeSpan> duration);
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
                                        IOvertimeSkillIntervalDataAggregator overtimeSkillIntervalDataAggregator)
        {
            _overtimePeriodValueMapper = overtimePeriodValueMapper;
            _overtimeSkillIntervalDataAggregator = overtimeSkillIntervalDataAggregator;
            _skillResolutionProvider = skillResolutionProvider;
            _overtimeSkillStaffPeriodToSkillIntervalDataMapper = overtimeSkillStaffPeriodToSkillIntervalDataMapper;
            _overtimeSkillIntervalDataDivider = overtimeSkillIntervalDataDivider;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _calculateBestOvertime = calculateBestOvertime;
        }

        public TimeSpan Decide(IPerson person, DateOnly dateOnly, DateTime overtimeStartTime, IActivity activity, MinMax<TimeSpan> duration)
        {
            var skills = aggregateSkills(person, dateOnly).Where(x => x.Activity == activity).ToList();
            if (skills.Count == 0) return TimeSpan.Zero;
            var minimumResolution = _skillResolutionProvider.MinimumResolution(skills);
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { dateOnly });
            if (skillDays == null) return TimeSpan.Zero;
            
            IList<IList<IOvertimeSkillIntervalData>> nestedList = new List<IList<IOvertimeSkillIntervalData>>();
            foreach (var personsActiveSkill in skills)
            {
                var filteredSkillDays = skillDays.Where(x => x.Skill == personsActiveSkill ).ToList();
                if (filteredSkillDays.Count == 0) continue;
                var mappedData = _overtimeSkillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(filteredSkillDays.SelectMany(x => x.SkillStaffPeriodCollection));
                if (mappedData.Count > 0)
                    mappedData = _overtimeSkillIntervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);
                if(mappedData.Count > 0)
                    nestedList.Add(mappedData);
            }

            var aggregatedMappedData  = _overtimeSkillIntervalDataAggregator.AggregateOvertimeSkillIntervalData(nestedList);

            return _calculateBestOvertime.GetBestOvertime(duration, _overtimePeriodValueMapper.Map(aggregatedMappedData), overtimeStartTime, minimumResolution);
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

        //backgroud worker problems
    }
}