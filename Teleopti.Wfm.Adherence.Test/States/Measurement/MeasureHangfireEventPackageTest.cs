using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Measurement
{
	[RealHangfire]
	[TestFixture]
	[Explicit]
	[Category("LongRunning")]
	[PerformanceMeasurementTest]
	public class MeasureHangfireEventPackageTest
	{
		public Database Database;
		public Rta Rta;
		public FakeConfigReader Config;
		public AnalyticsDatabase Analytics;
		public PerformanceMeasurementTestAttribute Context;
		public IEventPublisherScope EventPublisherScope;
		public FakeEventPublisher FakeEventPublisher;

		private void createData()
		{
			using (EventPublisherScope.OnAllThreadsPublishTo(FakeEventPublisher))
			{
				Analytics.WithDataSource(9, "sourceId");
				Database
					.WithDefaultScenario("default")
					.WithStateGroup("default", true)
					.WithStateCode(Rta.LogOutBySnapshot);
				stateCodes.ForEach(x => Database.WithStateGroup(x).WithStateCode(x));
				Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));

				Context.MakeUsersFaster(userCodes);

				// trigger tick to populate mappings
				//Publisher.Publish(new TenantMinuteTickEvent());
				Database.PublishRecurringEvents();
			}
		}

		private static IEnumerable<string> userCodes => Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();
		private static IEnumerable<string> stateCodes => Enumerable.Range(0, 100).Select(x => $"code{x}").ToArray();

		[Test]
		public void ShouldMeasurePackagePerformance()
		{
			createData();

			(
					from packageSize in new[] {100, 250, 500, 1000}
					from batchSize in Context.BatchSize()
					from variation in Context.Variation()
					select new { packageSize, batchSize, variation }
				)
				.Select(x =>
				{
					Config.FakeSetting("EventMaxPackageSize", x.packageSize.ToString());

					var batches = Enumerable.Range(0, 5000)
						.Batch(x.batchSize)
						.Select(s => new BatchForTest
						{
							States = userCodes
								.RandomizeBetter()
								.Take(x.batchSize)
								.Select(y => new BatchStateForTest
								{
									UserCode = y,
									StateCode = stateCodes.RandomizeBetter().First()
								})
								.ToArray()
						}).ToArray();

					var stopwatch = new Stopwatch();
					stopwatch.Start();
					batches.ForEach(Rta.Process);
					stopwatch.Stop();

					return new
					{
						x.packageSize,
						x.batchSize,
						x.variation,
						stopwatch.Elapsed
					};
				})
				.OrderBy(x => x.Elapsed)
				.ForEach(x => Console.WriteLine($@"{x.packageSize} {x.batchSize} {x.variation}: {x.Elapsed}"));
		}

	}
}