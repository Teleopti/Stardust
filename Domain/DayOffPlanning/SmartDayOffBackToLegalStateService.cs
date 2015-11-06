using System;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public class SmartDayOffBackToLegalStateService : ISmartDayOffBackToLegalStateService
    {
        private readonly IDayOffBackToLegalStateFunctions _backToLegalStateFunctions;
        private readonly int _maxIterations;
    	private readonly IDayOffDecisionMaker _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
    	private readonly IList<string> _failedSolverDescriptionKeys = new List<string>();

		public SmartDayOffBackToLegalStateService(IDayOffBackToLegalStateFunctions backToLegalStateFunctions, int maxIterations, IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker)
        {
            _maxIterations = maxIterations;
			_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
			_backToLegalStateFunctions = backToLegalStateFunctions;
        }

        public IList<IDayOffBackToLegalStateSolver> BuildSolverList(ILockableBitArray bitArray, IDaysOffPreferences daysOffPreferences)
        {
            _backToLegalStateFunctions.WorkingArray = bitArray;
            IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
            if (daysOffPreferences.UseFullWeekendsOff)
                solvers.Add(new FreeWeekendSolver(bitArray, _backToLegalStateFunctions, daysOffPreferences, _maxIterations));
            if (daysOffPreferences.UseWeekEndDaysOff)
                solvers.Add(new FreeWeekendDaySolver(bitArray, _backToLegalStateFunctions, daysOffPreferences, _maxIterations));
            if (daysOffPreferences.UseDaysOffPerWeek)
                solvers.Add(new DaysOffPerWeekSolver(bitArray, _backToLegalStateFunctions, daysOffPreferences, _maxIterations));
            if (daysOffPreferences.UseConsecutiveDaysOff)
                solvers.Add(new ConsecutiveDaysOffSolver(bitArray, _backToLegalStateFunctions, daysOffPreferences, _maxIterations));
            if (daysOffPreferences.UseConsecutiveWorkdays)
            {
                solvers.Add(new ConsecutiveWorkdaysSolver(bitArray, _backToLegalStateFunctions, daysOffPreferences, _maxIterations));
                if (daysOffPreferences.UseWeekEndDaysOff)
					solvers.Add(new TuiCaseSolver(bitArray, _backToLegalStateFunctions, daysOffPreferences, _maxIterations, (int)DateTime.Now.TimeOfDay.TotalSeconds));
				if(daysOffPreferences.ConsecutiveWorkdaysValue.Maximum == 5)
				{
					solvers.Add(new FiveConsecutiveWorkdaysSolver(bitArray, _backToLegalStateFunctions, daysOffPreferences));
					if (daysOffPreferences.FullWeekendsOffValue == new MinMax<int>(1, 1))
					{
						solvers.Add(new CMSBCaseSolver(bitArray, _backToLegalStateFunctions, daysOffPreferences, _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker));
					}
						
				}
            }

            return solvers;
        }

        public bool Execute(IList<IDayOffBackToLegalStateSolver> solvers, int maxIterations)
        {
            bool inLegalState = false;
            int iterationCounter = 0;
            _failedSolverDescriptionKeys.Clear();

            while (!inLegalState && iterationCounter <= maxIterations)
            {
                inLegalState = true;
                foreach (var solver in solvers)
                {
                    var isSolverInLegalState = executeSolver(solver);
	                inLegalState = inLegalState && isSolverInLegalState;

					if (!isSolverInLegalState && iterationCounter == maxIterations)
						_failedSolverDescriptionKeys.Add(solver.ResolverDescriptionKey);
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

	    public IList<string> FailedSolverDescriptionKeys
        {
            get { return _failedSolverDescriptionKeys; }
        }
    }
}