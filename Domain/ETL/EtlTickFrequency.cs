using System;

namespace Teleopti.Ccc.Domain.ETL
{
	public static class EtlTickFrequency
	{
		public static TimeSpan Value { get; } = TimeSpan.FromSeconds(10);
	}
}