using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
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
	[InfrastructureTest]
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
	public class MeasureActivityCheckerTest : InfrastructureTestWithOneTimeSetup, ISetup
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public FakeConfigReader Config;
		public ConfigurableSyncEventPublisher Publisher;
		public AgentStateMaintainer Maintainer;
		public StateStreamSynchronizer Synchronizer;
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

		public override void OneTimeSetUp()
		{
			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();
			Publisher.AddHandler<AgentStateMaintainer>();
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithDefaultScenario("default")
				.WithStateGroup("phone")
				.WithStateCode("phone");
			StateCodes().ForEach(x => Database.WithStateGroup($"code{x}").WithStateCode($"code{x}"));
			Enumerable.Range(0, 10).ForEach(x => Database.WithActivity($"activity{x}"));
			var userCodes = UserCodes();

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

			// trigger tick to populate mappings
			Publisher.Publish(new TenantMinuteTickEvent());

			// states for all and init (touch will think its already done)
			userCodes
				.Batch(1000)
				.Select(x => new BatchForTest
				{
					States = x.Select(y => new BatchStateForTest
					{
						UserCode = y,
						StateCode = "phone"
					}).ToArray()
				}).ForEach(Rta.SaveStateBatch);
			Synchronizer.Initialize();
		}

		private static string[] UserCodes()
		{
			return Enumerable.Range(0, 12000).Select(x => $"user{x}").ToArray();
		}

		private static string[] StateCodes()
		{
			return Enumerable.Range(0, 100).Select(x => $"code{x}").ToArray();
		}

		[Test]
		public void MeasureBatch(
			[Values(6, 7, 8)] int parallelTransactions,
			[Values(90, 100, 110)] int transactionSize,
			[Values("A", "B", "C")] string variation
		)
		{
			Config.FakeSetting("RtaActivityChangesParallelTransactions", parallelTransactions.ToString());
			Config.FakeSetting("RtaActivityChangesMaxTransactionSize", transactionSize.ToString());

			Rta.CheckForActivityChanges(DataSourceHelper.TestTenantName);
		}

	}
}