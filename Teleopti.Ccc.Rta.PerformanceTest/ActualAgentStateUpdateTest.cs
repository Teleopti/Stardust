using System;
using System.Threading;
using NUnit.Framework;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("ActualAgentStateUpdateTest")]
	public class ActualAgentStateUpdateTest
	{
		[Test]
		public void MeasurePerformance()
		{
			Thread.Sleep(TimeSpan.FromMinutes(new Random().Next(1, 2)));
		}
	}
}