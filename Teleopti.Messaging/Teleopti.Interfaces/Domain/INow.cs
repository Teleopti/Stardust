using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Gives current time
	/// </summary>
	public interface INow
	{
		/// <summary>
		/// Returns curent datetime
		/// </summary>
		DateTime LocalDateTime();

		/// <summary>
		/// Returns curent datetime in UTC
		/// </summary>
		DateTime UtcDateTime();

		/// <summary>
		/// Returns current date
		/// </summary>
		DateOnly DateOnly();
	}
}