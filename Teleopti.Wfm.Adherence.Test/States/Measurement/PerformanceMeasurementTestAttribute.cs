using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Measurement
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

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);

			extend.AddService(this);
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			//?????????????????????????????????
			if (QueryAllAttributes<RealHangfireAttribute>().Any())
				isolate.UseTestDouble<FakeEventPublisher>().For<FakeEventPublisher>();
			else
				isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		protected override void BeforeTest()
		{
			InfrastructureTestSetup.Before();

			base.BeforeTest();

			FakePublisher.AddHandler<PersonAssociationChangedEventPublisher>();
			FakePublisher.AddHandler<AgentStateMaintainer>();
			FakePublisher.AddHandler<MappingReadModelUpdater>();
			FakePublisher.AddHandler<ExternalLogonReadModelUpdater>();
			FakePublisher.AddHandler<ScheduleChangeProcessor>();
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			InfrastructureTestSetup.After();
		}

		public IEnumerable<int> ParallelTransactions() => new[] {5, 6, 7, 8, 9};
		public IEnumerable<int> TransactionSize() => new[] {50, 80, 100, 120, 150};
		public IEnumerable<int> BatchSize() => new[] {50, 250, 500, 750, 1000};
		public IEnumerable<string> Variation() => new[] {"A", "B", "C"};

		public void MakeUsersFaster(IEnumerable<string> userCodes)
		{
			Contract contract = null;
			PartTimePercentage partTimePercentage = null;
			ContractSchedule contractSchedule = null;
			Team team = null;

			Uow.Do(() =>
			{
				contract = new Contract(RandomName.Make());
				Contracts.Add(contract);

				partTimePercentage = new PartTimePercentage(RandomName.Make());
				PartTimePercentages.Add(partTimePercentage);

				contractSchedule = new ContractSchedule(RandomName.Make());
				ContractSchedules.Add(contractSchedule);

				var site = new Site(RandomName.Make());
				Sites.Add(site);

				team = new Team {Site = site}
					.WithDescription(new Description(RandomName.Make()));
				Teams.Add(team);
				site.AddTeam(team);
			});

			userCodes.Batch(5_000).ForEach(names =>
			{
				Uow.Do(() =>
				{
					names.ForEach(name =>
					{
						var person = new Person().WithName(new Name(name, name));
						person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);

						var personContract = new PersonContract(
							contract,
							partTimePercentage,
							contractSchedule);

						var personPeriod = new PersonPeriod(new Ccc.Domain.InterfaceLegacy.Domain.DateOnly("2001-01-01".Utc()), personContract, team);
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
			});
		}
	}
}