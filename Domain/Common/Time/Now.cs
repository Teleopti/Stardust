using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public sealed class Now : INow
	{
		public DateTime UtcDateTime()
		{
			return DateTime.UtcNow;
		}
	}
}