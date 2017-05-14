using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Service.PerformanceMeasurement
{
	public class PerformanceMeasurementTestAttribute : InfrastructureTestAttribute
	{
		public FakeEventPublisher FakePublisher;
		public AnalyticsDatabase Analytics;
		public WithUnitOfWork Uow;
		public IPersonRepository Persons;
		public IContractRepository Contracts;
		public IPartTimePercentageRepository PartTimePercentages;
		public IContractScheduleRepository ContractSchedules;
		public IExternalLogOnRepository ExternalLogOns;
		public ITeamRepository Teams;
		public ISiteRepository Sites;
		public HangfireClientStarter HangfireClientStarter;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			if (QueryAllAttributes<RealHangfireAttribute>().Any())
				system.UseTestDouble<FakeEventPublisher>().For<FakeEventPublisher>();
			else
				system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();

			system.AddService(this);
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			if (QueryAllAttributes<RealHangfireAttribute>().Any())
				HangfireClientStarter.Start();

			FakePublisher.AddHandler<PersonAssociationChangedEventPublisher>();
			FakePublisher.AddHandler<AgentStateMaintainer>();
			FakePublisher.AddHandler<MappingReadModelUpdater>();
			FakePublisher.AddHandler<ExternalLogonReadModelUpdater>();
			FakePublisher.AddHandler<ScheduleChangeProcessor>();
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			SetupFixtureForAssembly.RestoreCcc7Database();
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}

		public IEnumerable<int> ParallelTransactions() => new[] { 5, 6, 7, 8, 9 };
		public IEnumerable<int> TransactionSize() => new[] { 50, 80, 100, 120, 150 };
		public IEnumerable<int> BatchSize() => new[] { 50, 250, 500, 750, 1000 };
		public IEnumerable<string> Variation() => new[] {"A", "B", "C"};

		public void MakeUsersFaster(IEnumerable<string> userCodes)
		{

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

				var team = new Team { Site = site }
					.WithDescription(new Description(RandomName.Make()));
				Teams.Add(team);
				site.AddTeam(team);

				userCodes.ForEach(name =>
				{
					var person = new Person().WithName(new Name(name, name));
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
		}

	}
}