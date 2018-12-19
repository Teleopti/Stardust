using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class RunIntervalAttribute : Attribute
	{
		public int RunInterval { get; }

		public RunIntervalAttribute(int runInterval)
		{
			RunInterval = runInterval;
		}
	}
}
