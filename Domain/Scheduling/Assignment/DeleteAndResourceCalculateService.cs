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
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IResourceCalculateDaysDecider _resourceCalculateDaysDecider;

		public DeleteAndResourceCalculateService(IDeleteSchedulePartService deleteSchedulePartService, 
																					IResourceOptimizationHelper resourceOptimizationHelper,
																					IResourceCalculateDaysDecider resourceCalculateDaysDecider)
		{
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_resourceCalculateDaysDecider = resourceCalculateDaysDecider;
		}

		public void DeleteWithResourceCalculation(IEnumerable<IScheduleDay> daysToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			_deleteSchedulePartService.Delete(daysToDelete, rollbackService);
			resourceCalculate(daysToDelete, considerShortBreaks, doIntraIntervalCalculation);
		}

		public void DeleteWithResourceCalculation(IScheduleDay dayToDelete, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			_deleteSchedulePartService.Delete(new []{ dayToDelete}, rollbackService);

			var date = dayToDelete.DateOnlyAsPeriod.DateOnly;
			_resourceOptimizationHelper.ResourceCalculateDate(date, considerShortBreaks, doIntraIntervalCalculation);
			if (_resourceCalculateDaysDecider.IsNightShift(dayToDelete))
			{
				_resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), considerShortBreaks, doIntraIntervalCalculation);
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

			foreach (var pair in dic)
			{
				_resourceOptimizationHelper.ResourceCalculateDate(pair.Key, considerShortBreaks, doIntraIntervalCalculation);
				if (!dic.ContainsKey(pair.Key.AddDays(1)))
					_resourceOptimizationHelper.ResourceCalculateDate(pair.Key.AddDays(1), considerShortBreaks, doIntraIntervalCalculation);
			}
		}
	}
}