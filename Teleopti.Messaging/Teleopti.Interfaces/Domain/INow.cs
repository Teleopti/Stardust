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
		DateTime Time { get; }
	}
}