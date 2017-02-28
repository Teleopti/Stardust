namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Creates and build up a <see cref="ISchedulePeriodShiftCategoryBackToLegalStateService"/> 
    /// </summary>
    public interface ISchedulePeriodShiftCategoryBackToLegalStateServiceBuilder
    {
		/// <summary>
		/// Builds the ISchedulePeriodShiftCategoryBackToLegalStateService class with the given container.
		/// </summary>
		/// <param name="scheduleMatrix">The schedule matrix.</param>
		/// <param name="schedulePartModifyAndRollbackService">The schedule part modify and rollback service.</param>
		/// <returns></returns>
		ISchedulePeriodShiftCategoryBackToLegalStateService Build(IScheduleMatrixPro scheduleMatrix, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);
    }
}