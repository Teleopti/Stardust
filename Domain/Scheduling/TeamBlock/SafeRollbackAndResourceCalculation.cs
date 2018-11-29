

using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISafeRollbackAndResourceCalculation
	{
		void Execute(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, SchedulingOptions schedulingOptions);
	}

	public class SafeRollbackAndResourceCalculation : ISafeRollbackAndResourceCalculation
	{
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;

		public SafeRollbackAndResourceCalculation(IResourceCalculation resourceOptimizationHelper, Func<ISchedulingResultStateHolder> schedulingResultStateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, SchedulingOptions schedulingOptions)
		{
			var modifyedScheduleDays = schedulePartModifyAndRollbackService.ModificationCollection.ToList();
			schedulePartModifyAndRollbackService.Rollback();
			HashSet<DateOnly> dates = new HashSet<DateOnly>();

			foreach (var modifyedScheduleDay in modifyedScheduleDays)
			{
				dates.Add(modifyedScheduleDay.DateOnlyAsPeriod.DateOnly);
			}

			IList<DateOnly> initialDates = new List<DateOnly>(dates);
			foreach (var initialDate in initialDates)
			{
				dates.Add(initialDate.AddDays(1));
			}

			var resCalcData = _schedulingResultStateHolder().ToResourceOptimizationData(schedulingOptions.ConsiderShortBreaks, false);
			foreach (var dateOnly in dates)
			{
				_resourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
			}
		}
	}
}