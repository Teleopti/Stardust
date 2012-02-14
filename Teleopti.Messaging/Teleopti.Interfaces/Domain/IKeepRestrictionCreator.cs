namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Create effective restriction classes for re-scheduling a person to keep some features of the old shift that is being optimized.
	/// This class is used in optimization.
	/// In the old version we stored the original shift to be able to keep some of its old properties when finding another shift. THat solution was time consuming and 
	/// is replaced by this logic.
	/// Later maybe the logic should be refactored by using memo design pattern. 
	/// </summary>
	public interface IKeepRestrictionCreator
	{
		/// <summary>
		/// Creates the keep shift category restriction.
		/// </summary>
		/// <param name="scheduleDay">The schedule day.</param>
		/// <returns></returns>
		IEffectiveRestriction CreateKeepShiftCategoryRestriction(IScheduleDay scheduleDay);

		/// <summary>
		/// Creates the keep start and end time restriction.
		/// </summary>
		/// <param name="scheduleDay">The schedule day.</param>
		/// <returns></returns>
		IEffectiveRestriction CreateKeepStartAndEndTimeRestriction(IScheduleDay scheduleDay);
	}
}