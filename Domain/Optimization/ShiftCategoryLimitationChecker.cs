using System;
using System.Collections.Generic;
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

        public void SetBlockedShiftCategories(ISchedulingOptions optimizerPreferences, IPerson person, DateOnly dateOnly)
        {
            optimizerPreferences.NotAllowedShiftCategories.Clear();
            if (optimizerPreferences.UseShiftCategoryLimitations)
            {
                IVirtualSchedulePeriod schedulePeriod = person.VirtualSchedulePeriod(dateOnly);
                if (!schedulePeriod.IsValid)
                    return;

                foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
                {
                    IList<DateOnly> datesWithCategory;
                    if(shiftCategoryLimitation.Weekly)
                    {
						var firstDateInPeriodLocal = new DateOnly(DateHelper.GetFirstDateInWeek(dateOnly, person.FirstDayOfWeek));
						var dateOnlyWeek = new DateOnlyPeriod(firstDateInPeriodLocal, firstDateInPeriodLocal.AddDays(6));
                        if (IsShiftCategoryOverOrAtWeekLimit(shiftCategoryLimitation, ScheduleDictionary[person], dateOnlyWeek, out datesWithCategory))
                            optimizerPreferences.NotAllowedShiftCategories.Add(shiftCategoryLimitation.ShiftCategory);
                    }
                    else
                    {
						DateOnlyPeriod period = schedulePeriod.DateOnlyPeriod;
                        if (IsShiftCategoryOverOrAtPeriodLimit(shiftCategoryLimitation, period, ScheduleDictionary[person], out datesWithCategory))
                            optimizerPreferences.NotAllowedShiftCategories.Add(shiftCategoryLimitation.ShiftCategory);
                    }
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
                if (part.AssignmentHighZOrder().MainShift.ShiftCategory.Equals(shiftCategory))
                    return true;
            }

            return false;
        }
    }
}