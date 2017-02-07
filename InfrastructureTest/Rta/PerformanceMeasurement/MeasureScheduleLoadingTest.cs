using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.PerformanceMeasurement
{
	[TestFixture]
	[InfrastructureTest]
	[Explicit]
	[Category("LongRunning")]
	public class MeasureScheduleLoadingTest : PerformanceMeasurementTestBase, ISetup
	{
		public Database Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public FakeConfigReader Config;
		public ConfigurableSyncEventPublisher Publisher;
		public AgentStateMaintainer Maintainer;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
		}
		
		public override void OneTimeSetUp()
		{
			Now.Is("2016-09-20 00:00");
			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();
			Publisher.AddHandler<AgentStateMaintainer>();
			Publisher.AddHandler<ProjectionChangedEventPublisher>();
			Publisher.AddHandler<ScheduleProjectionReadOnlyUpdater>();
			Publisher.AddHandler<ExternalLogonReadModelUpdater>();
			Publisher.AddHandler<CurrentScheduleReadModelUpdater>();
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithDefaultScenario("default")
				.WithActivity("phone")
				.WithActivity("break")
				.WithActivity("lunch");
			stateCodes.ForEach(x => Database.WithStateGroup($"code{x}").WithStateCode($"code{x}"));
			var dates = new DateOnly(Now.UtcDateTime()).DateRange(100);

			MakeUsersFaster(userCodes);

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
						var assignment = new PersonAssignment(person, scenario, date);
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
		public void Measure(
			[Values(50, 500, 1000)] int batchSize,
			[Values(1, 2, 3, 4, 5)] int variation
		)
		{
			var persons = Uow.Get(uow => Persons.LoadAll());

			Uow.Do(() =>
			{
				persons.ForEach(p =>
				{ });
			});

			var timer = new Stopwatch();
			timer.Start();

			userCodes
				.Batch(batchSize)
				.Select(u => new BatchForTest
				{
					States = u
						.Select(y => new BatchStateForTest
						{
							UserCode = y,
							StateCode = $"code{variation}"
						})
						.ToArray()
				}).ForEach(Rta.SaveStateBatch);

			timer.Stop();
			Console.WriteLine($"Time {timer.Elapsed}");
		}
	}
}