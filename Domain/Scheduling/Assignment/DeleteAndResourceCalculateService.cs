using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IDeleteAndResourceCalculateService
	{
		void DeleteWithResourceCalculation(IEnumerable<IScheduleDay> daysToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation);
		void DeleteWithResourceCalculation(IScheduleDay dayToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation);
	}

	public class DeleteAndResourceCalculateService : IDeleteAndResourceCalculateService
	{
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IResourceCalculateDaysDecider _resourceCalculateDaysDecider;
		private readonly IResourceCalculateAfterDeleteDecider _resourceCalculateAfterDeleteDecider;

		public DeleteAndResourceCalculateService(Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
																					IDeleteSchedulePartService deleteSchedulePartService, 
																					IResourceOptimizationHelper resourceOptimizationHelper,
																					IResourceCalculateDaysDecider resourceCalculateDaysDecider,
																					IResourceCalculateAfterDeleteDecider resourceCalculateAfterDeleteDecider)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_resourceCalculateDaysDecider = resourceCalculateDaysDecider;
			_resourceCalculateAfterDeleteDecider = resourceCalculateAfterDeleteDecider;
		}

		public void DeleteWithResourceCalculation(IEnumerable<IScheduleDay> daysToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			_deleteSchedulePartService.Delete(daysToDelete, rollbackService);
			resourceCalculate(daysToDelete, considerShortBreaks, doIntraIntervalCalculation);
		}

		public void DeleteWithResourceCalculation(IScheduleDay dayToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			_deleteSchedulePartService.Delete(new []{ dayToDelete}, rollbackService);

			if (_resourceCalculateAfterDeleteDecider.DoCalculation(dayToDelete.Person, dayToDelete.DateOnlyAsPeriod.DateOnly))
			{
				var resCalcData = _schedulingResultStateHolder().ToResourceOptimizationData(considerShortBreaks, doIntraIntervalCalculation);
				var date = dayToDelete.DateOnlyAsPeriod.DateOnly;
				_resourceOptimizationHelper.ResourceCalculate(date, resCalcData);
				if (_resourceCalculateDaysDecider.IsNightShift(dayToDelete))
				{
					_resourceOptimizationHelper.ResourceCalculate(date.AddDays(1), resCalcData);
				}
			}
		}

		private void resourceCalculate(IEnumerable<IScheduleDay> daysToDelete, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			var dic = new Dictionary<DateOnly, IList<IScheduleDay>>();
			foreach (var scheduleDay in daysToDelete)
			{
				var key = scheduleDay.DateOnlyAsPeriod.DateOnly;
				IList<IScheduleDay> value;
				if (!dic.TryGetValue(key, out value))
				{
					value = new List<IScheduleDay>();
					dic.Add(key, value);
				}
				value.Add(scheduleDay);
			}
			var resCalcData = _schedulingResultStateHolder().ToResourceOptimizationData(considerShortBreaks, doIntraIntervalCalculation);
			foreach (var pair in dic)
			{
				_resourceOptimizationHelper.ResourceCalculate(pair.Key, resCalcData);
				if (!dic.ContainsKey(pair.Key.AddDays(1)))
					_resourceOptimizationHelper.ResourceCalculate(pair.Key.AddDays(1), resCalcData);
			}
		}
	}
}