namespace Teleopti.Interfaces.Domain
{
	
	/// <summary>
	/// A restriction that can match shifts to calculate work time min max
	/// </summary>
	public interface IWorkTimeMinMaxRestriction
	{
		/// <summary>
		/// returns true if the restriction has the posibility to match a work shift
		/// </summary>
		/// <returns></returns>
		bool MayMatchWithShifts();

		/// <summary>
		/// returns true if this restriction can match with blacklisted shifts
		/// </summary>
		/// <returns></returns>
		bool MayMatchBlacklistedShifts();

		/// <summary>
		/// returns true if this restriction can match with this shift category
		/// </summary>
		/// <param name="shiftCategory"></param>
		/// <returns></returns>
		bool Match(IShiftCategory shiftCategory);

		/// <summary>
		/// returns true if the restriction matches the given work shift
		/// </summary>
		/// <param name="workShiftProjection"></param>
		/// <returns></returns>
		bool Match(IWorkShiftProjection workShiftProjection);
	}
}