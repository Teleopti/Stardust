using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain.Service.PerformanceMeasurement;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	[PerformanceMeasurementTest]
	[Explicit]
	[Category("LongRunning")]
	public class MeasurePerformanceTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public PerformanceMeasurementTestAttribute Context;

		[Test]
		public void Swooosh()
		{
			const int numberOfAgents = 50_000;
			Context.MakeUsersFaster(Enumerable.Range(0, numberOfAgents).Select(x => $"agent{x}"));

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Target.Handle(new TenantHourTickEvent());
			stopwatch.Stop();

			Console.WriteLine($@"{numberOfAgents}: {stopwatch.Elapsed}");
		}
	}
}