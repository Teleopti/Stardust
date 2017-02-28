using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public sealed class Now : INow
	{
		public DateTime UtcDateTime()
		{
			return DateTime.UtcNow;
		}
	}
}