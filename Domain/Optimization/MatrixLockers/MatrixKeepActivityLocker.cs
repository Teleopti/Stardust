using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
	public interface IMatrixKeepActivityLocker
	{
		void Execute();
	}
	public class MatrixKeepActivityLocker
	{
		private readonly IEnumerable<IScheduleMatrixPro> _matrixList;
		private readonly IList<IActivity> _keepActivities;

		public MatrixKeepActivityLocker(IEnumerable<IScheduleMatrixPro> matrixList, IList<IActivity> keepActivities)
		{
			_matrixList = matrixList;
			_keepActivities = keepActivities;
		}

		public void Execute()
		{
			foreach (var scheduleMatrixPro in _matrixList)
			{
				foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
				{
					var schedulePart = scheduleDayPro.DaySchedulePart();
					if(!schedulePart.HasProjection()) continue;

					var visualLayers = schedulePart.ProjectionService().CreateProjection();

					foreach (var visualLayer in visualLayers)
					{
						var activity = visualLayer.Payload as IActivity;
						if (activity == null || !_keepActivities.Contains(activity)) continue;
						var dateOnly = scheduleDayPro.Day;
						scheduleMatrixPro.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly));
						break;
					}
				}
			}	
		}
	}
}
