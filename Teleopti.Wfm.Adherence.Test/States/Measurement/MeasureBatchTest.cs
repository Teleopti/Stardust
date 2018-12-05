using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Measurement
{
	[TestFixture]
	[Explicit]
	[Category("LongRunning")]
	[PerformanceMeasurementTest]
	public class MeasureBatchTest
	{
		public Database Database;
		public Rta Rta;
		public FakeConfigReader Config;
		public FakeEventPublisher Publisher;
		public AnalyticsDatabase Analytics;
		public PerformanceMeasurementTestAttribute Context;

		private void createData()
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
			Publisher.Publish(new TenantMinuteTickEvent());
		}

		private static IEnumerable<string> userCodes => Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();
		private static IEnumerable<string> stateCodes => Enumerable.Range(0, 100).Select(x => $"code{x}").ToArray();

		[Test]
		public void Measure()
		{
			createData();

			(
				from parallelTransactions in Context.ParallelTransactions()
				from transactionSize in Context.TransactionSize()
				from batchSize in Context.BatchSize()
				from variation in Context.Variation()
				select new { parallelTransactions, transactionSize, batchSize, variation }
			)
			.Select(x =>
			{
				Config.FakeSetting("RtaBatchParallelTransactions", x.parallelTransactions.ToString());
				Config.FakeSetting("RtaBatchMaxTransactionSize", x.transactionSize.ToString());

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
					x.parallelTransactions,
					x.transactionSize,
					x.batchSize,
					x.variation,
					stopwatch.Elapsed
				};
			})
			.OrderBy(x => x.Elapsed)
			.ForEach(x => Console.WriteLine($@"{x.parallelTransactions} {x.transactionSize} {x.batchSize} {x.variation}: {x.Elapsed}"));
		}

	}
}