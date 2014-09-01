using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization
{
	public class DeleteSelectedDaysForTeam : IDeleteSelectedDaysForTeam
	{
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;

		public DeleteSelectedDaysForTeam(IDeleteAndResourceCalculateService deleteAndResourceCalculateService)
		{
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
		}

		public void PerformDelete(IList<IScheduleMatrixPro> matrixList, DateOnly firstDate, DateOnly secondDate,
			ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks)
		{
			IList<IScheduleDay> listToDelete = new List<IScheduleDay>();
			foreach (var matrix in matrixList)
			{
				if (matrix.SchedulePeriod.DateOnlyPeriod.DayCollection().Contains(firstDate))
					listToDelete.Add(matrix.GetScheduleDayByKey(firstDate).DaySchedulePart());
				if (matrix.SchedulePeriod.DateOnlyPeriod.DayCollection().Contains(secondDate))
					listToDelete.Add(matrix.GetScheduleDayByKey(secondDate).DaySchedulePart());
			}

			_deleteAndResourceCalculateService.DeleteWithResourceCalculation(listToDelete, rollbackService, considerShortBreaks);
		}
	}

	public interface IDeleteSelectedDaysForTeam
	{
		void PerformDelete(IList<IScheduleMatrixPro> matrixList, DateOnly firstDate, DateOnly secondDate, ISchedulePartModifyAndRollbackService rollbackService, bool considerShortBreaks);
	}
}