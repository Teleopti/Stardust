using System;

namespace Teleopti.Ccc.Infrastructure.MessageBroker
{
	public static class Extensions
	{
		public static DateTime? NullIfMinValue(this DateTime value)
		{
			if (value == DateTime.MinValue)
				return null;
			return value;
		}
	}
}