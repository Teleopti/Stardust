using System;

namespace Teleopti.Ccc.Domain.Common
{
	public static class MathExtended
	{
		public static double Min(double value1, double value2, double value3)
		{
			return Math.Min(Math.Min(value1, value2), value3);
		}
	}
}