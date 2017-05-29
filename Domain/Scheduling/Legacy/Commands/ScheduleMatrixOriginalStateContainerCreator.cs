using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveBackToLegalStateGui_44333)]
	public class ScheduleMatrixOriginalStateContainerCreator
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly MatrixListFactory _matrixListFactory;

		public ScheduleMatrixOriginalStateContainerCreator(IScheduleDayEquator scheduleDayEquator, MatrixListFactory matrixListFactory)
		{
			_scheduleDayEquator = scheduleDayEquator;
			_matrixListFactory = matrixListFactory;
		}

		public IList<IScheduleMatrixOriginalStateContainer> CreateScheduleMatrixOriginalStateContainers(IScheduleDictionary schedules, IEnumerable<IScheduleDay> scheduleDays, DateOnlyPeriod selectedPeriod)
		{
			IList<IScheduleMatrixOriginalStateContainer> retList = new List<IScheduleMatrixOriginalStateContainer>();
			foreach (IScheduleMatrixPro scheduleMatrixPro in _matrixListFactory.CreateMatrixListForSelection(schedules, scheduleDays))
				retList.Add(new ScheduleMatrixOriginalStateContainer(scheduleMatrixPro, _scheduleDayEquator));

			return retList;
		}
	}
}