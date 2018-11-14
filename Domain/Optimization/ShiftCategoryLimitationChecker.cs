using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ShiftCategoryLimitationChecker : IShiftCategoryLimitationChecker
    {
        private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
        
        public ShiftCategoryLimitationChecker(Func<ISchedulingResultStateHolder> resultStateHolder)
        {
            _resultStateHolder = resultStateHolder;
        }

        public void SetBlockedShiftCategories(SchedulingOptions optimizerPreferences, IPerson person, DateOnly dateOnly)
        {
            optimizerPreferences.NotAllowedShiftCategories.Clear();
	        if (optimizerPreferences.UseShiftCategoryLimitations)
	        {

		        setBlockedShiftCategoriesForPerson(optimizerPreferences, person, dateOnly);
	        }
        }

        private void setBlockedShiftCategoriesForPerson(SchedulingOptions optimizerPreferences, IPerson person, DateOnly dateOnly)
        {
            var schedulePeriod = person.VirtualSchedulePeriod(dateOnly);
            if (!schedulePeriod.IsValid) return;

            foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
            {
				if (shiftCategoryLimitation.Weekly)
                {
                    var firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(dateOnly, person.FirstDayOfWeek);
                    var dateOnlyWeek = new DateOnlyPeriod(firstDateInPeriodLocal, firstDateInPeriodLocal.AddDays(6));
                    if (IsShiftCategoryOverOrAtWeekLimit(shiftCategoryLimitation, _resultStateHolder().Schedules[person], dateOnlyWeek, out _))
                        optimizerPreferences.NotAllowedShiftCategories.Add(shiftCategoryLimitation.ShiftCategory);
                }
                else
                {
                    var period = schedulePeriod.DateOnlyPeriod;
                    if (IsShiftCategoryOverOrAtPeriodLimit(shiftCategoryLimitation, period, _resultStateHolder().Schedules[person], out _))
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
                throw new ArgumentNullException(nameof(shiftCategoryLimitation));
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
            foreach (var part in personRange.ScheduledDayCollection(schedulePeriodDates))
            {
                if (IsThisDayCorrectCategory(part, shiftCategoryLimitation.ShiftCategory))
                {
                    datesWithCategory.Add(part.DateOnlyAsPeriod.DateOnly);
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
                throw new ArgumentNullException(nameof(shiftCategoryLimitation));

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
            foreach (var part in personRange.ScheduledDayCollection(queryWeek))
            {
                if (IsThisDayCorrectCategory(part, shiftCategoryLimitation.ShiftCategory))
                {
                    datesWithCategory.Add(part.DateOnlyAsPeriod.DateOnly);
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