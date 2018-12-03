using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Collections;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class TaskOwnerHelper
    {
        private readonly IList<ITaskOwner> _taskOwnerDays;

        /// <summary>
        /// Gets the task owner days.
        /// </summary>
        /// <value>The task owner days.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public IList<ITaskOwner> TaskOwnerDays => _taskOwnerDays;

	    /// <summary>
        /// Initializes a new instance of the <see cref="TaskOwnerHelper"/> class.
        /// </summary>
        /// <param name="taskOwnerDays">The task owner days.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public TaskOwnerHelper(IEnumerable taskOwnerDays)
        {
            InParameter.NotNull(nameof(taskOwnerDays), taskOwnerDays);

            _taskOwnerDays = taskOwnerDays.OfType<ITaskOwner>().ToList();
        }

        #region Seasonality

        /// <summary>
        /// Creates the week task owner periods.
        /// This method handles dates in spridda skurar
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet, peterwe
        /// Created date: 2008-03-10
        /// </remarks>
        public IList<TaskOwnerPeriod> CreateWeekTaskOwnerPeriods(Calendar calendar)
        {
            if (_taskOwnerDays.Count == 0) return new List<TaskOwnerPeriod>();

            IDictionary<int,IList<ITaskOwner>> taskOwnerDictionary = new Dictionary<int,IList<ITaskOwner>>();
            foreach (ITaskOwner task in _taskOwnerDays)
            {
                int currentDay = calendar.GetDayOfMonth(task.CurrentDate.Date);
                int dayOfWeek;
                int weekIndex = Math.DivRem(currentDay - 1, 7, out dayOfWeek);

                IList<ITaskOwner> taskOwners;
                if (taskOwnerDictionary.TryGetValue(weekIndex, out taskOwners))
                    taskOwners.Add(task);
                else
                    taskOwnerDictionary.Add(weekIndex, new List<ITaskOwner> { task });
            }

            DateTime dt = CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime;
            IList<TaskOwnerPeriod> taskOwnerPeriodWeeks = new List<TaskOwnerPeriod>();
            TaskOwnerPeriod calculatedTaskOwnerPeriod = CreateWholeSelectionTaskOwnerPeriod();
            for (int i = 0; i < 5; i++)
            {
                IList<ITaskOwner> taskOwners;
                if (!taskOwnerDictionary.TryGetValue(i, out taskOwners))
                {
                    if (i < 4)
                        calculatedTaskOwnerPeriod.CurrentDate = new DateOnly(calendar.AddDays(dt, ((i + 1) * 6)));
                    else
                        calculatedTaskOwnerPeriod.CurrentDate = new DateOnly(calendar.AddDays(dt, 29));
                    taskOwnerPeriodWeeks.Add(new TaskOwnerPeriod(
                                                 calculatedTaskOwnerPeriod.CurrentDate,
                                                 calculatedTaskOwnerPeriod.TaskOwnerDayCollection,
                                                 calculatedTaskOwnerPeriod.TypeOfTaskOwnerPeriod));
                }
                else
                {
                    taskOwnerPeriodWeeks.Add(new TaskOwnerPeriod(
                        taskOwners[0].CurrentDate,
                        taskOwners,
                        TaskOwnerPeriodType.Other));
                }
            }

            taskOwnerPeriodWeeks.ForEach(wp => wp.Release());
            return taskOwnerPeriodWeeks;
        }


        /// <summary>
        /// Creates the day task owner periods for missing days of week.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-13
        /// </remarks>
        public IList<TaskOwnerPeriod> CreateDayTaskOwnerPeriods(Calendar calendar)
        {
            if (_taskOwnerDays.Count == 0) return new List<TaskOwnerPeriod>();

            IDictionary<DayOfWeek, IList<ITaskOwner>> taskOwnerDictionary = new Dictionary<DayOfWeek, IList<ITaskOwner>>();
            foreach (ITaskOwner task in _taskOwnerDays)
            {
                DayOfWeek currentDay = task.CurrentDate.DayOfWeek;

                IList<ITaskOwner> taskOwners;
                if (!taskOwnerDictionary.TryGetValue(currentDay,out taskOwners))
                {
                    taskOwnerDictionary.Add(currentDay,new List<ITaskOwner>{task});
                }
                else
                {
                    taskOwners.Add(task);
                }
            }
            
            DateTime dt = calendar.MinSupportedDateTime.AddYears(1);
            IList<TaskOwnerPeriod> taskOwnerPeriods = new List<TaskOwnerPeriod>();
            TaskOwnerPeriod calculatedTaskOwnerPeriod = CreateWholeSelectionTaskOwnerPeriod(); 
            foreach(DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                IList<ITaskOwner> taskOwners;
                if (!taskOwnerDictionary.TryGetValue(dayOfWeek, out taskOwners))
                    taskOwnerPeriods.Add(
                        new TaskOwnerPeriod(
                           new DateOnly(dt.AddDays(-(int)calendar.GetDayOfWeek(dt) + (int)dayOfWeek)),
                                            calculatedTaskOwnerPeriod.TaskOwnerDayCollection, TaskOwnerPeriodType.Other));
                else
                    taskOwnerPeriods.Add(new TaskOwnerPeriod(taskOwners[0].CurrentDate, taskOwners,
                                                             TaskOwnerPeriodType.Other));
            }

            taskOwnerPeriods.ForEach(wp => wp.Release());
            return taskOwnerPeriods;
        }

        /// <summary>
        /// Creates the month task owner periods.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-06
        /// </remarks>
        public IList<TaskOwnerPeriod> CreateMonthTaskOwnerPeriods(Calendar calendar)
        {
            if (_taskOwnerDays.Count == 0) return new List<TaskOwnerPeriod>();

            var currentDate = _taskOwnerDays.Min(tod => tod.CurrentDate);
            var maxDate = _taskOwnerDays.Max(tod => tod.CurrentDate);
            maxDate = new DateOnly(DateHelper.GetLastDateInMonth(maxDate.Date, calendar));
            IDictionary<int, IList<ITaskOwner>> taskOwnerDictionary = new Dictionary<int, IList<ITaskOwner>>();

            do
            {
                TaskOwnerPeriod taskOwnerPeriod = new TaskOwnerPeriod(currentDate, null, TaskOwnerPeriodType.Month);

                var currentTaskOwnerDays = _taskOwnerDays.Where(tod =>
                                                                tod.CurrentDate >= taskOwnerPeriod.StartDate &&
                                                                tod.CurrentDate <= taskOwnerPeriod.EndDate);

                int currentMonth = calendar.GetMonth(currentDate.Date);

	            currentDate = currentDate.AddMonths(calendar, 1);
                if (currentTaskOwnerDays.IsEmpty()) continue;

                IList<ITaskOwner> taskOwners;
                if (!taskOwnerDictionary.TryGetValue(currentMonth, out taskOwners))
                {
                    taskOwnerDictionary.Add(currentMonth, currentTaskOwnerDays.ToList());
                }
                else
                {
                    ((List<ITaskOwner>) taskOwners).AddRange(currentTaskOwnerDays);
                }
            } while (currentDate <= maxDate);

            int monthsInYear =
                calendar.GetMonthsInYear(
                    calendar.GetYear(maxDate.Date));

            DateTime dt = calendar.AddYears(calendar.MinSupportedDateTime,1);
            dt = calendar.AddMonths(dt, 1 - calendar.GetMonth(dt));
            IList<TaskOwnerPeriod> taskOwnerPeriodMonths = new List<TaskOwnerPeriod>();
            TaskOwnerPeriod calculatedTaskOwnerPeriod = CreateWholeSelectionTaskOwnerPeriod();
            for (int i = 0; i < monthsInYear; i++)
            {
                IList<ITaskOwner> taskOwners;
                if (!taskOwnerDictionary.TryGetValue(i+1, out taskOwners))
                {
                    calculatedTaskOwnerPeriod.CurrentDate = new DateOnly(calendar.AddMonths(dt, i));
                    taskOwnerPeriodMonths.Add(new TaskOwnerPeriod(
                                                  calculatedTaskOwnerPeriod.CurrentDate,
                                                  calculatedTaskOwnerPeriod.TaskOwnerDayCollection,
                                                  calculatedTaskOwnerPeriod.TypeOfTaskOwnerPeriod));
                }
                else
                {
                    taskOwnerPeriodMonths.Add(new TaskOwnerPeriod(
                                                  taskOwners[0].CurrentDate,
                                                  taskOwners,
                                                  TaskOwnerPeriodType.Other));
                }
            }

            taskOwnerPeriodMonths.ForEach(wp => wp.Release());

            return taskOwnerPeriodMonths.OrderBy(t => calendar.GetMonth(t.CurrentDate.Date)).ToList();
        }

        #endregion

        /// <summary>
        /// Splits to months.
        /// Splitted these into two different methods instead of taking TaskOwnerPeriodType as argument
        /// (to many ifs)
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-09
        /// </remarks>
        public IList<TaskOwnerPeriod> CreateWholeMonthTaskOwnerPeriods()
        {
            IList<TaskOwnerPeriod> taskOwnerPeriods = new List<TaskOwnerPeriod>();
            if (_taskOwnerDays.Count == 0) return taskOwnerPeriods;

            var currentDate = _taskOwnerDays.Min(tod => tod.CurrentDate);
            var maxDate = _taskOwnerDays.Max(tod => tod.CurrentDate);
            maxDate = new DateOnly(DateHelper.GetLastDateInMonth(maxDate.Date, CultureInfo.CurrentCulture));
     
            do
            {
                TaskOwnerPeriod taskOwnerPeriod = new TaskOwnerPeriod(currentDate, null, TaskOwnerPeriodType.Month);
                taskOwnerPeriod.Lock();

                taskOwnerPeriod.AddRange(
                    _taskOwnerDays
                        .Where(t =>
                            t.CurrentDate >= taskOwnerPeriod.StartDate &&
                            t.CurrentDate <= taskOwnerPeriod.EndDate));
                
                //Only add whole months
                if (taskOwnerPeriod.TaskOwnerDayCollection.Count == CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(
                    CultureInfo.CurrentCulture.Calendar.GetYear(currentDate.Date),
                    CultureInfo.CurrentCulture.Calendar.GetMonth(currentDate.Date)))
                {
                    taskOwnerPeriods.Add(taskOwnerPeriod);
                }
                
                currentDate = new DateOnly(CultureInfo.CurrentCulture.Calendar.AddMonths(currentDate.Date, 1));
            } while (currentDate <= maxDate);

            taskOwnerPeriods.ForEach(wp => wp.Release());

            return taskOwnerPeriods;
        }


        /// <summary>
        /// Creates the whole selection taskowner period.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-25
        /// </remarks>
        public TaskOwnerPeriod CreateWholeSelectionTaskOwnerPeriod()
        {
            if (_taskOwnerDays.Count == 0) return null;

            var currentDate = _taskOwnerDays.Min(tod => tod.CurrentDate);
            TaskOwnerPeriod taskOwnerPeriod = new TaskOwnerPeriod(currentDate, _taskOwnerDays, TaskOwnerPeriodType.Other);

            return taskOwnerPeriod;
        }

        /// <summary>
        /// Splits the into weeks task owner periods.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-09
        /// </remarks>
        public IList<TaskOwnerPeriod> CreateWholeWeekTaskOwnerPeriods()
        {
            IList<TaskOwnerPeriod> taskOwnerPeriods = new List<TaskOwnerPeriod>();
            if (_taskOwnerDays.Count == 0) return taskOwnerPeriods;

            IList<DayOfWeek> daysOfWeek = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);
            var currentDate = _taskOwnerDays.Min(tod => tod.CurrentDate);
            var maxDate = _taskOwnerDays.Max(tod => tod.CurrentDate);
            maxDate = new DateOnly(DateHelper.GetLastDateInMonth(maxDate.Date, CultureInfo.CurrentCulture));
           
            do
            {
                TaskOwnerPeriod taskOwnerPeriod = new TaskOwnerPeriod(currentDate, null, TaskOwnerPeriodType.Week);
                taskOwnerPeriod.Lock();

                taskOwnerPeriod.AddRange(
                    _taskOwnerDays
                        .Where(t =>
                            t.CurrentDate >= taskOwnerPeriod.StartDate &&
                            t.CurrentDate <= taskOwnerPeriod.EndDate));

                //Only add whole weeks
                if (taskOwnerPeriod.TaskOwnerDayCollection.Count == daysOfWeek.Count)
                {
                    taskOwnerPeriods.Add(taskOwnerPeriod);
                }

                currentDate = new DateOnly(CultureInfo.CurrentCulture.Calendar.AddWeeks(currentDate.Date, 1));
            } while (currentDate <= maxDate);

            taskOwnerPeriods.ForEach(wp => wp.Release());

            return taskOwnerPeriods;
        }

        /// <summary>
        /// Creates the year task owner periods.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-04-16
        /// </remarks>
        public IList<TaskOwnerPeriod> CreateYearTaskOwnerPeriods(Calendar calendar)
        {
            if (_taskOwnerDays.Count == 0) return  new List<TaskOwnerPeriod>();

            IDictionary<int,IList<ITaskOwner>> taskOwnerDictionary = new Dictionary<int, IList<ITaskOwner>>();
            foreach (var taskOwnerDay in _taskOwnerDays)
            {
                int year = calendar.GetYear(taskOwnerDay.CurrentDate.Date);

                IList<ITaskOwner> taskOwnerDays;
                if (!taskOwnerDictionary.TryGetValue(year,out taskOwnerDays))
                    taskOwnerDictionary.Add(year,new List<ITaskOwner>{taskOwnerDay});
                else
                    taskOwnerDays.Add(taskOwnerDay);
            }

            var taskOwnerPeriods = new List<TaskOwnerPeriod>();
            foreach (var pair in taskOwnerDictionary.OrderBy(k =>k.Key))
            {
                taskOwnerPeriods.Add(new TaskOwnerPeriod(pair.Value[0].CurrentDate, pair.Value,
                                                         TaskOwnerPeriodType.Other));
            }

            taskOwnerPeriods.ForEach(wp => wp.Release());

            return taskOwnerPeriods.ToList();
        }

        /// <summary>
        /// Begins the update.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public void BeginUpdate()
        {
            _taskOwnerDays.ForEach(to => to.Lock());
        }

        /// <summary>
        /// Ends the update.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        public void EndUpdate()
        {
            _taskOwnerDays.ForEach(to => { if (to.IsLocked) to.Release(); });
        }
    }
}
