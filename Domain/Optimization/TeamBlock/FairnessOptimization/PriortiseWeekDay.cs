using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IPriortiseWeekDay
    {
        int HigestPriority { get; }
        int LowestPriority { get; }
        HashSet<DateOnly> GetDateListOnPriority(int priority);
        void IdentifyPriority(IList<IScheduleDay> scheduleDays);
    }

    public class PriortiseWeekDay : IPriortiseWeekDay
    {
        private readonly IDictionary<int, HashSet<DateOnly>> _dateOnPriorityDays;
        private IDictionary<DayOfWeek, int> _weekDayProprity;

        public PriortiseWeekDay()
        {
            _dateOnPriorityDays = new Dictionary<int, HashSet<DateOnly>>();
        }

        public void IdentifyPriority(IList<IScheduleDay> scheduleDays)
        {
            populateReferencePriority();
            foreach (IScheduleDay scheduleDay in scheduleDays)
            {
                DateOnly date = scheduleDay.DateOnlyAsPeriod.DateOnly;
                int priority = _weekDayProprity[date.DayOfWeek];
                if (_dateOnPriorityDays.ContainsKey(priority))
                    _dateOnPriorityDays[priority].Add(date);
                else
                {
                    _dateOnPriorityDays.Add(priority, new HashSet<DateOnly>());
                    _dateOnPriorityDays[priority].Add(date);
                }
            }
        }

        //This method should be retested once the actualy priority assignment is done.

        public HashSet<DateOnly> GetDateListOnPriority(int priority)
        {
            if (_dateOnPriorityDays.ContainsKey(priority))
            {
                return _dateOnPriorityDays[priority];
            }
            return new HashSet<DateOnly>();
        }

        public int HigestPriority
        {
            get { return _dateOnPriorityDays.Keys.Max(); }
        }

        public int LowestPriority
        {
            get { return _dateOnPriorityDays.Keys.Min(); }
        }

        private void populateReferencePriority()
        {
            _weekDayProprity = new Dictionary<DayOfWeek, int>
                {
                    {DayOfWeek.Monday, 7},
                    {DayOfWeek.Tuesday, 6},
                    {DayOfWeek.Wednesday, 5},
                    {DayOfWeek.Thursday, 4},
                    {DayOfWeek.Friday, 3},
                    {DayOfWeek.Saturday, 2},
                    {DayOfWeek.Sunday, 1}
                };
        }
    }
}