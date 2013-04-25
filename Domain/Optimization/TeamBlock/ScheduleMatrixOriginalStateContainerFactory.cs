using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface IScheduleMatrixOriginalStateContainerFactory
	{
		IScheduleMatrixOriginalStateContainer CreateScheduleMatrixOriginalStateContainer(IScheduleMatrixPro scheduleMatrix, IScheduleDayEquator scheduleDayEquator);
	}
	public class ScheduleMatrixOriginalStateContainerFactory : IScheduleMatrixOriginalStateContainerFactory
	{
		public IScheduleMatrixOriginalStateContainer CreateScheduleMatrixOriginalStateContainer(IScheduleMatrixPro scheduleMatrix,
		                                                                                       IScheduleDayEquator scheduleDayEquator)
		{
			return new ScheduleMatrixOriginalStateContainer(scheduleMatrix, scheduleDayEquator);
		}
	}
}