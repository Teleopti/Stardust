using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class DeleteAndResourceCalculateService
	{
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IResourceCalculation _resourceOptimizationHelper;

		public DeleteAndResourceCalculateService(Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
																					IDeleteSchedulePartService deleteSchedulePartService, 
																					IResourceCalculation resourceOptimizationHelper)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		public void DeleteWithResourceCalculation(IEnumerable<IScheduleDay> daysToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			if (daysToDelete.Any()) //needed because it throws lower down the stack if list is empty for some strange reason I cannot understand
			{
				_deleteSchedulePartService.Delete(daysToDelete, rollbackService, NewBusinessRuleCollection.Minimum());
				resourceCalculate(daysToDelete, considerShortBreaks, doIntraIntervalCalculation);
			}
		}
		
		private void resourceCalculate(IEnumerable<IScheduleDay> daysToDelete, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			var dic = new Dictionary<DateOnly, IList<IScheduleDay>>();
			foreach (var scheduleDay in daysToDelete)
			{
				var key = scheduleDay.DateOnlyAsPeriod.DateOnly;
				if (!dic.TryGetValue(key, out var value))
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