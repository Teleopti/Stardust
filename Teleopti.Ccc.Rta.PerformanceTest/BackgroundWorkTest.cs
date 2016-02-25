using System;
using System.Threading;
using NUnit.Framework;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("BackgroundWorkTest")]
	public class BackgroundWorkTest
    {
		[Test]
		public void MeasurePerformance()
		{
			Thread.Sleep(TimeSpan.FromMinutes(new Random().Next(1, 2)));
		}
    }
}
