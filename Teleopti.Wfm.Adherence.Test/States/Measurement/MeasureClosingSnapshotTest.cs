using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Measurement
{
	[TestFixture]
	[Explicit]
	[Category("LongRunning")]
	[PerformanceMeasurementTest]
	public class MeasureClosingSnapshotTest
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
				.WithStateGroup("default", true)
				.WithStateGroup("logged out", false, true)
				.WithStateCode(Rta.LogOutBySnapshot)
				.WithStateGroup("phone")
				.WithStateCode("phone");
			stateCodes.ForEach(x => Database.WithStateGroup($"code{x}").WithStateCode($"code{x}"));
			Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));

			Context.MakeUsersFaster(userCodes);

			// trigger tick to populate mappings
			Publisher.Publish(new TenantMinuteTickEvent());

			// states for all
			userCodes
				.Batch(1000)
				.Select(x => new BatchForTest
				{
					SnapshotId = "2016-09-09 10:00".Utc(),
					States = x.Select(y => new BatchStateForTest
					{
						UserCode = y,
						StateCode = "phone"
					}).ToArray()
				}).ForEach(Rta.Process);
		}

		private static IEnumerable<string> userCodes => Enumerable.Range(0, 3000).Select(x => $"user{x}").ToArray();
		private static IEnumerable<string> stateCodes => Enumerable.Range(0, 100).Select(x => $"code{x}").ToArray();

		[Test]
		public void Measure()
		{
			createData();

			(
				from parallelTransactions in Context.ParallelTransactions()
				from transactionSize in Context.TransactionSize()
				from variation in Context.Variation()
				select new { parallelTransactions, transactionSize, variation }
			)
			.Select(x =>
			{
				Config.FakeSetting("RtaCloseSnapshotParallelTransactions", x.parallelTransactions.ToString());
				Config.FakeSetting("RtaCloseSnapshotMaxTransactionSize", x.transactionSize.ToString());

				userCodes
					.Batch(1000)
					.Select(userCodeBatch => new BatchForTest
					{
						SnapshotId = "2016-09-09 10:00".Utc(),
						States = userCodeBatch.Select(y => new BatchStateForTest
						{
							UserCode = y,
							StateCode = "phone"
						}).ToArray()
					}).ForEach(Rta.Process);

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				Rta.CloseSnapshot(new CloseSnapshotForTest
				{
					SnapshotId = "2016-09-09 10:01".Utc()
				});

				stopwatch.Stop();

				return new
				{
					x.parallelTransactions,
					x.transactionSize,
					x.variation,
					stopwatch.Elapsed
				};
			})
			.OrderBy(x => x.Elapsed)
			.ForEach(x => Console.WriteLine($@"{x.parallelTransactions} {x.transactionSize} {x.variation}: {x.Elapsed}"));
		}

	}
}