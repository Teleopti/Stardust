namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Can tell if a preference is extended
	/// </summary>
	public interface IExtendedPreferencePredicate
	{
		/// <summary>
		/// Can tell if a preference is extended
		/// </summary>
		/// <param name="preferenceDay">the preference day</param>
		/// <returns>True if extended, otherwise false</returns>
		bool IsExtended(IPreferenceDay preferenceDay);
	}

}