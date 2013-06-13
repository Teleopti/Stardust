using System;

namespace Teleopti.Interfaces.Domain
{
	public struct ResourceLayer
	{
		public double Resource { get; set; }

		public DateTimePeriod Period { get; set; }

		public Guid Activity { get; set; }
	}
}