using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class Now : INow
	{
		public DateTime LocalTime()
		{
			return DateTime.Now;
		}

		public DateTime UtcTime()
		{
			return DateTime.UtcNow;
		}

		public DateOnly Date()
		{
			return new DateOnly(LocalTime());
		}
	}
}