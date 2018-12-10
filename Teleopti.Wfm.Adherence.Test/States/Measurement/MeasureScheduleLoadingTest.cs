using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Measurement
{
	[TestFixture]
	[Explicit]
	[Category("LongRunning")]
	[PerformanceMeasurementTest]
	public class MeasureScheduleLoadingTest
	{
		public Database Database;
		public Rta Rta;
		public FakeConfigReader Config;
		public FakeEventPublisher Publisher;
		public AnalyticsDatabase Analytics;
		public PerformanceMeasurementTestAttribute Context;

		public WithUnitOfWork Uow;
		public MutableNow Now;
		public IPersonRepository Persons;
		public IScenarioRepository Scenarios;
		public IActivityRepository Activities;
		public IPersonAssignmentRepository PersonAssignments;

		private void createData()
		{
			Now.Is("2016-09-20 00:00");
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithDefaultScenario("default")
				.WithStateGroup("default", true)
				.WithStateCode(Rta.LogOutBySnapshot)
				.WithActivity("phone")
				.WithActivity("break")
				.WithActivity("lunch");
			stateCodes.ForEach(x => Database.WithStateGroup($"code{x}").WithStateCode($"code{x}"));
			var dates = new DateOnly(Now.UtcDateTime()).DateRange(100);

			Context.MakeUsersFaster(userCodes);

			var persons = Uow.Get(uow => Persons.LoadAll());

			var scenario = Uow.Get(() => Scenarios.LoadDefaultScenario());
			var activities = Uow.Get(() => Activities.LoadAll());
			var phone = activities.Single(x => x.Name == "phone");
			var brejk = activities.Single(x => x.Name == "break");
			var lunch = activities.Single(x => x.Name == "lunch");

			userCodes.ForEach(userCode =>
			{
				Uow.Do(uow =>
				{
					var person = persons.Single(x => x.Name.FirstName == userCode);
					dates.ForEach(date =>
					{
						var d = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
						var assignment = new PersonAssignment(person, scenario, new Ccc.Domain.InterfaceLegacy.Domain.DateOnly(d.Date));
						assignment.AddActivity(phone, d.AddHours(8), d.AddHours(17));
						assignment.AddActivity(brejk, d.AddHours(10), d.AddHours(10).AddMinutes(15));
						assignment.AddActivity(lunch, d.AddHours(12), d.AddHours(13));
						assignment.AddActivity(brejk, d.AddHours(15), d.AddHours(15).AddMinutes(15));
						PersonAssignments.Add(assignment);
					});
				});
			});

			// trigger tick to populate mappings
			Publisher.Publish(new TenantMinuteTickEvent());
		}

		private static IEnumerable<string> userCodes => Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();
		private static IEnumerable<string> stateCodes => Enumerable.Range(0, 2).Select(x => $"code{x}").ToArray();

		[Test]
		[Setting("OptimizeScheduleChangedEvents_DontUseFromWeb", true)]
		public void Measure()
		{
			createData();

			(
					from parallelTransactions in Context.ParallelTransactions()
					from transactionSize in Context.TransactionSize()
					from batchSize in Context.BatchSize()
					from variation in Context.Variation()
					select new {parallelTransactions, transactionSize, batchSize, variation}
				)
				.Select(x =>
				{
					Config.FakeSetting("RtaBatchParallelTransactions", x.parallelTransactions.ToString());
					Config.FakeSetting("RtaBatchMaxTransactionSize", x.transactionSize.ToString());

					var batches = userCodes
						.Batch(x.batchSize)
						.Select(u => new BatchForTest
						{
							States = u
								.Select(y => new BatchStateForTest
								{
									UserCode = y,
									StateCode = $"code{x.variation}"
								})
								.ToArray()
						}).ToArray();

					var stopwatch = new Stopwatch();
					stopwatch.Start();
					batches.ForEach(Rta.Process);
					stopwatch.Stop();

					return new
					{
						x.batchSize,
						x.variation,
						stopwatch.Elapsed
					};
				})
				.OrderBy(x => x.Elapsed)
				.ForEach(x => Console.WriteLine($@"{x.batchSize} {x.variation}: {x.Elapsed}"));
		}
	}
}