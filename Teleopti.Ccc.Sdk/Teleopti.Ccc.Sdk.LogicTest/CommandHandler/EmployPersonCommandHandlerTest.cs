using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class EmployPersonCommandHandlerTest
    {
        private MockRepository _mock;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IPersonRepository _personRepository;
        private IPersonAssembler _personAssembler;
        private IPartTimePercentageRepository _partTimePercentageRepository;
        private IContractScheduleRepository _contractScheduleRepository;
        private IContractRepository _contractRepository;
        private ITeamRepository _teamRepository;
        private EmployPersonCommandHandler _target;
        private IPerson _person;
        private PersonDto _personDto;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly DateOnlyPeriod _dateOnlyPeriod =
            new DateOnlyPeriod(new DateOnly(_startDate), new DateOnly(_startDate.AddDays(1)));

        private ITeam _team;
        private TeamDto _teamDto;
        private PersonContractDto _personContractDto;
        private EmployPersonCommandDto _employPersonCommandDto;
        private IContract _contract;
        private IContractSchedule _contractSchedule;
        private IPartTimePercentage _partTimePercentage;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _personAssembler = _mock.StrictMock<IPersonAssembler>();
            _partTimePercentageRepository = _mock.StrictMock<IPartTimePercentageRepository>();
            _contractScheduleRepository = _mock.StrictMock<IContractScheduleRepository>();
            _contractRepository = _mock.StrictMock<IContractRepository>();
            _teamRepository = _mock.StrictMock<ITeamRepository>();
            _target = new EmployPersonCommandHandler(_currentUnitOfWorkFactory, _personRepository, _personAssembler,
                _partTimePercentageRepository, _contractScheduleRepository, _contractRepository, _teamRepository, new FullPermission(), new SpecificBusinessUnit(BusinessUnitUsedInTests.BusinessUnit));

            _person = PersonFactory.CreatePerson("test").WithId();
            _personDto = new PersonDto {Id = Guid.NewGuid()};

            _team = TeamFactory.CreateTeamWithId(Guid.NewGuid(), "test team");
            _team.Site = new Site("test site");
            _teamDto = new TeamDto
            {
                Id = _team.Id,
                Description = _team.Description.Name,
                SiteAndTeam = _team.SiteAndTeam
            };

            _personContractDto = new PersonContractDto
            {
                Id = Guid.NewGuid(),
                ContractScheduleId = Guid.NewGuid(),
                ContractId = Guid.NewGuid(),
                PartTimePercentageId = Guid.NewGuid()
            };

            _employPersonCommandDto = new EmployPersonCommandDto
            {
                Person = _personDto,
                Period = new DateOnlyPeriodDto
                {
                    StartDate = new DateOnlyDto {DateTime = _dateOnlyPeriod.StartDate.Date},
                    EndDate = new DateOnlyDto {DateTime = _dateOnlyPeriod.EndDate.Date}
                },
                PersonContract = _personContractDto,
                Team = _teamDto
            };

            _contract = new Contract("test contract").WithId();
            _contractSchedule = new ContractSchedule("temp contract schedule").WithId();
            _partTimePercentage = new PartTimePercentage("temp part time percentage").WithId();
        }

        [Test]
        public void ShouldHandlerEmployeePersonCommand()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personAssembler.DtoToDomainEntity(_employPersonCommandDto.Person)).Return(_person);
                Expect.Call(_personAssembler.EnableSaveOrUpdate = true);
                Expect.Call(_partTimePercentageRepository.Load(_employPersonCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                    .Return(_partTimePercentage);

                Expect.Call(_contractScheduleRepository.Load(_employPersonCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                    .Return(_contractSchedule);
                Expect.Call(_teamRepository.Load(_employPersonCommandDto.Team.Id.GetValueOrDefault())).Return(_team);
                Expect.Call(_contractRepository.Load(_employPersonCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(_contract);
                Expect.Call(() => _personRepository.Add(_person));
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
            }

            using (_mock.Playback())
			{
				_target.Handle(_employPersonCommandDto);
			}
        }

        [Test]
        public void ShouldRaiseExceptionWhenNewTeamIsInAnotherBusinessUnit()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();

            var businessUnitX = BusinessUnitFactory.CreateWithId("BusinessUnit X");
            var siteX = SiteFactory.CreateSiteWithId(Guid.NewGuid(), "Site X");
            siteX.SetBusinessUnit(businessUnitX);
            var teamX = TeamFactory.CreateTeamWithId(Guid.NewGuid(), "Team X");
            teamX.Site = siteX;

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personAssembler.DtoToDomainEntity(_employPersonCommandDto.Person)).Return(_person);
                Expect.Call(_personAssembler.EnableSaveOrUpdate = true);
                Expect.Call(_partTimePercentageRepository.Load(_employPersonCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                    .Return(_partTimePercentage);

                Expect.Call(_contractScheduleRepository.Load(_employPersonCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                    .Return(_contractSchedule);
                Expect.Call(_teamRepository.Load(_team.Id.GetValueOrDefault())).Return(_team);
                Expect.Call(_teamRepository.Load(teamX.Id.GetValueOrDefault())).Return(teamX);
                Expect.Call(_contractRepository.Load(_employPersonCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(_contract);
                Expect.Call(() => _personRepository.Add(_person));
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
            }

            var ex = Assert.Throws<FaultException>(() =>
            {
                using (_mock.Playback())
                {
                    _employPersonCommandDto.Team = new TeamDto
                    {
                        Id = teamX.Id,
                        Description = teamX.Description.Name,
                        SiteAndTeam = teamX.SiteAndTeam
                    };

					_target.Handle(_employPersonCommandDto);
				}
            });
            ex.Message.Should().Contain("Adding references to items from a different business unit than the currently specified in the header is not allowed");
        }
    }
}
