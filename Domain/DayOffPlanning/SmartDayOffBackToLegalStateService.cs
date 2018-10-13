using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class SmartDayOffBackToLegalStateService : ISmartDayOffBackToLegalStateService
    {
    	private readonly IDayOffDecisionMaker _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
		private readonly IMatrixClosedDayLocker _matrixClosedDayLocker;
		private ILockableBitArray _bitArray;
		private ILockableBitArray _clonedArrayWithLocksOnClosedDays;

		public SmartDayOffBackToLegalStateService(IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker, IMatrixClosedDayLocker matrixClosedDayLocker)
		{
			_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
			_matrixClosedDayLocker = matrixClosedDayLocker;
			_bitArray = null;
			_clonedArrayWithLocksOnClosedDays = null;
		}

        public IList<IDayOffBackToLegalStateSolver> BuildSolverList(ISchedulingResultStateHolder schedulingResultStateHolder, IVirtualSchedulePeriod schedulePeriod, ILockableBitArray bitArray, IDaysOffPreferences daysOffPreferences, int maxIterations)
		{
			_bitArray = bitArray;
			_clonedArrayWithLocksOnClosedDays = (ILockableBitArray)_bitArray.Clone();
			_matrixClosedDayLocker.Execute(_clonedArrayWithLocksOnClosedDays, schedulePeriod, schedulingResultStateHolder);

			var functions = new DayOffBackToLegalStateFunctions(_clonedArrayWithLocksOnClosedDays);
            IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
            if (daysOffPreferences.UseFullWeekendsOff)
                solvers.Add(new FreeWeekendSolver(_clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences, maxIterations));
            if (daysOffPreferences.UseWeekEndDaysOff)
                solvers.Add(new FreeWeekendDaySolver(_clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences, maxIterations));
            if (daysOffPreferences.UseDaysOffPerWeek)
                solvers.Add(new DaysOffPerWeekSolver(_clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences, maxIterations));
            if (daysOffPreferences.UseConsecutiveDaysOff)
                solvers.Add(new ConsecutiveDaysOffSolver(_clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences, maxIterations));
            if (daysOffPreferences.UseConsecutiveWorkdays)
            {
                solvers.Add(new ConsecutiveWorkdaysSolver(_clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences, maxIterations));
                if (daysOffPreferences.UseWeekEndDaysOff)
					solvers.Add(new TuiCaseSolver(_clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences, maxIterations, (int)DateTime.Now.TimeOfDay.TotalSeconds));
				if(daysOffPreferences.ConsecutiveWorkdaysValue.Maximum == 5)
				{
					solvers.Add(new FiveConsecutiveWorkdaysSolver(_clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences));
					if (daysOffPreferences.FullWeekendsOffValue == new MinMax<int>(1, 1))
					{
						solvers.Add(new CMSBCaseSolver(_clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences, _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker));
					}
						
				}
            }

            return solvers;
        }

        public bool Execute(IList<IDayOffBackToLegalStateSolver> solvers, int maxIterations)
        {
            bool inLegalState = false;
            int iterationCounter = 0;
            while (!inLegalState && iterationCounter <= maxIterations)
            {
                inLegalState = true;
                foreach (var solver in solvers)
                {
                    var isSolverInLegalState = executeSolver(solver);
	                inLegalState = inLegalState && isSolverInLegalState;
				}
                iterationCounter++;
            }

			for (int i = 0; i < _clonedArrayWithLocksOnClosedDays.DaysOffBitArray.Length; i++)
			{
				if(!_bitArray.IsLocked(i, true))
				{
					_bitArray.Set(i, _clonedArrayWithLocksOnClosedDays.DaysOffBitArray[i]);
				}
			}

			return inLegalState;
        }

	    private static bool executeSolver(IDayOffBackToLegalStateSolver solver)
	    {
		    bool isTooFewInLegalState = !solver.SetToFewBackToLegalState();
		    bool isTooManyInLegalState = !solver.SetToManyBackToLegalState();
		    bool isSolverInLegalState = isTooFewInLegalState && isTooManyInLegalState;
		    return isSolverInLegalState;
	    }
	}
}