﻿using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Rta.PerformanceTest.Code;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("BackgroundWorkTest")]
	[RtaPerformanceTest]
	public class BackgroundWorkTest
	{
		public StatesSender States;
		public HangfireUtilities Hangfire;

		[Test]
		public void MeasurePerformance()
		{
			States.Send();
			Hangfire.WaitForQueue();
		}
    }
}
