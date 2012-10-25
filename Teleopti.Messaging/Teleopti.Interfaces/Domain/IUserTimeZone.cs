using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Returns logged on user's <see cref="TimeZoneInfo"/>.
	/// </summary>
	public interface IUserTimeZone
	{
		/// <summary>
		/// Gets the time zone
		/// </summary>
		/// <returns></returns>
		TimeZoneInfo TimeZone();
	}
}