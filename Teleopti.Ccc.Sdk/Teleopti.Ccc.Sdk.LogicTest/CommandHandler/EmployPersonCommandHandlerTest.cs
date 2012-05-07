using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _personAssembler = _mock.StrictMock<IPersonAssembler>();
            _partTimePercentageRepository = _mock.StrictMock<IPartTimePercentageRepository>();
            _contractScheduleRepository = _mock.StrictMock<IContractScheduleRepository>();
            _contractRepository = _mock.StrictMock<IContractRepository>();
            _teamRepository = _mock.StrictMock<ITeamRepository>();
            _target = new EmployPersonCommandHandler(_unitOfWorkFactory,_personRepository,_personAssembler,_partTimePercentageRepository,_contractScheduleRepository,_contractRepository,_teamRepository);
        }

        [Test]
        public void ShouldHandlerEmployeePersonCommand()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var personDto = new PersonDto {Id = Guid.NewGuid()};
            var startDate = new DateTime(2012, 1, 1,0,0,0,DateTimeKind.Utc);
            var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(startDate.AddDays(1)));

            var team = TeamFactory.CreateSimpleTeam("test team");
            team.SetId(Guid.NewGuid());
            team.Site = new Site("test site");
            var teamDto = new TeamDto(team) {Id = Guid.NewGuid()};

            var personContractDto = new PersonContractDto
                                        {
                                            Id = Guid.NewGuid(),
                                            ContractScheduleId = Guid.NewGuid(),
                                            ContractId = Guid.NewGuid(),
                                            PartTimePercentageId = Guid.NewGuid()
                                        };

            var employPersonCommandDto = new EmployPersonCommandDto
                                             {
                                                 Person = personDto,
                                                 Period = new DateOnlyPeriodDto(dateOnlyPeriod),
                                                 PersonContract = personContractDto,
                                                 Team = teamDto
                                             };

            var contract = new Contract("test contract");
            contract.SetId(Guid.NewGuid());
            var contractSchedule = new ContractSchedule("temp contract schedule");
            contractSchedule.SetId(Guid.NewGuid());
            var partTimePercentage = new PartTimePercentage("temp part time percentage");
            partTimePercentage.SetId(Guid.NewGuid());
            
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personAssembler.DtoToDomainEntity(employPersonCommandDto.Person)).Return(person);
                Expect.Call(_personAssembler.EnableSaveOrUpdate = true);
                Expect.Call(_partTimePercentageRepository.Load(employPersonCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).Return(partTimePercentage);
                
                Expect.Call(
                    _contractScheduleRepository.Load(
                        employPersonCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(
                            contractSchedule);
                Expect.Call(
                    _teamRepository.Load(
                        employPersonCommandDto.Team.Id.GetValueOrDefault())).Return(team);
                Expect.Call(
                    _contractRepository.Load(
                        employPersonCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(contract);
                Expect.Call(() => _personRepository.Add(person));
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
            }
            using(_mock.Playback())
            {
                _target.Handle(employPersonCommandDto);
            }
        }
    }
}
