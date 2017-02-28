namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service for finding a workshift.
    /// </summary>
    public interface IWorkShiftFinderService
    {
		WorkShiftFinderServiceResult FindBestShift(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix, IEffectiveRestriction effectiveRestriction);
    }

	public class WorkShiftFinderServiceResult
	{
		public WorkShiftFinderServiceResult(IWorkShiftCalculationResultHolder resultHolder, IWorkShiftFinderResult finderResult)
		{
			FinderResult = finderResult;
			ResultHolder = resultHolder;
		}

		public IWorkShiftCalculationResultHolder ResultHolder { get; private set; }
		public IWorkShiftFinderResult FinderResult { get; private set; }
	}
}