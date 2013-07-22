using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DayOffPlanning;
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
		/// <param name="useSameDaysOff">if set to <c>true</c> [use same days off].</param>
		void Execute(IEnumerable<IGroupDayOffOptimizerContainer> optimizers, bool useSameDaysOff);

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
        //private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
    	private readonly IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
        private readonly IDaysOffPreferences _daysOffPreferences;
    	private readonly IGroupDayOffOptimizationResourceHelper _groupDayOffOptimizationResourceHelper;
    	private bool _cancelMe;

        public GroupDayOffOptimizationService(IPeriodValueCalculator periodValueCalculator, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
            IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup, IDaysOffPreferences daysOffPreferences, IGroupDayOffOptimizationResourceHelper groupDayOffOptimizationResourceHelper)
        {
            _periodValueCalculatorForAllSkills = periodValueCalculator;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            //_resourceOptimizationHelper = resourceOptimizationHelper;
        	_groupOptimizerFindMatrixesForGroup = groupOptimizerFindMatrixesForGroup;
            _daysOffPreferences = daysOffPreferences;
        	_groupDayOffOptimizationResourceHelper = groupDayOffOptimizationResourceHelper;
        }

        public event EventHandler<ResourceOptimizerProgressEventArgs> ReportProgress;

        public void Execute(IEnumerable<IGroupDayOffOptimizerContainer> optimizers, bool useSameDaysOff)
        {
            using (PerformanceOutput.ForOperation("Optimizing days off for " + optimizers.Count() + " agents"))
            {
				executeOptimizersWhileActiveFound(optimizers, useSameDaysOff);
				//if (_cancelMe)
				//    return;
				//executeOptimizersWhileActiveFound(optimizers, useSameDaysOff);
            }
        }

        public void OnReportProgress(string message)
        {
        	var handler = ReportProgress;
            if (handler != null)
            {
                var args = new ResourceOptimizerProgressEventArgs(0, 0, message);
                handler(this, args);
                if (args.Cancel)
                    _cancelMe = true;
            }
        }
  
        private void executeOptimizersWhileActiveFound(IEnumerable<IGroupDayOffOptimizerContainer> optimizers, bool useSameDaysOff)
        {
            var successfulContainers = new List<IGroupDayOffOptimizerContainer>(optimizers);

            while (successfulContainers.Count > 0)
            {
				var unSuccessfulContainers = shuffleAndExecuteOptimizersInList(successfulContainers, useSameDaysOff);

                if (_cancelMe)
                    break;

                foreach (var unSuccessfulContainer in unSuccessfulContainers)
                {
                    successfulContainers.Remove(unSuccessfulContainer);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Optimization.GroupDayOffOptimizationService.OnReportProgress(System.String)")]
        private IEnumerable<IGroupDayOffOptimizerContainer> shuffleAndExecuteOptimizersInList(ICollection<IGroupDayOffOptimizerContainer> activeOptimizers, bool useSameDaysOff)
        {
            var retList = new List<IGroupDayOffOptimizerContainer>();
			var skipList = new List<IGroupDayOffOptimizerContainer>();
            int executes = 0;
            double lastPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
            double newPeriodValue = lastPeriodValue;
            var changesTracker = new LockableBitArrayChangesTracker();

            foreach (var optimizer in activeOptimizers.GetRandom(activeOptimizers.Count, true))
            {
            	

				executes++;
				if (retList.Contains(optimizer))
					continue;
				if (skipList.Contains(optimizer))
					continue;

                var converter = new ScheduleMatrixLockableBitArrayConverter(optimizer.Matrix);
                var originalArray = converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter);

                _schedulePartModifyAndRollbackService.ClearModificationCollection();
                bool result = optimizer.Execute();
                var tmpPeriodValue = _periodValueCalculatorForAllSkills.PeriodValue(IterationOperationOption.DayOffOptimization);
                if (tmpPeriodValue > lastPeriodValue)
                    result = false;
                
                if (!result)
                {
                    using(PerformanceOutput.ForOperation("Period value for all skills was not better or optimization failed, resetting schedules"))
                    {
						retList.AddRange(containersToRemove(activeOptimizers, optimizer, useSameDaysOff));
                    }
                }
                else
                {
                    newPeriodValue = tmpPeriodValue;
					skipList.AddRange(containersToSkip(activeOptimizers, optimizer, useSameDaysOff));

                    var daysOffRemoved = changesTracker.DaysOffRemoved(optimizer.WorkingBitArray, originalArray,
                                                                      optimizer.Matrix,
                                                                      _daysOffPreferences.ConsiderWeekBefore);
                    var optimizerCapture = optimizer;
                    daysOffRemoved.ForEach(d => optimizerCapture.Matrix.LockPeriod(new DateOnlyPeriod(d, d)));
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

		private IList<IGroupDayOffOptimizerContainer> containersToRemove(ICollection<IGroupDayOffOptimizerContainer> activeOptimizers, IGroupDayOffOptimizerContainer optimizer, 
			bool useSameDaysOff)
		{
			var retList = new List<IGroupDayOffOptimizerContainer>();
			retList.Add(optimizer);
			HashSet<DateOnly> dates = new HashSet<DateOnly>();

			var modifiedDays = new List<IScheduleDay>();
			var orgDays = new List<IScheduleDay>();

			foreach (var scheduleDay in _schedulePartModifyAndRollbackService.ModificationCollection)
			{
				if (!dates.Contains(scheduleDay.DateOnlyAsPeriod.DateOnly))
				{
					var scheduleDayPro = optimizer.Matrix.GetScheduleDayByKey(scheduleDay.DateOnlyAsPeriod.DateOnly);
					var orgScheduleDay = scheduleDayPro.DaySchedulePart();
					orgDays.Add(orgScheduleDay);
				}

				dates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
				modifiedDays.Add(scheduleDay);	
			}
			_schedulePartModifyAndRollbackService.Rollback();

			if(dates.Count == 0)
			{
				DateOnly date = optimizer.Matrix.EffectivePeriodDays[0].Day;
				dates.Add(date);
			}
			foreach (var dateOnly in dates)
			{
				if (useSameDaysOff)
				{
					IList<IScheduleMatrixPro> matrixesToRemove =
					_groupOptimizerFindMatrixesForGroup.Find(optimizer.Owner, dateOnly);
					foreach (var scheduleMatrixPro in matrixesToRemove)
					{
						foreach (var groupDayOffOptimizerContainer in activeOptimizers)
						{
							if (scheduleMatrixPro.SchedulePeriod.Equals(groupDayOffOptimizerContainer.Matrix.SchedulePeriod))
								retList.Add(groupDayOffOptimizerContainer);
						}
					}
				}
			}

			_groupDayOffOptimizationResourceHelper.ResourceCalculateContainersToRemove(orgDays, modifiedDays);

			return retList;
		}

		private IList<IGroupDayOffOptimizerContainer> containersToSkip(ICollection<IGroupDayOffOptimizerContainer> activeOptimizers, IGroupDayOffOptimizerContainer optimizer,
			bool useSameDaysOff)
		{
			var retList = new List<IGroupDayOffOptimizerContainer>();
			retList.Add(optimizer);
			HashSet<DateOnly> dates = new HashSet<DateOnly>();
			foreach (var scheduleDay in _schedulePartModifyAndRollbackService.ModificationCollection)
			{
				dates.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
			}
			if(dates.Count == 0)
			{
				DateOnly date = optimizer.Matrix.EffectivePeriodDays[0].Day;
				dates.Add(date);
			}
			foreach (var dateOnly in dates)
			{
				if (useSameDaysOff)
				{
					IList<IScheduleMatrixPro> matrixesToRemove =
					_groupOptimizerFindMatrixesForGroup.Find(optimizer.Owner, dateOnly);
					foreach (var scheduleMatrixPro in matrixesToRemove)
					{
						foreach (var groupDayOffOptimizerContainer in activeOptimizers)
						{
							if (scheduleMatrixPro.SchedulePeriod.Equals(groupDayOffOptimizerContainer.Matrix.SchedulePeriod))
								retList.Add(groupDayOffOptimizerContainer);
						}
					}
				}
			}

			return retList;
		}
    }
}
