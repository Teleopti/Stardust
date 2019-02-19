using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;
using Description = Teleopti.Wfm.Adherence.Configuration.Description;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service
{
	[TestFixture]
	[DatabaseTest]
	public class UnrecognizedStatesTest : IIsolateSystem, IExtendSystem
	{
		public IRtaStateGroupRepository StateGroupRepository;
		public PersonCreator PersonCreatorr;
		public ILogOnOffContext Context;
		public MutableNow Now;
		public TheServiceImpl TheService;
		public Rta Target;
		public Database Database;
		public AnalyticsDatabase Analytics;
		public FakeEventPublisher Publisher;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<TheServiceImpl>();
			extend.AddService<PersonCreator>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldAddMissingStateCodeToDefaultStateGroup()
		{
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			Publisher.AddHandler(typeof(CurrentScheduleReadModelUpdater));
			Publisher.AddHandler(typeof(ExternalLogonReadModelUpdater));
			Now.Is("2015-05-13 08:00");
			TheService.DoesWhileLoggedIn(uow =>
			{
				var rtaStateGroup = new RtaStateGroup("Phone", true, true);
				StateGroupRepository.Add(rtaStateGroup);
				PersonCreatorr.CreatePersonWithExternalLogOn(Now, "usercode");
				uow.PersistAll();
			});
			Database.PublishRecurringEvents();
			Context.Logout();

			Target.ProcessState(new StateForTest
			{
				AuthenticationKey = "!#¤atAbgT%",
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
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			Publisher.AddHandler(typeof(CurrentScheduleReadModelUpdater));
			Publisher.AddHandler(typeof(ExternalLogonReadModelUpdater));
			Now.Is("2016-07-11 08:00");
			TheService.DoesWhileLoggedIn(uow =>
			{
				var rtaStateGroup = new RtaStateGroup("Phone", true, true);
				StateGroupRepository.Add(rtaStateGroup);
				20.Times(i => PersonCreatorr.CreatePersonWithExternalLogOn(Now, i.ToString()));
				uow.PersistAll();
			});
			Database.PublishRecurringEvents();
			Context.Logout();

			Target.Process(new BatchInputModel
			{
				AuthenticationKey = "!#¤atAbgT%",
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
				using (_dataSource.OnThisThreadUse(InfraTestConfigReader.TenantName()))
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
				var team = new Team { Site = site }.WithDescription(new Ccc.Domain.InterfaceLegacy.Domain.Description("team"));
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
				var personPeriod = new PersonPeriod(new Ccc.Domain.InterfaceLegacy.Domain.DateOnly(now.UtcDateTime()), new PersonContract(contract, partTimePercentage, contractSchedule), team);
				personPeriod.AddExternalLogOn(externalLogOn);
				person.AddPersonPeriod(personPeriod);
				_personRepository.Add(person);

				return person;
			}
		}
	}
}