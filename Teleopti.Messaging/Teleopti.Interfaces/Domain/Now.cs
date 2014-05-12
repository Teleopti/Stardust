using System;

namespace Teleopti.Interfaces.Domain
{
	public sealed class Now : INow
	{
		public DateTime UtcDateTime()
		{
			return DateTime.UtcNow;
		}
	}
}