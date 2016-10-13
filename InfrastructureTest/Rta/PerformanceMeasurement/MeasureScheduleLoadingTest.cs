using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	[MultiDatabaseTest]
	[Toggle(Toggles.RTA_Optimize_39667)]
	[Toggle(Toggles.RTA_RuleMappingOptimization_39812)]
	[Toggle(Toggles.RTA_BatchConnectionOptimization_40116)]
	[Toggle(Toggles.RTA_BatchQueryOptimization_40169)]
	[Toggle(Toggles.RTA_PersonOrganizationQueryOptimization_40261)]
	[Toggle(Toggles.RTA_ScheduleQueryOptimization_40260)]
	[Toggle(Toggles.RTA_ConnectionQueryOptimizeAllTheThings_40262)]
	[Toggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
	[Explicit]
	[Category("LongRunning")]
	public class MeasureScheduleLoadingTest : PerformanceMeasurementTestBase, ISetup
	{
		public Database Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public FakeConfigReader Config;
		public ConfigurableSyncEventPublisher Publisher;
		public AgentStateMaintainer Maintainer;
		public StateStreamSynchronizer Synchronizer;
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
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithDefaultScenario("default")
				.WithActivity("phone")
				.WithActivity("break")
				.WithActivity("lunch");
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

			// single state and init (touch will think its already done)
			Rta.SaveState(new StateForTest
			{
				UserCode = userCodes.First(),
				StateCode = "code1"
			});
			Synchronizer.Initialize();
		}

		[SetUp]
		public void Setup()
		{
			var persons = Uow.Get(uow => Persons.LoadAll());

			Uow.Do(() =>
			{
				persons.ForEach(p =>
				{
					AgentState.InvalidateSchedules(p.Id.Value, DeadLockVictim.Yes);
				});
			});

		}

		private static IEnumerable<string> userCodes => Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();

		[Test]
		//[ToggleOff(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
		[Setting("OptimizeScheduleChangedEvents_DontUseFromWeb", true)]
		public void Measure(
			[Values(50, 500, 1000)] int batchSize,
			[Values(1, 2, 3, 4, 5)] string variation
		)
		{

			userCodes
				.Batch(batchSize)
				.Select(u => new BatchForTest
				{
					States = u
						.Select(y => new BatchStateForTest
						{
							UserCode = y,
							StateCode = variation.ToString()
						})
						.ToArray()
				}).ForEach(Rta.SaveStateBatch);

		}
	}
}