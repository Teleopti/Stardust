using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ShiftCategoryLimitationChecker : IShiftCategoryLimitationChecker
    {
        private readonly ISchedulingResultStateHolder _resultStateHolder;
        
        private ShiftCategoryLimitationChecker(){}

        public ShiftCategoryLimitationChecker(ISchedulingResultStateHolder resultStateHolder) :this()
        {
            _resultStateHolder = resultStateHolder;
        }

        private IScheduleDictionary ScheduleDictionary { get { return _resultStateHolder.Schedules; } }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public void SetBlockedShiftCategories(ISchedulingOptions optimizerPreferences, IPerson person, DateOnly dateOnly)
        {
            optimizerPreferences.NotAllowedShiftCategories.Clear();
            if (optimizerPreferences.UseShiftCategoryLimitations)
            {
                if(person is IGroupPerson )
                    setBlockedShiftCategoriesForGroupPerson(optimizerPreferences,(IGroupPerson ) person, dateOnly);
                else
                    setBlockedShiftCategoriesForPerson(optimizerPreferences, person, dateOnly);
            }
        }

        private void setBlockedShiftCategoriesForGroupPerson(ISchedulingOptions optimizerPreferences, IGroupPerson groupPerson, DateOnly dateOnly)
        {
            foreach(var person in groupPerson.GroupMembers  )
            {
                IVirtualSchedulePeriod schedulePeriod = person.VirtualSchedulePeriod(dateOnly);
                if (!schedulePeriod.IsValid)
                    return;
                foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
                {
                    IList<DateOnly> datesWithCategory;
                    if (shiftCategoryLimitation.Weekly)
                    {
                        var firstDateInPeriodLocal = new DateOnly(DateHelper.GetFirstDateInWeek(dateOnly, person.FirstDayOfWeek));
                        var dateOnlyWeek = new DateOnlyPeriod(firstDateInPeriodLocal, firstDateInPeriodLocal.AddDays(6));

                        if (IsShiftCategoryOverOrAtWeekLimit(shiftCategoryLimitation, ScheduleDictionary[person], dateOnlyWeek, out datesWithCategory))
                        {
                            fillNotAllowedShiftCategories(optimizerPreferences, groupPerson, dateOnly, shiftCategoryLimitation, person);
                        }
                    }
                    else
                    {
                        DateOnlyPeriod period = schedulePeriod.DateOnlyPeriod;
                        if (IsShiftCategoryOverOrAtPeriodLimit(shiftCategoryLimitation, period, ScheduleDictionary[person], out datesWithCategory))
                        {
                            fillNotAllowedShiftCategories(optimizerPreferences, groupPerson, dateOnly, shiftCategoryLimitation, person);
                        }
                    }
                }
            }
            
           
        }

        private void fillNotAllowedShiftCategories(ISchedulingOptions optimizerPreferences, IGroupPerson groupPerson,
                                                   DateOnly dateOnly, IShiftCategoryLimitation shiftCategoryLimitation,
                                                   IPerson person)
        {
            var currentPerson = person;
            var restMemebers = groupPerson.GroupMembers.Where(x => x != currentPerson);
            if ((restMemebers.All(
                x => ScheduleDictionary[x].ScheduledDay(dateOnly).IsScheduled())
                 ||
                 groupPerson.GroupMembers.All(
                     x => !ScheduleDictionary[x].ScheduledDay(dateOnly).IsScheduled())
                ) &&
                !optimizerPreferences.NotAllowedShiftCategories.Contains(
                    shiftCategoryLimitation.ShiftCategory))
            {
                optimizerPreferences.NotAllowedShiftCategories.Add(
                    shiftCategoryLimitation.ShiftCategory);
            }
        }

        private void setBlockedShiftCategoriesForPerson(ISchedulingOptions optimizerPreferences, IPerson person,
                                                        DateOnly dateOnly)
        {
            IVirtualSchedulePeriod schedulePeriod = person.VirtualSchedulePeriod(dateOnly);
            if (!schedulePeriod.IsValid)
                return;

            foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
            {
                IList<DateOnly> datesWithCategory;
                if (shiftCategoryLimitation.Weekly)
                {
                    var firstDateInPeriodLocal = new DateOnly(DateHelper.GetFirstDateInWeek(dateOnly, person.FirstDayOfWeek));
                    var dateOnlyWeek = new DateOnlyPeriod(firstDateInPeriodLocal, firstDateInPeriodLocal.AddDays(6));
                    if (IsShiftCategoryOverOrAtWeekLimit(shiftCategoryLimitation, ScheduleDictionary[person], dateOnlyWeek,
                                                         out datesWithCategory))
                        optimizerPreferences.NotAllowedShiftCategories.Add(shiftCategoryLimitation.ShiftCategory);
                }
                else
                {
                    DateOnlyPeriod period = schedulePeriod.DateOnlyPeriod;
                    if (IsShiftCategoryOverOrAtPeriodLimit(shiftCategoryLimitation, period, ScheduleDictionary[person],
                                                           out datesWithCategory))
                        optimizerPreferences.NotAllowedShiftCategories.Add(shiftCategoryLimitation.ShiftCategory);
                }
            }
        }

        public bool IsShiftCategoryOverPeriodLimit(IShiftCategoryLimitation shiftCategoryLimitation, DateOnlyPeriod schedulePeriodDates, IScheduleRange personRange, out IList<DateOnly> datesWithCategory)
        {
            if (shiftCategoryLimitation.Weekly)
                throw new ArgumentException("shiftCategoryLimitation.Weekly must be not true");

            int categoryCounter = countCategoriesInPeriod(shiftCategoryLimitation, schedulePeriodDates, personRange, out datesWithCategory);

            return (categoryCounter > shiftCategoryLimitation.MaxNumberOf);
        }

        public bool IsShiftCategoryOverWeekLimit(IShiftCategoryLimitation shiftCategoryLimitation, IScheduleRange personRange, DateOnlyPeriod queryWeek, out IList<DateOnly> datesWithCategory)
        {
            if (shiftCategoryLimitation == null)
                throw new ArgumentNullException("shiftCategoryLimitation");
            if (!shiftCategoryLimitation.Weekly)
                throw new ArgumentException("shiftCategoryLimitation.Weekly must be true");

            int categoryCounter = countCategoriesInWeek(shiftCategoryLimitation, personRange, queryWeek, out datesWithCategory);

            return categoryCounter > shiftCategoryLimitation.MaxNumberOf;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        public static bool IsShiftCategoryOverOrAtPeriodLimit(IShiftCategoryLimitation shiftCategoryLimitation, DateOnlyPeriod schedulePeriodDates, IScheduleRange personRange, out IList<DateOnly> datesWithCategory)
        {
            if (shiftCategoryLimitation.Weekly)
                throw new ArgumentException("shiftCategoryLimitation.Weekly must not be true");

            int categoryCounter = countCategoriesInPeriod(shiftCategoryLimitation, schedulePeriodDates, personRange, out datesWithCategory);

            return (categoryCounter >= shiftCategoryLimitation.MaxNumberOf);
        }

        private static int countCategoriesInPeriod(IShiftCategoryLimitation shiftCategoryLimitation, DateOnlyPeriod schedulePeriodDates, IScheduleRange personRange, out IList<DateOnly> datesWithCategory)
        {
            int categoryCounter = 0;
            datesWithCategory = new List<DateOnly>();
            foreach (var dateOnly in schedulePeriodDates.DayCollection())
            {
                IScheduleDay part = personRange.ScheduledDay(dateOnly);
                if (IsThisDayCorrectCategory(part, shiftCategoryLimitation.ShiftCategory))
                {
                    datesWithCategory.Add(dateOnly);
                    categoryCounter++;
                }
            }
            return categoryCounter;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        public static bool IsShiftCategoryOverOrAtWeekLimit(IShiftCategoryLimitation shiftCategoryLimitation, 
            IScheduleRange personRange, DateOnlyPeriod queryWeek, out IList<DateOnly> datesWithCategory)
        {
            if (shiftCategoryLimitation == null)
                throw new ArgumentNullException("shiftCategoryLimitation");

            if (!shiftCategoryLimitation.Weekly)
                throw new ArgumentException("shiftCategoryLimitation.Weekly must be true");

            int categoryCounter = countCategoriesInWeek(shiftCategoryLimitation,
                                                        personRange, queryWeek, out datesWithCategory);

            return categoryCounter >= shiftCategoryLimitation.MaxNumberOf;

        }

        private static int countCategoriesInWeek(IShiftCategoryLimitation shiftCategoryLimitation,  IScheduleRange personRange, DateOnlyPeriod queryWeek, out IList<DateOnly> datesWithCategory)
        {
            datesWithCategory = new List<DateOnly>();
            int categoryCounter = 0;
            foreach (var dateOnly in queryWeek.DayCollection())
            {
                IScheduleDay part = personRange.ScheduledDay(dateOnly);
                if (IsThisDayCorrectCategory(part, shiftCategoryLimitation.ShiftCategory))
                {
                    datesWithCategory.Add(dateOnly);
                    categoryCounter++;
                }
            }

            return categoryCounter;
        }

        public static bool IsThisDayCorrectCategory(IScheduleDay part, IEntity shiftCategory)
        {
            if (part.SignificantPart() == SchedulePartView.MainShift)
            {
                if (part.PersonAssignment().ShiftCategory.Equals(shiftCategory))
                    return true;
            }

            return false;
        }
    }
}