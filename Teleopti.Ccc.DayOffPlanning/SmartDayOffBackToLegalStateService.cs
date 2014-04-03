using System;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class SmartDayOffBackToLegalStateService : ISmartDayOffBackToLegalStateService
    {
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffBackToLegalStateFunctions _backToLegalStateFunctions;
        private readonly int _maxIterations;
    	private readonly IDayOffDecisionMaker _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
    	private readonly IList<string> _failedSolverDescriptionKeys = new List<string>();

		public SmartDayOffBackToLegalStateService(IDayOffBackToLegalStateFunctions backToLegalStateFunctions, IDaysOffPreferences daysOffPreferences, int maxIterations, IDayOffDecisionMaker cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker)
        {
            _maxIterations = maxIterations;
			_cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker = cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker;
			_backToLegalStateFunctions = backToLegalStateFunctions;
            _daysOffPreferences = daysOffPreferences;
        }

        public IList<IDayOffBackToLegalStateSolver> BuildSolverList(ILockableBitArray bitArray)
        {
            _backToLegalStateFunctions.WorkingArray = bitArray;
            IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
            if (_daysOffPreferences.UseFullWeekendsOff)
                solvers.Add(new FreeWeekendSolver(bitArray, _backToLegalStateFunctions, _daysOffPreferences, _maxIterations));
            if (_daysOffPreferences.UseWeekEndDaysOff)
                solvers.Add(new FreeWeekendDaySolver(bitArray, _backToLegalStateFunctions, _daysOffPreferences, _maxIterations));
            if (_daysOffPreferences.UseDaysOffPerWeek)
                solvers.Add(new DaysOffPerWeekSolver(bitArray, _backToLegalStateFunctions, _daysOffPreferences, _maxIterations));
            if (_daysOffPreferences.UseConsecutiveDaysOff)
                solvers.Add(new ConsecutiveDaysOffSolver(bitArray, _backToLegalStateFunctions, _daysOffPreferences, _maxIterations));
            if (_daysOffPreferences.UseConsecutiveWorkdays)
            {
                solvers.Add(new ConsecutiveWorkdaysSolver(bitArray, _backToLegalStateFunctions, _daysOffPreferences, _maxIterations));
                if (_daysOffPreferences.UseWeekEndDaysOff)
					solvers.Add(new TuiCaseSolver(bitArray, _backToLegalStateFunctions, _daysOffPreferences, _maxIterations, (int)DateTime.Now.TimeOfDay.TotalSeconds));
				if(_daysOffPreferences.ConsecutiveWorkdaysValue.Maximum == 5)
				{
					solvers.Add(new FiveConsecutiveWorkdaysSolver(bitArray, _backToLegalStateFunctions, _daysOffPreferences));
					if (_daysOffPreferences.FullWeekendsOffValue == new MinMax<int>(1, 1))
					{
						solvers.Add(new CMSBCaseSolver(bitArray, _backToLegalStateFunctions, _daysOffPreferences, _cmsbOneFreeWeekendMax5WorkingDaysDecisionMaker));
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