using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IDeleteAndResourceCalculateService
	{
		void DeleteWithResourceCalculation(IEnumerable<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation);
		void DeleteWithoutResourceCalculationOnNextDay(IEnumerable<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation);
	}

	public class DeleteAndResourceCalculateService : IDeleteAndResourceCalculateService
	{
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public DeleteAndResourceCalculateService(IDeleteSchedulePartService deleteSchedulePartService, IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
		}

		public void DeleteWithResourceCalculation(IEnumerable<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			_deleteSchedulePartService.Delete(list, rollbackService);
			resourceCalculate(list, considerShortBreaks, true, doIntraIntervalCalculation);
		}

		public void DeleteWithoutResourceCalculationOnNextDay(IEnumerable<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			_deleteSchedulePartService.Delete(list, rollbackService);
			resourceCalculate(list, considerShortBreaks, false, doIntraIntervalCalculation);
		}

		private void resourceCalculate(IEnumerable<IScheduleDay> list, bool considerShortBreaks, bool resourceCalculateNextDay, bool doIntraIntervalCalculation)
		{
			var dic = new Dictionary<DateOnly, IList<IScheduleDay>>();
			foreach (var scheduleDay in list)
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
				if (resourceCalculateNextDay && !dic.ContainsKey(pair.Key.AddDays(1)))
					_resourceOptimizationHelper.ResourceCalculateDate(pair.Key.AddDays(1), considerShortBreaks, doIntraIntervalCalculation);
			}
		}
	}
}