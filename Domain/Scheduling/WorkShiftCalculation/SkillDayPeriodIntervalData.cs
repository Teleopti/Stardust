using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public class SkillDayPeriodIntervalData : ISkillDayPeriodIntervalData
    {
        private readonly List<ISkillDay> _skillDays;
        private readonly IIntervalDataCalculator _intervalDataMedianCalculator;

        public SkillDayPeriodIntervalData()
        {
            
        }

        public SkillDayPeriodIntervalData(List<ISkillDay> skillDays)
        {
            _skillDays = skillDays;
            _intervalDataMedianCalculator= new IntervalDataMedianCalculator();
        }

        public Dictionary<TimeSpan, ISkillIntervalData> GetIntervalDistribution()
        {
            var intervalBasedData = new Dictionary<TimeSpan, List<double>>();
            var firstDay = _skillDays.FirstOrDefault();
            if (firstDay == null) throw new ArgumentException("skillDays are empty");
            var resolution = firstDay.Skill.DefaultResolution;
            for (var i = 0; i < TimeSpan.FromDays(2).TotalMinutes/resolution; i++)
            {
                intervalBasedData.Add(TimeSpan.FromMinutes(i*resolution), new List<double>());
            }
            
            foreach (var period in _skillDays.SelectMany(skill => skill.SkillStaffPeriodCollection))
            {
                var periodTimeSpan = period.Period.StartDateTime.TimeOfDay;
                if (intervalBasedData.ContainsKey(periodTimeSpan))
                {
                    var invervalData = intervalBasedData[periodTimeSpan];
                    invervalData.Add(period.AbsoluteDifference);
                }
            }
            var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);
            var result = new Dictionary<TimeSpan, ISkillIntervalData>();
            foreach (var intervalPair in intervalBasedData)
            {
                var startTime = baseDate.Date.AddMinutes(intervalPair.Key.TotalMinutes);
                var endTime = startTime.AddMinutes(resolution);
                var calculatedDemand = _intervalDataMedianCalculator.Calculate(intervalPair.Value);
                var skillIntervalData = new SkillIntervalData(new DateTimePeriod(startTime, endTime), calculatedDemand,0, 0, 0, 0);
                result.Add(intervalPair.Key, skillIntervalData);
            }
            return result;
        }
    }

    public interface ISkillDayPeriodIntervalData
    {
        Dictionary<TimeSpan, ISkillIntervalData> GetIntervalDistribution();
    }
}