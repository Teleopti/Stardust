﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Rules;
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
		private readonly IResourceOptimization _resourceOptimizationHelper;
		private readonly IResourceCalculateAfterDeleteDecider _resourceCalculateAfterDeleteDecider;
		private readonly IsNightShift _isNightShift;

		public DeleteAndResourceCalculateService(Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
																					IDeleteSchedulePartService deleteSchedulePartService, 
																					IResourceOptimization resourceOptimizationHelper,
																					IResourceCalculateAfterDeleteDecider resourceCalculateAfterDeleteDecider,
																					IsNightShift isNightShift)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_resourceCalculateAfterDeleteDecider = resourceCalculateAfterDeleteDecider;
			_isNightShift = isNightShift;
		}

		public void DeleteWithResourceCalculation(IEnumerable<IScheduleDay> daysToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			if (daysToDelete.Any()) //needed because it throws lower down the stack if list is empty for some strange reason I cannot understand
			{
				_deleteSchedulePartService.Delete(daysToDelete, rollbackService, NewBusinessRuleCollection.Minimum());
				resourceCalculate(daysToDelete, considerShortBreaks, doIntraIntervalCalculation);
			}
		}

		public void DeleteWithResourceCalculation(IScheduleDay dayToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			_deleteSchedulePartService.Delete(new []{ dayToDelete}, rollbackService);

			if (_resourceCalculateAfterDeleteDecider.DoCalculation(dayToDelete.Person, dayToDelete.DateOnlyAsPeriod.DateOnly))
			{
				var resCalcData = _schedulingResultStateHolder().ToResourceOptimizationData(considerShortBreaks, doIntraIntervalCalculation);
				var date = dayToDelete.DateOnlyAsPeriod.DateOnly;
				_resourceOptimizationHelper.ResourceCalculate(date, resCalcData);
				if (_isNightShift.InEndUserTimeZone(dayToDelete))
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