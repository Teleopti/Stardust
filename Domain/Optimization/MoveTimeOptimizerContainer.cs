using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

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
	        var cancel = false;
			var activeOptimizers = new List<IMoveTimeOptimizer>(_optimizers);
			while (activeOptimizers.Count > 0)
			{
				IEnumerable<IMoveTimeOptimizer> shuffledOptimizers = activeOptimizers.GetRandom(activeOptimizers.Count, true);

				int executes = 0;
				double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.WorkShiftOptimization);
				double newPeriodValue = lastPeriodValue;
				foreach (IMoveTimeOptimizer optimizer in shuffledOptimizers)
				{
					if (cancel) return;

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

					string unlocked = " (" +
									  (int)
									  (optimizer.Matrix.UnlockedDays.Count /
									   (double)optimizer.Matrix.EffectivePeriodDays.Length * 100) + "%) ";
					string who = Resources.OptimizingShiftLengths + Resources.Colon + "(" + activeOptimizers.Count + ")" + executes + " " + unlocked + optimizer.ContainerOwner.Name.ToString(NameOrderOption.FirstNameLastName);
					string success = !result ? " " + Resources.wasNotSuccessful : " " + Resources.wasSuccessful;
					string values = " " + newPeriodValue + "(" + (newPeriodValue - lastPeriodValue) + ")";
					var progressResult = onReportProgress(new ResourceOptimizerProgressEventArgs(0, 0, who + success + values, 10, ()=>cancel=true));
					if (cancel || progressResult.ShouldCancel) return;
				}
			}
        }

        private CancelSignal onReportProgress(ResourceOptimizerProgressEventArgs args)
        {
        	var handler = ReportProgress;
            if (handler != null)
            {
                handler(this, args);
                if (args.Cancel) return new CancelSignal{ShouldCancel = true};
            }
			return new CancelSignal();
        }
    }
}
