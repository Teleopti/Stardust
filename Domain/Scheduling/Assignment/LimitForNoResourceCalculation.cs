﻿namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class LimitForNoResourceCalculation : ILimitForNoResourceCalculation
	{
		public int NumberOfAgents { get; private set; } = 75;

		public void SetFromTest(int limit)
		{
			NumberOfAgents = limit;
		}
	}
}