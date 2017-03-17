using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class SmartDayOffBackToLegalStateService : ISmartDayOffBackToLegalStateService
    {
    	private readonly IDayOffDecisionMaker _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;

		public SmartDayOffBackToLegalStateService(IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker)
        {
			_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
        }

        public IList<IDayOffBackToLegalStateSolver> BuildSolverList(ILockableBitArray bitArray, IDaysOffPreferences daysOffPreferences, int maxIterations)
        {
			var functions = new DayOffBackToLegalStateFunctions(bitArray);
            IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
            if (daysOffPreferences.UseFullWeekendsOff)
                solvers.Add(new FreeWeekendSolver(bitArray, functions, daysOffPreferences, maxIterations));
            if (daysOffPreferences.UseWeekEndDaysOff)
                solvers.Add(new FreeWeekendDaySolver(bitArray, functions, daysOffPreferences, maxIterations));
            if (daysOffPreferences.UseDaysOffPerWeek)
                solvers.Add(new DaysOffPerWeekSolver(bitArray, functions, daysOffPreferences, maxIterations));
            if (daysOffPreferences.UseConsecutiveDaysOff)
                solvers.Add(new ConsecutiveDaysOffSolver(bitArray, functions, daysOffPreferences, maxIterations));
            if (daysOffPreferences.UseConsecutiveWorkdays)
            {
                solvers.Add(new ConsecutiveWorkdaysSolver(bitArray, functions, daysOffPreferences, maxIterations));
                if (daysOffPreferences.UseWeekEndDaysOff)
					solvers.Add(new TuiCaseSolver(bitArray, functions, daysOffPreferences, maxIterations, (int)DateTime.Now.TimeOfDay.TotalSeconds));
				if(daysOffPreferences.ConsecutiveWorkdaysValue.Maximum == 5)
				{
					solvers.Add(new FiveConsecutiveWorkdaysSolver(bitArray, functions, daysOffPreferences));
					if (daysOffPreferences.FullWeekendsOffValue == new MinMax<int>(1, 1))
					{
						solvers.Add(new CMSBCaseSolver(bitArray, functions, daysOffPreferences, _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker));
					}
						
				}
            }

            return solvers;
        }

        public bool Execute(IList<IDayOffBackToLegalStateSolver> solvers, int maxIterations, ICollection<string> failedSolverDescriptionKeys)
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

					if (!isSolverInLegalState && iterationCounter == maxIterations)
						failedSolverDescriptionKeys.Add(solver.ResolverDescriptionKey);
                }
                iterationCounter++;
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