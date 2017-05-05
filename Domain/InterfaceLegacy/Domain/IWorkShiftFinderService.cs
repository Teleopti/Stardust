using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Service for finding a workshift.
    /// </summary>
    public interface IWorkShiftFinderService
    {
		WorkShiftFinderServiceResult FindBestShift(IScheduleDay schedulePart, SchedulingOptions schedulingOptions, IScheduleMatrixPro matrix, IEffectiveRestriction effectiveRestriction);
    }

	public class WorkShiftFinderServiceResult
	{
		public WorkShiftFinderServiceResult(IWorkShiftCalculationResultHolder resultHolder, WorkShiftFinderResult finderResult)
		{
			FinderResult = finderResult;
			ResultHolder = resultHolder;
		}

		public IWorkShiftCalculationResultHolder ResultHolder { get; private set; }
		public WorkShiftFinderResult FinderResult { get; private set; }
	}
}