﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IExtendReduceDaysOffOptimizerService
    {
        /// <summary>
        /// Occurs when [report progress].
        /// </summary>
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        /// <summary>
        /// Executes this service.
        /// </summary>
        /// <param name="optimizers">The optimizers.</param>
        void Execute(IEnumerable<IExtendReduceDaysOffOptimizer> optimizers);

        /// <summary>
        /// Called when [report progress].
        /// </summary>
        /// <param name="message">The message.</param>
        void OnReportProgress(string message);
    }

    public class ExtendReduceDaysOffOptimizerService : IExtendReduceDaysOffOptimizerService
    {
         private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
    	private bool _cancelMe;

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;



        public ExtendReduceDaysOffOptimizerService(IPeriodValueCalculator periodValueCalculatorForAllSkills)
        {
            _periodValueCalculatorForAllSkills = periodValueCalculatorForAllSkills;
        }

        public void Execute(IEnumerable<IExtendReduceDaysOffOptimizer> optimizers)
        {
            using (PerformanceOutput.ForOperation("Extending and reduces time for " + optimizers.Count() + " agents"))
            {
                if (_cancelMe)
                    return;

                executeOptimizersWhileActiveFound(optimizers);
            }
        }

        public void OnReportProgress(string message)
        {
            if (ReportProgress != null)
            {
                var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
                ReportProgress(this, args);
                if (args.Cancel)
                    _cancelMe = true;
            }
        }

        private void executeOptimizersWhileActiveFound(IEnumerable<IExtendReduceDaysOffOptimizer> optimizers)
        {
            IList<IExtendReduceDaysOffOptimizer> successfulContainers =
                new List<IExtendReduceDaysOffOptimizer>(optimizers);

            while (successfulContainers.Count > 0)
            {
                IList<IExtendReduceDaysOffOptimizer> unSuccessfulContainers =
                    shuffleAndExecuteOptimizersInList(successfulContainers);

                if (_cancelMe)
                    break;

                foreach (IExtendReduceDaysOffOptimizer unSuccessfulContainer in unSuccessfulContainers)
                {
                    successfulContainers.Remove(unSuccessfulContainer);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.ExtendReduceDaysOffOptimizerService.OnReportProgress(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.ExtendReduceTimeOptimizerService.OnReportProgress(System.String)")]
        private IList<IExtendReduceDaysOffOptimizer> shuffleAndExecuteOptimizersInList(ICollection<IExtendReduceDaysOffOptimizer> activeOptimizers)
        {
            IList<IExtendReduceDaysOffOptimizer> retList = new List<IExtendReduceDaysOffOptimizer>();
            int executes = 0;
            double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
            double newPeriodValue = lastPeriodValue;
            foreach (IExtendReduceDaysOffOptimizer optimizer in activeOptimizers.GetRandom(activeOptimizers.Count, true))
            {
                bool result = optimizer.Execute();
                executes++;
                if (!result)
                {
                    retList.Add(optimizer);
                }
                else
                {
                    newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
                }

                string who = Resources.ExtendingAndReducingDaysoff + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + optimizer.Owner.Name.ToString(NameOrderOption.FirstNameLastName);
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

                lastPeriodValue = newPeriodValue;
                if (_cancelMe)
                    break;
            }
            return retList;
        }
    }
}