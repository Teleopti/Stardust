using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Gives current utc time
	/// </summary>
	public interface IUtcNow
	{
		/// <summary>
		/// Returns curent datetime in UTC
		/// </summary>
		DateTime UtcDateTime();

		/// <summary>
		/// Has time been explicitly set (mostly used in tests)
		/// </summary>
		bool IsExplicitlySet();
	}
}