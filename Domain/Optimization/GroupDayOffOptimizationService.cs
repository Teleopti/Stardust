using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupDayOffOptimizationService
    {
        /// <summary>
        /// Occurs when [report progress].
        /// </summary>
        event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        /// <summary>
        /// Executes this service.
        /// </summary>
        /// <param name="optimizers">The optimizers.</param>
        void Execute(IEnumerable<IGroupDayOffOptimizerContainer> optimizers);

        /// <summary>
        /// Called when [report progress].
        /// </summary>
        /// <param name="message">The message.</param>
        void OnReportProgress(string message);
    }

    public class GroupDayOffOptimizationService : IGroupDayOffOptimizationService
    {
        private readonly IPeriodValueCalculator _periodValueCalculatorForAllSkills;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
    	private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
    	private bool _cancelMe;

        public GroupDayOffOptimizationService(IPeriodValueCalculator periodValueCalculator, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
            IResourceOptimizationHelper resourceOptimizationHelper, IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup)
        {
            _periodValueCalculatorForAllSkills = periodValueCalculator;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _resourceOptimizationHelper = resourceOptimizationHelper;
        	_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void Execute(IEnumerable<IGroupDayOffOptimizerContainer> optimizers)
        {
            using (PerformanceOutput.ForOperation("Optimizing days off for " + optimizers.Count() + " agents"))
            {
                executeOptimizersWhileActiveFound(optimizers);
                if (_cancelMe)
                    return;
                executeOptimizersWhileActiveFound(optimizers);
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
        private void executeOptimizersWhileActiveFound(IEnumerable<IGroupDayOffOptimizerContainer> optimizers)
        {
            var successfulContainers = new List<IGroupDayOffOptimizerContainer>(optimizers);

            while (successfulContainers.Count > 0)
            {
                var unSuccessfulContainers = shuffleAndExecuteOptimizersInList(successfulContainers);

                if (_cancelMe)
                    break;

                foreach (var unSuccessfulContainer in unSuccessfulContainers)
                {
                    successfulContainers.Remove(unSuccessfulContainer);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.GroupDayOffOptimizationService.OnReportProgress(System.String)")]
        private IEnumerable<IGroupDayOffOptimizerContainer> shuffleAndExecuteOptimizersInList(ICollection<IGroupDayOffOptimizerContainer> activeOptimizers)
        {
            var retList = new List<IGroupDayOffOptimizerContainer>();
            int executes = 0;
            double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
            double newPeriodValue = lastPeriodValue;
            foreach (var optimizer in activeOptimizers.GetRandom(activeOptimizers.Count, true))
            {
                _schedulePartModifyAndRollbackService.ClearModificationCollection();
                bool result = optimizer.Execute();
                var tmpPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
                if (tmpPeriodValue > lastPeriodValue)
                    result = false;
                executes++;
                if (!result)
                {
                    using(PerformanceOutput.ForOperation("Period value for all skills was not better, resetting schedules"))
                    {
                        HashSet<DateOnly> dates = new HashSet<DateOnly>();
                        foreach (var scheduleDay in _schedulePartModifyAndRollbackService.ModificationCollection)
                        {
                            dates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
                        }
                        _schedulePartModifyAndRollbackService.Rollback();
                        // recalculate
                        foreach (var dateOnly in dates)
                        {
                        	IList<IScheduleMatrixPro> matrixesToRemove =
                        		_groupOptimizerFindMatrixesForGroup.Find(optimizer.Owner, dateOnly);
                        	foreach (var scheduleMatrixPro in matrixesToRemove)
                        	{
								foreach (var groupDayOffOptimizerContainer in activeOptimizers)
								{
									if(scheduleMatrixPro.SchedulePeriod.Equals(groupDayOffOptimizerContainer.Matrix.SchedulePeriod))
										retList.Add(groupDayOffOptimizerContainer);
								}
                        	}
                        	
                            _resourceOptimizationHelper.ResourceCalculateDate(dateOnly, false, false);
                        }
                    }
                }
                else
                {
                    newPeriodValue = tmpPeriodValue;
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
            _schedulePartModifyAndRollbackService.ClearModificationCollection();
            return retList;
        }
    }
}
