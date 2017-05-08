using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.PerformanceMeasurement
{
	[TestFixture]
	[Explicit]
	[Category("LongRunning")]
	[PerformanceMeasurementTest]
	public class MeasureBatchTest : ISetup
	{
		public Database Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public FakeConfigReader Config;
		public FakeEventPublisher Publisher;
		public AgentStateMaintainer Maintainer;
		public MutableNow Now;
		public AnalyticsDatabase Analytics;
		public WithUnitOfWork Uow;
		public PerformanceMeasurementTestAttribute Attribute;
		public IPersonRepository Persons;
		public IScenarioRepository Scenarios;
		public IActivityRepository Activities;
		public IPersonAssignmentRepository PersonAssignments;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		private void createData()
		{
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithDefaultScenario("default")
				.WithStateGroup("default", true)
				.WithStateCode(Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot);
			stateCodes.ForEach(x => Database.WithStateGroup(x).WithStateCode(x));
			Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));

			Attribute.MakeUsersFaster(userCodes);

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
				from parallelTransactions in Attribute.ParallelTransactions()
				from transactionSize in Attribute.TransactionSize()
				from batchSize in Attribute.BatchSize()
				from variation in Attribute.Variation()
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
				batches.ForEach(Rta.SaveStateBatch);
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