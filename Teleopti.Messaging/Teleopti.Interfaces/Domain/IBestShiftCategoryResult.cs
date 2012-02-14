namespace Teleopti.Interfaces.Domain
{
	///<summary>
	/// Why no ShiftCategory was found
	///</summary>
	public enum FailureCause
	{
		///<summary>
		/// No failure, a category was found
		///</summary>
		NoFailure,
		///<summary>
		/// Couldn't find a category because there was already an assignment
		///</summary>
		AlreadyAssigned,
		/// <summary>
		/// No valid Schedule Period
		/// </summary>
		NoValidPeriod,
		/// <summary>
		/// The Restrictions can not be combined into one
		/// </summary>
		ConflictingRestrictions
	}
	/// <summary>
	/// 
	/// </summary>
	public interface IBestShiftCategoryResult
	{
		/// <summary>
		/// Gets the best shift category.
		/// </summary>
		/// <value>The best shift category.</value>
		IShiftCategory BestShiftCategory { get; }
		/// <summary>
		/// Gets the failure cause.
		/// </summary>
		/// <value>The failure cause.</value>
		FailureCause FailureCause { get; }
	}
}