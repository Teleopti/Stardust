using System;
using System.Threading;
using NUnit.Framework;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("SendStateTest")]
	public class StateSendTest
	{
		[Test]
		public void MeasurePerformance()
		{
			Thread.Sleep(TimeSpan.FromMinutes(new Random().Next(1, 2)));
		}
	}
}