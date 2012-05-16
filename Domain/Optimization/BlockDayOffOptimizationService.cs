using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IBlockDayOffOptimizationService
    {
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;
		void Execute(IEnumerable<IBlockDayOffOptimizerContainer> optimizers, ISchedulingOptions schedulingOptions);
        void OnReportProgress(string message);
    }

    public class BlockDayOffOptimizationService : IBlockDayOffOptimizationService
    {
        private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
        private readonly ISchedulePartModifyAndRollbackService _rollbackService;
        private bool _cancelMe;

        public BlockDayOffOptimizationService(
            IPeriodValueCalculator periodValueCalculator,
            ISchedulePartModifyAndRollbackService rollbackService
            )
        {
            _periodValueCalculatorForAllSkills = periodValueCalculator;
            _rollbackService = rollbackService;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

		public void Execute(IEnumerable<IBlockDayOffOptimizerContainer> optimizers, ISchedulingOptions schedulingOptions)
        {
            using (PerformanceOutput.ForOperation("Optimizing days off for " + optimizers.Count() + " agents"))
            {
                executeOptimizersWhileActiveFound(optimizers, schedulingOptions);
                if (_cancelMe)
                    return;
                executeOptimizersWhileActiveFound(optimizers, schedulingOptions);
            }
        }

        public void OnReportProgress(string message)
        {
        	var handler = ReportProgress;
            if (handler != null)
            {
                var args = new ResourceOptimizerProgressEventArgs(null, 0, 0, message);
                handler(this, args);
                if (args.Cancel)
                    _cancelMe = true;
            }
        }

		/// <summary>
		/// Runs the active optimizers while at least one is active and can do more optimization.
		/// </summary>
		/// <param name="optimizers">All optimizer containers.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		private void executeOptimizersWhileActiveFound(IEnumerable<IBlockDayOffOptimizerContainer> optimizers, ISchedulingOptions schedulingOptions)
        {
            IList<IBlockDayOffOptimizerContainer> successfulContainers =
                new List<IBlockDayOffOptimizerContainer>(optimizers);

            while (successfulContainers.Count > 0)
            {
                IEnumerable<IBlockDayOffOptimizerContainer> unSuccessfulContainers =
                    shuffleAndExecuteOptimizersInList(successfulContainers, schedulingOptions);

                if (_cancelMe)
                    break;

                foreach (IBlockDayOffOptimizerContainer unSuccessfulContainer in unSuccessfulContainers)
                {
                    successfulContainers.Remove(unSuccessfulContainer);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.BlockDayOffOptimizationService.OnReportProgress(System.String)")]
		private IEnumerable<IBlockDayOffOptimizerContainer> shuffleAndExecuteOptimizersInList(ICollection<IBlockDayOffOptimizerContainer> activeOptimizers, ISchedulingOptions schedulingOptions)
        {
            IList<IBlockDayOffOptimizerContainer> retList = new List<IBlockDayOffOptimizerContainer>();
            int executes = 0;
            double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
            double newPeriodValue = lastPeriodValue;
            foreach (IBlockDayOffOptimizerContainer optimizer in activeOptimizers.GetRandom(activeOptimizers.Count, true))
            {
                _rollbackService.ClearModificationCollection();
                bool result = optimizer.Execute(schedulingOptions);
                executes++;
                if (!result)
                {
                    _rollbackService.Rollback();
                    retList.Add(optimizer);
                }
                else
                {
                    newPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
                }

                if(newPeriodValue > lastPeriodValue)
                {
                    _rollbackService.Rollback();
                    retList.Add(optimizer);
                    result = false;
                }

                string who = Resources.OptimizingDaysOff + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + optimizer.Owner.Name.ToString(NameOrderOption.FirstNameLastName);
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
