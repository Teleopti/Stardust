﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Contains the logic of the move time optimization iteration for one matrix.
    /// - Order the list of MoveTimeOptimizers
    /// - Manages the list of MoveTimeOptimizers according to the result of Optimizers
    /// </summary>
    public class MoveTimeOptimizerContainer : IScheduleOptimizationService
    {
        private readonly IList<IMoveTimeOptimizer> _optimizers;
        private bool _cancelMe;
        private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;

        public MoveTimeOptimizerContainer(IList<IMoveTimeOptimizer> optimizers,
            IPeriodValueCalculator periodValueCalculatorForAllSkills)
        {
            _optimizers = optimizers;
            _periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void Execute()
        {
            if (_cancelMe)
                return;

            executeOptimizersWhileActiveFound(_optimizers);
        }

        public void OnReportProgress(string message)
        {
        	var handler = ReportProgress;
            if (handler != null)
            {
                ResourceOptimizerProgressEventArgs args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
                handler(this, args);
                if (args.Cancel)
                    _cancelMe = true;
            }
        }

        /// <summary>
        /// Runs the active optimizers while at least one is active and can do more optimization.
        /// </summary>
        /// <param name="optimizers">All optimizer containers.</param>
        private void executeOptimizersWhileActiveFound(IEnumerable<IMoveTimeOptimizer> optimizers)
        {
            IList<IMoveTimeOptimizer> activeOptimizers =
                new List<IMoveTimeOptimizer>(optimizers);

            while (activeOptimizers.Count > 0)
            {
                if (_cancelMe)
                    break;

                IEnumerable<IMoveTimeOptimizer> shuffledOptimizers = activeOptimizers.GetRandom(activeOptimizers.Count, true);

                int executes = 0;
                double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization);
                double newPeriodValue = lastPeriodValue;
                foreach (IMoveTimeOptimizer optimizer in shuffledOptimizers)
                {
                    if (_cancelMe)
                        break;

                    executes++;
                    bool result = optimizer.Execute();

                    if (!result)
                    {
                        activeOptimizers.Remove(optimizer);
                    }
                    else
                    {
                        newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization);
                    }
                    
                    string who = Resources.OptimizingShiftLengths + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + optimizer.ContainerOwner.Name.ToString(NameOrderOption.FirstNameLastName);
                    string success;
                    if (!result)
                    {
                        success = " " + Resources.wasNotSuccessful;
                    }
                    else
                    {
                        success = " " + Resources.wasSuccessful;
                    }
                    string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ")";
                    OnReportProgress(who + success + values);
                }
            }
        }
    }
}
