using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
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
	//[Category("LongRunning")]
	public class PerformanceMeasurementTest : ISetup
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public FakeConfigReader Config;
		public ConfigurableSyncEventPublisher Publisher;
		public AgentStateMaintainer Maintainer;
		public MutableNow Now;
		public WithUnitOfWork Uow;
		public IPersonRepository Persons;
		public IActivityRepository Activities;
		public IPersonAssignmentRepository PersonAssignments;
		public IScenarioRepository Scenarios;
		public IAgentStatePersister AgentState;
		public IContractRepository Contracts;
		public IPartTimePercentageRepository PartTimePercentages;
		public IContractScheduleRepository ContractSchedules;
		public IExternalLogOnRepository ExternalLogOns;
		public ITeamRepository Teams;
		public ISiteRepository Sites;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ConfigurableSyncEventPublisher>().For<IEventPublisher>();
		}
		
		[Test]
		[ToggleOff(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
		[Setting("OptimizeScheduleChangedEvents_DontUseFromWeb", true)]
		public void MeasureScheduleLoading()
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
			var userCodes = Enumerable.Range(0, 1000).Select(x => $"user{x}").ToArray();
			Uow.Do(() =>
			{

				var contract = new Contract(RandomName.Make());
				Contracts.Add(contract);

				var partTimePercentage = new PartTimePercentage(RandomName.Make());
				PartTimePercentages.Add(partTimePercentage);

				var contractSchedule = new ContractSchedule(RandomName.Make());
				ContractSchedules.Add(contractSchedule);

				var site = new Site(RandomName.Make());
				Sites.Add(site);

				var team = new Team
				{
					Description = new Description(RandomName.Make()),
					Site = site
				};
				Teams.Add(team);
				site.AddTeam(team);

				userCodes.ForEach(name =>
				{
					var person = new Person { Name = new Name(name, name) };
					person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

					var personContract = new PersonContract(
						contract,
						partTimePercentage,
						contractSchedule);

					var personPeriod = new PersonPeriod("2001-01-01".Date(), personContract, team);
					person.AddPersonPeriod(personPeriod);

					var exteralLogOn = new ExternalLogOn
					{
						//AcdLogOnName = name, // is not used?
						AcdLogOnMartId = -1, // NotDefined should be there, 0 probably wont
						DataSourceId = Analytics.CurrentDataSourceId,
						AcdLogOnOriginalId = name // this is what the rta receives
					};
					ExternalLogOns.Add(exteralLogOn);
					person.AddExternalLogOn(exteralLogOn, personPeriod);

					Persons.Add(person);
				});
			});

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
			Publisher.Publish(new TenantMinuteTickEvent());

			var results = (
				from variation in Enumerable.Range(1, 5)
				select new {variation}).Select(x =>
			{
				Uow.Do(() =>
				{
					persons.ForEach(p =>
					{
						AgentState.InvalidateSchedules(p.Id.Value, DeadLockVictim.Yes);
					});
				});

				var timer = new Stopwatch();
				timer.Start();

				userCodes
					.Batch(50)
					.Select(u => new BatchForTest
					{
						States = u
							.Select(y => new BatchStateForTest
							{
								UserCode = y,
								StateCode = x.variation.ToString()
							})
							.ToArray()
					}).ForEach(Rta.SaveStateBatch);

				timer.Stop();
				return new
				{
					timer.Elapsed,
					x.variation
				};
			});

			results
				.OrderBy(x => x.Elapsed)
				.ForEach(x => Console.WriteLine($"{x.Elapsed} - {x.variation}"));			
		}
	}
}