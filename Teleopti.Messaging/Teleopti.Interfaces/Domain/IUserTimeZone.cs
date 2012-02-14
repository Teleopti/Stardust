namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Returns logged on user's <see cref="ICccTimeZoneInfo"/>.
	/// </summary>
	public interface IUserTimeZone
	{
		/// <summary>
		/// Gets the time zone
		/// </summary>
		/// <returns></returns>
		ICccTimeZoneInfo TimeZone();
	}
}