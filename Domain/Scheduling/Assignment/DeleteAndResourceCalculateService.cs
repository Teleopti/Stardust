using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IDeleteAndResourceCalculateService
	{
		IList<IScheduleDay> DeleteWithResourceCalculation(IList<IScheduleDay> list,
			ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation);

		IList<IScheduleDay> DeleteWithoutResourceCalculation(IList<IScheduleDay> list,
			ISchedulePartModifyAndRollbackService rollbackService);

		IList<IScheduleDay> DeleteWithoutResourceCalculationOnNextDay(IList<IScheduleDay> list,
			ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation);
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

		public IList<IScheduleDay> DeleteWithResourceCalculation(IList<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			IList<IScheduleDay> deleted = _deleteSchedulePartService.Delete(list, rollbackService);
			resourceCalculate(list, considerShortBreaks, false, doIntraIntervalCalculation);

			return deleted;
		}

		public IList<IScheduleDay> DeleteWithoutResourceCalculationOnNextDay(IList<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{

			IList<IScheduleDay> deleted = _deleteSchedulePartService.Delete(list, rollbackService);
			resourceCalculate(list, considerShortBreaks, true, doIntraIntervalCalculation);

			return deleted;
		}

		public IList<IScheduleDay> DeleteWithoutResourceCalculation(IList<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService)
		{
			IList<IScheduleDay> deleted = _deleteSchedulePartService.Delete(list, rollbackService);

			return deleted;
		}

		private void resourceCalculate(IEnumerable<IScheduleDay> list, bool considerShortBreaks, bool skipNextDay, bool doIntraIntervalCalculation)
		{
			IDictionary<DateOnly, IList<IScheduleDay>> dic = new Dictionary<DateOnly, IList<IScheduleDay>>();
			foreach (var scheduleDay in list)
			{
				DateOnly key = scheduleDay.DateOnlyAsPeriod.DateOnly;
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
				if (!dic.ContainsKey(pair.Key.AddDays(1)) && !skipNextDay)
					_resourceOptimizationHelper.ResourceCalculateDate(pair.Key.AddDays(1), considerShortBreaks, doIntraIntervalCalculation);
			}
		}
	}
}