using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[PrincipalAndStateTest]
	[Toggle(Toggles.RTA_Optimize_39667)]
	[Toggle(Toggles.RTA_RuleMappingOptimization_39812)]
	public class UnrecognizedStatesTest: ISetup
	{
		public IRtaStateGroupRepository StateGroupRepository;
		public PersonCreator PersonCreatorr;
		public IPrincipalAndStateContext Context;
		public MutableNow Now;
		public TheServiceImpl TheService;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public Database Database;
		public AnalyticsDatabase Analytics;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheServiceImpl>();
			system.AddService<PersonCreator>();
		}

		[Test]
		public void ShouldAddMissingStateCodeToDefaultStateGroup()
		{
			Now.Is("2015-05-13 08:00");
			TheService.DoesWhileLoggedIn(uow =>
			{
				var rtaStateGroup = new RtaStateGroup("Phone", true, true);
				StateGroupRepository.Add(rtaStateGroup);
				PersonCreatorr.CreatePersonWithExternalLogOn(Now, "usercode");
				uow.PersistAll();
			});
			Context.Logout();

			Target.SaveState(new StateInputModel
			{
				AuthenticationKey = "!#¤atAbgT%",
				PlatformTypeId = Guid.Empty.ToString(),
				StateCode = "InCall",
				StateDescription = "InCall",
				UserCode = "usercode",
				SourceId = "-1"
			});

			TheService.DoesWhileNotLoggedIn(uow =>
				StateGroupRepository.LoadAllCompleteGraph()
				.First().StateCollection
				.Any(x => x.StateCode == "InCall")
				.Should().Be.True()
				);
		}

		[Test]
		public void ShouldNotAddDuplicateStateCodes()
		{
			Now.Is("2016-07-11 08:00");
			TheService.DoesWhileLoggedIn(uow =>
			{
				var rtaStateGroup = new RtaStateGroup("Phone", true, true);
				StateGroupRepository.Add(rtaStateGroup);
				20.Times(i => PersonCreatorr.CreatePersonWithExternalLogOn(Now, i.ToString()));
				uow.PersistAll();
			});
			Context.Logout();

			Target.SaveStateBatch(new BatchInputModel
			{
				AuthenticationKey = "!#¤atAbgT%",
				PlatformTypeId = Guid.Empty.ToString(),
				SourceId = "-1",
				States = Enumerable.Range(0, 20)
					.Select(i => new BatchStateInputModel
					{
						StateCode = "InCall",
						StateDescription = "InCall",
						UserCode = i.ToString()
					})
			});

			TheService.DoesWhileNotLoggedIn(uow =>
				StateGroupRepository.LoadAllCompleteGraph().Single().StateCollection.Should().Have.Count.EqualTo(1)
				);
		}

		[Test]
		public void ShouldNotAddDuplicatesInBatch()
		{
			Analytics.WithDataSource(7, new BatchForTest().SourceId);
			Database
				.WithAgent("usercode1")
				.WithAgent("usercode2")
				.WithAgent("usercode3")
				.WithStateGroup("default", true);
			Context.Logout();

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-05-18 08:00".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "phone"
					},
					new BatchStateForTest
					{
						UserCode = "usercode2",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-05-18 08:00".Utc()
			});

			Target.SaveStateBatch(new BatchForTest
			{
				SnapshotId = "2016-05-18 08:05".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode3",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-05-18 08:05".Utc()
			});

			TheService.DoesWhileNotLoggedIn(uow =>
			{
				var stateCodes = from g in StateGroupRepository.LoadAll()
					from s in g.StateCollection
					select s;
				stateCodes.Where(x => x.StateCode == Domain.ApplicationLayer.Rta.Service.Rta.LogOutBySnapshot)
					.Should().Have.Count.EqualTo(1);
			});

		}

		public class TheServiceImpl
		{
			private readonly ICurrentUnitOfWork _uow;
			private readonly IDataSourceScope _dataSource;

			public TheServiceImpl(
				ICurrentUnitOfWork uow,
				IDataSourceScope dataSource)
			{
				_uow = uow;
				_dataSource = dataSource;
			}

			[UnitOfWork]
			public virtual void DoesWhileLoggedIn(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}

			public virtual void DoesWhileNotLoggedIn(Action<IUnitOfWork> action)
			{
				using (_dataSource.OnThisThreadUse(SetupFixtureForAssembly.DataSource.DataSourceName))
					DoesWhileNotLoggedInInner(action);
			}

			[AllBusinessUnitsUnitOfWork]
			protected virtual void DoesWhileNotLoggedInInner(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}
		}

		public class PersonCreator
		{
			private readonly IPersonRepository _personRepository;
			private readonly ISiteRepository _siteRepository;
			private readonly ITeamRepository _teamRepository;
			private readonly IPartTimePercentageRepository _partTimePercentageRepository;
			private readonly IContractRepository _contractRepository;
			private readonly IContractScheduleRepository _contractScheduleRepository;
			private readonly IExternalLogOnRepository _externalLogOnRepository;

			public PersonCreator(
				IPersonRepository personRepository,
				ISiteRepository siteRepository,
				ITeamRepository teamRepository,
				IPartTimePercentageRepository partTimePercentageRepository,
				IContractRepository contractRepository,
				IContractScheduleRepository contractScheduleRepository,
				IExternalLogOnRepository externalLogOnRepository)
			{
				_personRepository = personRepository;
				_siteRepository = siteRepository;
				_teamRepository = teamRepository;
				_partTimePercentageRepository = partTimePercentageRepository;
				_contractRepository = contractRepository;
				_contractScheduleRepository = contractScheduleRepository;
				_externalLogOnRepository = externalLogOnRepository;
			}


			public IPerson CreatePersonWithExternalLogOn(INow now, string externalLogon)
			{
				var site = new Site("site");
				_siteRepository.Add(site);
				var team = new Team { Site = site, Description = new Description("team") };
				_teamRepository.Add(team);
				var contract = new Contract("c");
				_contractRepository.Add(contract);
				var partTimePercentage = new PartTimePercentage("p");
				_partTimePercentageRepository.Add(partTimePercentage);
				var contractSchedule = new ContractSchedule("cs");
				_contractScheduleRepository.Add(contractSchedule);
				var externalLogOn = new ExternalLogOn(1, 1, externalLogon, externalLogon, true) { DataSourceId = -1 };
				_externalLogOnRepository.Add(externalLogOn);
				var person = new Person();
				person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
				var personPeriod = new PersonPeriod(new DateOnly(now.UtcDateTime()),
					new PersonContract(contract, partTimePercentage, contractSchedule), team);
				personPeriod.AddExternalLogOn(externalLogOn);
				person.AddPersonPeriod(personPeriod);
				_personRepository.Add(person);

				return person;
			}
		}
	}
}