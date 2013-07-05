using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IBetweenDayOffBlockFinder
    {
        IBlockFinderResult FindValidBlockForDate(DateOnly dateOnly);
    }

    public class BetweenDayOffBlockFinder : IBlockFinder, IBetweenDayOffBlockFinder
    {
        private readonly IScheduleMatrixPro _scheduleMatrixPro;
    	private readonly IEmptyDaysInBlockOutsideSelectedHandler _emptyDaysInBlockOutsideSelectedHandler;
    	private int _lastIndex;
        private IDictionary<string, IWorkShiftFinderResult> _workShiftFinderResult = new Dictionary<string, IWorkShiftFinderResult>();

        private BetweenDayOffBlockFinder(){}

        public BetweenDayOffBlockFinder(IScheduleMatrixPro scheduleMatrixPro, IEmptyDaysInBlockOutsideSelectedHandler emptyDaysInBlockOutsideSelectedHandler)
            : this()
        {
        	_scheduleMatrixPro = scheduleMatrixPro;
        	_emptyDaysInBlockOutsideSelectedHandler = emptyDaysInBlockOutsideSelectedHandler;
        }

    	public IBlockFinderResult NextBlock()
		{
			for (int index = _lastIndex; index < _scheduleMatrixPro.FullWeeksPeriodDays.Count; index++)
			{
				if (index >= _scheduleMatrixPro.FullWeeksPeriodDays.Count)
					return new BlockFinderResult(null, new List<DateOnly>(), new Dictionary<string, IWorkShiftFinderResult>());

				IScheduleDayPro scheduleDay = _scheduleMatrixPro.FullWeeksPeriodDays[index];
				IBlockFinderResult result = FindValidBlockForDate(scheduleDay.Day);
				if (result.BlockDays.Count > 0)
				{
					IScheduleDayPro lastDay = _scheduleMatrixPro.GetScheduleDayByKey(result.BlockDays[result.BlockDays.Count - 1]);
					int newLastIndex = _scheduleMatrixPro.FullWeeksPeriodDays.IndexOf(lastDay);
					if (newLastIndex >= _lastIndex || newLastIndex == -1)
						_lastIndex = newLastIndex;
					else
					{
						continue;
					}

					if (_lastIndex == -1)
						_lastIndex = _scheduleMatrixPro.FullWeeksPeriodDays.Count;
					else
						_lastIndex += 1;

					return result;
				}

			}
			return new BlockFinderResult(null, new List<DateOnly>(), _workShiftFinderResult);
		}

        public IScheduleMatrixPro ScheduleMatrix
        {
            get { return _scheduleMatrixPro; }
        }

        public void ResetBlockPointer()
        {
            _lastIndex = 0;
        }

        public IBlockFinderResult FindBlockForDate(DateOnly dateOnly)
        {
            return findAnyBlockForDate(dateOnly, false);
        }

        public IBlockFinderResult FindValidBlockForDate(DateOnly dateOnly)
        {
            return findAnyBlockForDate(dateOnly, true);
        }

        private IBlockFinderResult findAnyBlockForDate(DateOnly dateOnly, bool validBlockOnly)
        {
            IScheduleDayPro scheduleDayPro = _scheduleMatrixPro.GetScheduleDayByKey(dateOnly);
            if (scheduleDayPro == null)
                return new BlockFinderResult(null, new List<DateOnly>(), new Dictionary<string, IWorkShiftFinderResult>());

        	SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
			if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
                return new BlockFinderResult(null, new List<DateOnly>(), new Dictionary<string, IWorkShiftFinderResult>());

            IShiftCategory foundShiftCategory = null;
            if (significant == SchedulePartView.MainShift)
                foundShiftCategory = scheduleDayPro.DaySchedulePart().PersonAssignment().ShiftCategory;

            bool emptyDayFound = false;

            DateOnly? startDate = traverse(dateOnly, -1, ref foundShiftCategory, ref emptyDayFound, validBlockOnly);
            if(!startDate.HasValue && emptyDayFound)
            {
                createWorkShiftFinderReport(dateOnly);
                return new BlockFinderResult(null, new List<DateOnly>(), _workShiftFinderResult); 
            }
                
            DateOnly? endDate = traverse(dateOnly, 1, ref foundShiftCategory, ref emptyDayFound, validBlockOnly);
            if (!endDate.HasValue && emptyDayFound)
            {
                createWorkShiftFinderReport(dateOnly);
                return new BlockFinderResult(null, new List<DateOnly>(), _workShiftFinderResult);
            }

            if (!emptyDayFound && validBlockOnly)
                return new BlockFinderResult(null, new List<DateOnly>(), new Dictionary<string, IWorkShiftFinderResult>());

            if (!startDate.HasValue || !endDate.HasValue)
                return new BlockFinderResult(null, new List<DateOnly>(), new Dictionary<string, IWorkShiftFinderResult>());

            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(startDate.Value, endDate.Value);
            IList<DateOnly> dates = new List<DateOnly>();
            bool foundEmpty = false;
            foreach (var dayInPeriod in dateOnlyPeriod.DayCollection())
            {
                scheduleDayPro = _scheduleMatrixPro.GetScheduleDayByKey(dayInPeriod);
                SchedulePartView partView = scheduleDayPro.DaySchedulePart().SignificantPart();
                if (partView != SchedulePartView.FullDayAbsence)
                    dates.Add(dayInPeriod);
                if(validBlockOnly)
                {
                    if (_scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
                    {

						if (!scheduleDayPro.DaySchedulePart().IsScheduled())
                            foundEmpty = true;
                    }
                    
                }
            }

            if (!foundEmpty && validBlockOnly)
                return new BlockFinderResult(null, new List<DateOnly>(), new Dictionary<string, IWorkShiftFinderResult>());

            if (!validBlockOnly)
                foundShiftCategory = null;
        	dates = _emptyDaysInBlockOutsideSelectedHandler.CheckDates(dates, _scheduleMatrixPro);
            return new BlockFinderResult(foundShiftCategory, dates, new Dictionary<string, IWorkShiftFinderResult>());
        }

        private void createWorkShiftFinderReport(DateOnly dateOnly)
        {
            IWorkShiftFinderResult workShiftFinderResult = new WorkShiftFinderResult(_scheduleMatrixPro.Person, dateOnly);
            workShiftFinderResult.SchedulingDateTime = DateTime.Now;
            workShiftFinderResult.Successful = false;
			workShiftFinderResult.AddFilterResults(new WorkShiftFilterResult(Resources.BlockNotValidConflictingCategories, 0, 0));
            if (!_workShiftFinderResult.ContainsKey(workShiftFinderResult.PersonDateKey))
                _workShiftFinderResult.Add(workShiftFinderResult.PersonDateKey, workShiftFinderResult);
        }


        private DateOnly? traverse(DateOnly dateOnly, int daysToAdd, ref IShiftCategory foundShiftCategory, ref bool emptyDayFound, bool validOnly)
        {
            DateOnly thisDate = dateOnly;
            IScheduleDayPro scheduleDayPro = _scheduleMatrixPro.GetScheduleDayByKey(thisDate);
            bool iFoundEmptyDay = false;
            SchedulePartView partView = scheduleDayPro.DaySchedulePart().SignificantPart();
			if (!scheduleDayPro.DaySchedulePart().IsScheduled())
                iFoundEmptyDay = true;
            if (partView == SchedulePartView.MainShift)
                foundShiftCategory = scheduleDayPro.DaySchedulePart().PersonAssignment().ShiftCategory;
            bool conflictingCategoryFound = false;
            do
            {
                thisDate = thisDate.AddDays(daysToAdd);
                scheduleDayPro = _scheduleMatrixPro.GetScheduleDayByKey(thisDate);
                if (scheduleDayPro != null)
                {
                    partView = scheduleDayPro.DaySchedulePart().SignificantPart();
					if (!scheduleDayPro.DaySchedulePart().IsScheduled())
                        iFoundEmptyDay = true;

                    if (partView == SchedulePartView.MainShift)
                    {
                        if(validOnly)
                        {
                            if (foundShiftCategory == null)
                            {
                                foundShiftCategory =
                                    scheduleDayPro.DaySchedulePart().PersonAssignment().ShiftCategory;
                            }
                            else
                            {
                                if (!foundShiftCategory.Equals(scheduleDayPro.DaySchedulePart().PersonAssignment().ShiftCategory))
                                    conflictingCategoryFound = true;
                            }
                        }
                    }
                }
			} while (scheduleDayPro != null && scheduleDayPro.DaySchedulePart().SignificantPart() != SchedulePartView.DayOff && scheduleDayPro.DaySchedulePart().SignificantPart() != SchedulePartView.ContractDayOff);
            
			if(!emptyDayFound)
                emptyDayFound = iFoundEmptyDay;

            if (conflictingCategoryFound)
                return null;

            return thisDate.AddDays(-daysToAdd);
        }
    }
}