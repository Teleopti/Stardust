using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;
using Teleopti.Ccc.Secrets.DayOffPlanning;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class SmartDayOffBackToLegalStateService : ISmartDayOffBackToLegalStateService
    {
    	private readonly IDayOffDecisionMaker _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
		private readonly MatrixClosedDayLocker _matrixClosedDayLocker;
		private const int solverAndExecuteMaxIterations = 100;

		public SmartDayOffBackToLegalStateService(IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker, MatrixClosedDayLocker matrixClosedDayLocker)
		{
			_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
			_matrixClosedDayLocker = matrixClosedDayLocker;
		}

		
		public bool Execute(ISchedulingResultStateHolder schedulingResultStateHolder, IVirtualSchedulePeriod schedulePeriod, ILockableBitArray bitArray, IDaysOffPreferences daysOffPreferences)
		{
			var clonedArrayWithLocksOnClosedDays = (ILockableBitArray)bitArray.Clone();
			_matrixClosedDayLocker.Execute(clonedArrayWithLocksOnClosedDays, schedulePeriod,
				schedulingResultStateHolder);
			var solvers = buildSolverList(daysOffPreferences, clonedArrayWithLocksOnClosedDays);
            bool inLegalState = false;
            int iterationCounter = 0;
            while (!inLegalState && iterationCounter <= solverAndExecuteMaxIterations)
            {
                inLegalState = true;
                foreach (var solver in solvers)
                {
                    var isSolverInLegalState = executeSolver(solver);
	                inLegalState = inLegalState && isSolverInLegalState;
				}
                iterationCounter++;
            }

			for (int i = 0; i < clonedArrayWithLocksOnClosedDays.DaysOffBitArray.Length; i++)
			{
				if(!bitArray.IsLocked(i, true))
				{
					bitArray.Set(i, clonedArrayWithLocksOnClosedDays.DaysOffBitArray[i]);
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

		private IList<IDayOffBackToLegalStateSolver> buildSolverList(IDaysOffPreferences daysOffPreferences, ILockableBitArray clonedArrayWithLocksOnClosedDays)
		{
			var functions = new DayOffBackToLegalStateFunctions(clonedArrayWithLocksOnClosedDays);
			IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
			if (daysOffPreferences.UseFullWeekendsOff)
				solvers.Add(new FreeWeekendSolver(clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences,
					solverAndExecuteMaxIterations));
			if (daysOffPreferences.UseWeekEndDaysOff)
				solvers.Add(new FreeWeekendDaySolver(clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences,
					solverAndExecuteMaxIterations));
			if (daysOffPreferences.UseDaysOffPerWeek)
				solvers.Add(new DaysOffPerWeekSolver(clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences,
					solverAndExecuteMaxIterations));
			if (daysOffPreferences.UseConsecutiveDaysOff)
				solvers.Add(new ConsecutiveDaysOffSolver(clonedArrayWithLocksOnClosedDays, functions,
					daysOffPreferences, solverAndExecuteMaxIterations));
			if (daysOffPreferences.UseConsecutiveWorkdays)
			{
				solvers.Add(new ConsecutiveWorkdaysSolver(clonedArrayWithLocksOnClosedDays, functions,
					daysOffPreferences, solverAndExecuteMaxIterations));
				if (daysOffPreferences.UseWeekEndDaysOff)
					solvers.Add(new TuiCaseSolver(clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences,
						solverAndExecuteMaxIterations, (int)DateTime.Now.TimeOfDay.TotalSeconds));
				if (daysOffPreferences.ConsecutiveWorkdaysValue.Maximum == 5)
				{
					solvers.Add(new FiveConsecutiveWorkdaysSolver(clonedArrayWithLocksOnClosedDays, functions,
						daysOffPreferences));
					if (daysOffPreferences.FullWeekendsOffValue == new MinMax<int>(1, 1))
					{
						solvers.Add(new CMSBCaseSolver(clonedArrayWithLocksOnClosedDays, functions, daysOffPreferences,
							_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker));
					}

				}
			}

			return solvers;
		}

	}
}