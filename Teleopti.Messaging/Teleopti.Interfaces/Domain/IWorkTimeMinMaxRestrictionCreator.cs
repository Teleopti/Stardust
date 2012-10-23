namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Result from creating a WorkTimeMinMaxRestriction
	/// </summary>
	public class WorkTimeMinMaxRestrictionCreationResult
	{
		/// <summary>
		/// The restriction created
		/// </summary>
		public IWorkTimeMinMaxRestriction Restriction { get; set; }

		/// <summary>
		/// True if the restriction has an absence that is in contract time
		/// Used to know if we should fake a WorkTimeMinMax
		/// Needs more refactoring here...
		/// </summary>
		public bool IsAbsenceInContractTime { get; set; }
	}

	/// <summary>
	/// Created work time min max restrictions
	/// </summary>
	public interface IWorkTimeMinMaxRestrictionCreator
	{
		/// <summary>
		/// Makes the stuff
		/// </summary>
		/// <param name="scheduleDay"></param>
		/// <param name="effectiveRestrictionOptions"></param>
		/// <returns></returns>
		WorkTimeMinMaxRestrictionCreationResult MakeWorkTimeMinMaxRestriction(IScheduleDay scheduleDay, IEffectiveRestrictionOptions effectiveRestrictionOptions);
	}
}