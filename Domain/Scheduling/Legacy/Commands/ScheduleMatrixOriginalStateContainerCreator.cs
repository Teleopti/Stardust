using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleMatrixOriginalStateContainerCreator
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly IMatrixListFactory _matrixListFactory;

		public ScheduleMatrixOriginalStateContainerCreator(IScheduleDayEquator scheduleDayEquator, IMatrixListFactory matrixListFactory)
		{
			_scheduleDayEquator = scheduleDayEquator;
			_matrixListFactory = matrixListFactory;
		}

		public IList<IScheduleMatrixOriginalStateContainer> CreateScheduleMatrixOriginalStateContainers(
			IList<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod)
		{
			IList<IScheduleMatrixOriginalStateContainer> retList = new List<IScheduleMatrixOriginalStateContainer>();
			foreach (IScheduleMatrixPro scheduleMatrixPro in _matrixListFactory.CreateMatrixListForSelection(scheduleDays))
				retList.Add(new ScheduleMatrixOriginalStateContainer(scheduleMatrixPro, _scheduleDayEquator));

			return retList;
		}
	}
}