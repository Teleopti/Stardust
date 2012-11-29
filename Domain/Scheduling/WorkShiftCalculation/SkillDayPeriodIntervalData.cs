using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public class SkillDayPeriodPeriodIntervalData : ISkillDayPeriodIntervalData
    {
        private readonly List<ISkillDay> _skillDays;
        private readonly IIntervalDataCalculator _intervalDataMedianCalculator;

        public SkillDayPeriodPeriodIntervalData(List<ISkillDay> skillDays)
        {
            _skillDays = skillDays;
            _intervalDataMedianCalculator= new IntervalDataMedianCalculator();
        }

        public Dictionary<TimeSpan , ISkillIntervalData> GetIntervalDistribution()
        {
            //break the intervals into a span of 1.5 day
            //foreach(var day in _skillDays )
            //{
            //    foreach (var nextDay in _skillDays )
            //    {
            //        if(day. )
            //    }
            //}


            var intervalBasedData = new Dictionary<TimeSpan, List<double>>();
            foreach (var period in _skillDays.SelectMany(skill => skill.SkillStaffPeriodCollection))
            {
                var periodTimeSpan = period.Period.StartDateTime.TimeOfDay;
                if (!intervalBasedData.ContainsKey(periodTimeSpan))
                    intervalBasedData.Add(periodTimeSpan, new List<double>());
                intervalBasedData[periodTimeSpan].Add(period.AbsoluteDifference);
            }
			////TODO: need to fix the value of 0s
			//return intervalBasedData.ToDictionary<KeyValuePair<TimeSpan, List<double>>, TimeSpan, ISkillIntervalData>
			//    (interval => interval.Key, interval => new SkillIntervalData(_intervalDataMedianCalculator.Calculate(interval.Value), 0, 0, 0));
        }

        
    }

    public interface ISkillDayPeriodIntervalData
    {
        Dictionary<TimeSpan, ISkillIntervalData> GetIntervalDistribution();
    }
}