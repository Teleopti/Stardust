namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service for finding a workshift.
    /// </summary>
    public interface IWorkShiftFinderService
    {
    	/// <summary>
    	/// Finds the best shift.
    	/// </summary>
    	/// <param name="schedulePart">The schedule part.</param>
    	/// <param name="schedulingOptions">The scheduling options.</param>
    	/// <param name="matrix">The matrix.</param>
    	/// <param name="effectiveRestriction">The effective restriction.</param>
    	/// <param name="possibleStartEndCategory"> </param>
    	/// <returns></returns>
    	/// ///
    	/// <remarks>
    	/// Created by: Ola
    	/// Created date: 2008-09-18
    	/// ///
    	/// </remarks>
		WorkShiftFinderServiceResult FindBestShift(IScheduleDay schedulePart, ISchedulingOptions schedulingOptions, IScheduleMatrixPro matrix, IEffectiveRestriction effectiveRestriction, IPossibleStartEndCategory possibleStartEndCategory);
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