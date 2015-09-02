using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class InfrastructureTest : ISetup
	{
		public IRtaStateGroupRepository StateGroupRepository;
		public PersonCreator PersonCreator;
		public IPrincipalAndStateContext Context;
		public MutableNow Now;
		public TheServiceImpl TheService;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheServiceImpl>();
			system.AddService<PersonCreator>();
		}

		public class TheServiceImpl
		{
			private readonly ICurrentUnitOfWork _uow;

			public TheServiceImpl(ICurrentUnitOfWork uow)
			{
				_uow = uow;
			}

			[UnitOfWork]
			public virtual void DoesWhileLoggedIn(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}

			[AllBusinessUnitsUnitOfWork]
			public virtual void DoesWhileNotLoggedIn(Action<IUnitOfWork> action)
			{
				action(_uow.Current());
			}
		}

		[Test]
		public void ShouldAddMissingStateCodeToDefaultStateGroup()
		{
			Now.Is("2015-05-13 08:00");
			TheService.DoesWhileLoggedIn(uow =>
			{
				var rtaStateGroup = new RtaStateGroup("Phone", true, true);
				StateGroupRepository.Add(rtaStateGroup);
				PersonCreator.CreatePersonWithExternalLogOn(Now, "usercode");
				uow.PersistAll();
			});
			Context.Logout();

			Target.SaveState(new ExternalUserStateInputModel
			{
				AuthenticationKey = "!#¤atAbgT%",
				PlatformTypeId = Guid.Empty.ToString(),
				StateCode = "InCall",
				StateDescription = "InCall",
				UserCode = "usercode",
				SourceId = "-1",
				IsLoggedOn = true
			});

			TheService.DoesWhileNotLoggedIn(uow => 
				StateGroupRepository.LoadAllCompleteGraph()
				.First().StateCollection
				.Any(x => x.StateCode == "InCall")
				.Should().Be.True()
				);
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