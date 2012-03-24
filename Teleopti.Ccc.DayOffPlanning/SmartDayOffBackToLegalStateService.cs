using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class SmartDayOffBackToLegalStateService : ISmartDayOffBackToLegalStateService
    {
        private readonly DayOffPlannerSessionRuleSet _dayOffPlannerSessionRuleSet;
        private readonly IDayOffBackToLegalStateFunctions _backToLegalStateFunctions;
        private readonly int _maxIterations;
        private readonly IList<string> _failedSolverDescriptionKeys = new List<string>();

        public SmartDayOffBackToLegalStateService(IDayOffBackToLegalStateFunctions backToLegalStateFunctions, DayOffPlannerSessionRuleSet dayOffPlannerSessionRuleSet, int maxIterations)
        {
            _maxIterations = maxIterations;
            _backToLegalStateFunctions = backToLegalStateFunctions;
            _dayOffPlannerSessionRuleSet = dayOffPlannerSessionRuleSet;
        }

        public IList<IDayOffBackToLegalStateSolver> BuildSolverList(ILockableBitArray bitArray)
        {
            _backToLegalStateFunctions.WorkingArray = bitArray;
            IList<IDayOffBackToLegalStateSolver> solvers = new List<IDayOffBackToLegalStateSolver>();
            if (_dayOffPlannerSessionRuleSet.UseFreeWeekends)
                solvers.Add(new FreeWeekendSolver(bitArray, _backToLegalStateFunctions, _dayOffPlannerSessionRuleSet, _maxIterations));
            if (_dayOffPlannerSessionRuleSet.UseFreeWeekendDays)
                solvers.Add(new FreeWeekendDaySolver(bitArray, _backToLegalStateFunctions, _dayOffPlannerSessionRuleSet, _maxIterations));
            if (_dayOffPlannerSessionRuleSet.UseDaysOffPerWeek)
                solvers.Add(new DaysOffPerWeekSolver(bitArray, _backToLegalStateFunctions, _dayOffPlannerSessionRuleSet, _maxIterations));
            if (_dayOffPlannerSessionRuleSet.UseConsecutiveDaysOff)
                solvers.Add(new ConsecutiveDaysOffSolver(bitArray, _backToLegalStateFunctions, _dayOffPlannerSessionRuleSet, _maxIterations));
            if (_dayOffPlannerSessionRuleSet.UseConsecutiveWorkdays)
            {
                solvers.Add(new ConsecutiveWorkdaysSolver(bitArray, _backToLegalStateFunctions, _dayOffPlannerSessionRuleSet, _maxIterations));
                if (_dayOffPlannerSessionRuleSet.UseFreeWeekendDays)
                    solvers.Add(new TuiCaseSolver(bitArray, _backToLegalStateFunctions, _dayOffPlannerSessionRuleSet, _maxIterations, (int)DateTime.Now.TimeOfDay.TotalSeconds));
            }

            return solvers;
        }

        public bool Execute(IList<IDayOffBackToLegalStateSolver> solvers, int maxIterations)
        {
            bool result = false;
            int iterationCounter = 0;
            _failedSolverDescriptionKeys.Clear();

            while (!result)
            {
                result = true;
                foreach (var dayOffBackToLegalStateSolver in solvers)
                {
                    // tamasb***** randomiye here solvers

                    bool result1 = !dayOffBackToLegalStateSolver.SetToFewBackToLegalState();
                    bool result2 = !dayOffBackToLegalStateSolver.SetToManyBackToLegalState();
                    result = result & result1;
                    result = result & result2;
                }

                iterationCounter++;
                if (iterationCounter > maxIterations)
                {
                    foreach (var dayOffBackToLegalStateSolver in solvers)
                    {
                        if (dayOffBackToLegalStateSolver.SetToFewBackToLegalState() || dayOffBackToLegalStateSolver.SetToManyBackToLegalState())
                            _failedSolverDescriptionKeys.Add(dayOffBackToLegalStateSolver.ResolverDescriptionKey);
                    }
                    return false;  //could not be solved
                }

            }
            return true;
        }

        public IList<string> FailedSolverDescriptionKeys
        {
            get { return _failedSolverDescriptionKeys; }
        }
    }
}