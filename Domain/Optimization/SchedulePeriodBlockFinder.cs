using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class SchedulePeriodBlockFinder : IBlockFinder
    {
        private readonly IScheduleMatrixPro _scheduleMatrixPro;
    	private readonly IEmptyDaysInBlockOutsideSelectedHandler _emptyDaysInBlockOutsideSelectedHandler;
    	private bool _blockFound;
        private readonly IDictionary<string, IWorkShiftFinderResult> _workShiftFinderResult = new Dictionary<string, IWorkShiftFinderResult>();

        private SchedulePeriodBlockFinder(){}

		public SchedulePeriodBlockFinder(IScheduleMatrixPro scheduleMatrixPro, IEmptyDaysInBlockOutsideSelectedHandler emptyDaysInBlockOutsideSelectedHandler)
			: this()
		{
			_scheduleMatrixPro = scheduleMatrixPro;
			_emptyDaysInBlockOutsideSelectedHandler = emptyDaysInBlockOutsideSelectedHandler;
		}

    	public IBlockFinderResult NextBlock()
        {
            if(_blockFound)
                return new BlockFinderResult(null, new List<DateOnly>(), new Dictionary<string, IWorkShiftFinderResult>());

            IList<DateOnly> retList = new List<DateOnly>();
            IShiftCategory foundCategory = null;
            bool foundEmpty = false;
            bool foundConflictingCategory = false;
            foreach (var scheduleDayPro in _scheduleMatrixPro.EffectivePeriodDays)
            {
                SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
                if (!scheduleDayPro.DaySchedulePart().IsScheduled())
                {
                    retList.Add(scheduleDayPro.Day);
                    foundEmpty = true;
                }

                if (significant == SchedulePartView.MainShift)
                {
                    IShiftCategory shiftCategory =
                        scheduleDayPro.DaySchedulePart().PersonAssignment().ShiftCategory;

                    if(foundCategory != null)
                    {
                        if(!foundCategory.Equals(shiftCategory))
                        {  
                            foundConflictingCategory = true;
                        }     
                    }
                    else
                    {
                        foundCategory = shiftCategory;
                    }
                    if (foundConflictingCategory)
                        createWorkShiftFinderReport(scheduleDayPro.Day);
                }
                
            }
            if (foundConflictingCategory && foundEmpty)
                return new BlockFinderResult(null, new List<DateOnly>(), _workShiftFinderResult);

			retList = _emptyDaysInBlockOutsideSelectedHandler.CheckDates(retList, _scheduleMatrixPro);

            if (!foundEmpty)
                return new BlockFinderResult(null, retList, new Dictionary<string, IWorkShiftFinderResult>());
            
            _blockFound = true;
            return new BlockFinderResult(foundCategory, retList, new Dictionary<string, IWorkShiftFinderResult>());
        }

        public IScheduleMatrixPro ScheduleMatrix
        {
            get { return _scheduleMatrixPro; }
        }

        public void ResetBlockPointer()
        {
            _blockFound = false;
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
    }
}