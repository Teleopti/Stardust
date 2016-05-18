﻿using System;

namespace Teleopti.Ccc.Domain.Cascading
{
	public static class DoubleExtensionsInCascadingScenarios
	{
		//put in this namespace and not in "Common" or similar because the diff might be different in other scenarios.
		public static bool IsZero(this double value)
		{
			return Math.Abs(value) < 0.0000000000000001d;
		}
	}
}