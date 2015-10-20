using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IDeleteAndResourceCalculateService
	{
		IList<IScheduleDay> DeleteWithResourceCalculation(IList<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IScheduleDay> DeleteWithResourceCalculation(IList<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks)
		{

			IList<IScheduleDay> deleted = _deleteSchedulePartService.Delete(list, rollbackService);
			resourceCalculate(list, considerShortBreaks);

			return deleted;
		}

		private void resourceCalculate(IEnumerable<IScheduleDay> list, bool considerShortBreaks)
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
				_resourceOptimizationHelper.ResourceCalculateDate(pair.Key, considerShortBreaks);
				if (!dic.ContainsKey(pair.Key.AddDays(1)))
					_resourceOptimizationHelper.ResourceCalculateDate(pair.Key.AddDays(1), considerShortBreaks);
			}
		}
	}
}