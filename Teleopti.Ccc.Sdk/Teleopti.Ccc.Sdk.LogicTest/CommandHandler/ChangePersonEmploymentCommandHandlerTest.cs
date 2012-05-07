using System;
using System.Collections.Generic;
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
    public class ChangePersonEmploymentCommandHandlerTest
    {
        private MockRepository _mock;
        private IAssembler<IPersonPeriod, PersonSkillPeriodDto> _personSkillPeriodAssembler;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISkillRepository _skillRepository;
        private IExternalLogOnRepository _externalLogOnRepository;
        private IPersonRepository _personRepository;
        private ITeamRepository _teamRepository;
        private IPartTimePercentageRepository _partTimePercentageRepository;
        private IContractScheduleRepository _contractScheduleRepository;
        private IContractRepository _contractRepository;
        private ChangePersonEmploymentCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personSkillPeriodAssembler = _mock.StrictMock<IAssembler<IPersonPeriod,PersonSkillPeriodDto>>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _skillRepository = _mock.StrictMock<ISkillRepository>();
            _externalLogOnRepository = _mock.StrictMock<IExternalLogOnRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _teamRepository = _mock.StrictMock<ITeamRepository>();
            _partTimePercentageRepository = _mock.StrictMock<IPartTimePercentageRepository>();
            _contractScheduleRepository = _mock.StrictMock<IContractScheduleRepository>();
            _contractRepository = _mock.StrictMock<IContractRepository>();
            _target = new ChangePersonEmploymentCommandHandler(_personSkillPeriodAssembler,_unitOfWorkFactory,_skillRepository,_externalLogOnRepository,_personRepository,_teamRepository,_partTimePercentageRepository,_contractScheduleRepository,_contractRepository);
        }

        [Test]
        public void ShouldUpdateExistingPersonPeriodIfItExists()
        {
            var externalLogOnDto = new ExternalLogOnDto {Id = Guid.NewGuid()};
            var externalLogOnList = new List<ExternalLogOnDto> {externalLogOnDto};

            var dateOnlyPeriodDto = new DateOnlyPeriodDto(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
            var personDto = new PersonDto {Id = Guid.NewGuid()};

            var personContractDto = new PersonContractDto
                                        {
                                            Id = Guid.NewGuid(),
                                            ContractScheduleId = Guid.NewGuid(),
                                            ContractId = Guid.NewGuid(),
                                            PartTimePercentageId = Guid.NewGuid()
                                        };

            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var personSkillPeriodDto = new PersonSkillPeriodDto
                                           {
                                               DateFrom = new DateOnlyDto(DateOnly.Today),
                                               DateTo = new DateOnlyDto(DateOnly.Today),
                                               Id = Guid.NewGuid(),
                                               PersonId = person.Id.GetValueOrDefault()
                                           };

            var personSkillPeriodCollection = new List<PersonSkillPeriodDto> {personSkillPeriodDto};

            var team = TeamFactory.CreateSimpleTeam("Test Team");
            team.SetId(Guid.NewGuid());
            team.Site = new Site("test site");
            var teamDto = new TeamDto(team) {Id = Guid.NewGuid()};

            var skill = SkillFactory.CreateSkill("TestSkill");
            skill.SetId(Guid.NewGuid());

            var changePersonEmploymentCommandDto = new ChangePersonEmploymentCommandDto
                                                       {
                                                           ExternalLogOn = externalLogOnList,
                                                           Period = dateOnlyPeriodDto,
                                                           Person = personDto,
                                                           PersonContract = personContractDto,
                                                           PersonSkillPeriodCollection = personSkillPeriodCollection,
                                                           Team = teamDto
                                                       };

            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
            person.AddPersonPeriod(personPeriod);

            var contract = new Contract("test contract");
            contract.SetId(Guid.NewGuid());
            var contractSchedule = new ContractSchedule("temp contract schedule");
            contractSchedule.SetId(Guid.NewGuid());
            var partTimePercentage = new PartTimePercentage("temp part time percentage");
            partTimePercentage.SetId(Guid.NewGuid());

            var unitOfWork = _mock.StrictMock<IUnitOfWork>();

            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);

                Expect.Call(_personRepository.Get(changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).
                    Return(person);

                Expect.Call(
                    _partTimePercentageRepository.Load(
                        changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).
                    Return(partTimePercentage);

                Expect.Call(
                    _contractRepository.Load(
                        changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(
                            contract);

                Expect.Call(
                    _contractScheduleRepository.Load(
                        changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(
                            contractSchedule);
                
                Expect.Call(_teamRepository.Load(changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault())).Return(
                    team);
                
                Expect.Call(_externalLogOnRepository.LoadAllExternalLogOns()).Return(new List<IExternalLogOn>());
                Expect.Call((() => unitOfWork.PersistAll()));
                Expect.Call(unitOfWork.Dispose);
            }
            using(_mock.Playback())
            {
                _target.Handle(changePersonEmploymentCommandDto);
            }
        }

        [Test]
        public void ShouldAddNewPersonPeriodIfExistingPersonPeriodNotExists()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();

            var externalLogOnDto = new ExternalLogOnDto
                                       {Id = Guid.NewGuid()};
            var externalLogOnList = new List<ExternalLogOnDto> { externalLogOnDto };

            var dateOnlyPeriodDto = new DateOnlyPeriodDto(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
            var personDto = new PersonDto {Id = Guid.NewGuid()};

            var personContractDto = new PersonContractDto
                                        {
                                            Id = Guid.NewGuid(),
                                            ContractScheduleId = Guid.NewGuid(),
                                            ContractId = Guid.NewGuid(),
                                            PartTimePercentageId = Guid.NewGuid()
                                        };

            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var personSkillPeriodDto = new PersonSkillPeriodDto
                                           {
                                               DateFrom = new DateOnlyDto(DateOnly.Today),
                                               DateTo = new DateOnlyDto(DateOnly.Today),
                                               Id = Guid.NewGuid(),
                                               PersonId = person.Id.GetValueOrDefault()
                                           };

            var personSkillPeriodCollection = new List<PersonSkillPeriodDto> {personSkillPeriodDto};

            var team = TeamFactory.CreateSimpleTeam("Test Team");
            team.SetId(Guid.NewGuid());
            team.Site = new Site("test site");
            var teamDto = new TeamDto(team) {Id = Guid.NewGuid()};

            var skill = SkillFactory.CreateSkill("TestSkill");
            skill.SetId(Guid.NewGuid());

            var changePersonEmploymentCommandDto = new ChangePersonEmploymentCommandDto
                                                       {
                                                           ExternalLogOn = externalLogOnList,
                                                           Period = dateOnlyPeriodDto,
                                                           Person = personDto,
                                                           PersonContract = personContractDto,
                                                           PersonSkillPeriodCollection = personSkillPeriodCollection,
                                                           Team = teamDto
                                                       };

            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            person.AddPersonPeriod(personPeriod);

            var contract = new Contract("test contract");
            contract.SetId(Guid.NewGuid());
            var contractSchedule = new ContractSchedule("temp contract schedule");
            contractSchedule.SetId(Guid.NewGuid());
            var partTimePercentage = new PartTimePercentage("temp part time percentage");
            partTimePercentage.SetId(Guid.NewGuid());

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);

                Expect.Call(_personRepository.Get(changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).
                    Return(person);

                Expect.Call(_teamRepository.Load(changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault())).Return(
                    team);

                Expect.Call(
                    _partTimePercentageRepository.Load(
                        changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).
                    Return(partTimePercentage);

                Expect.Call(
                    _contractRepository.Load(
                        changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(
                            contract);

                Expect.Call(
                    _contractScheduleRepository.Load(
                        changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(
                            contractSchedule);

                Expect.Call(_personSkillPeriodAssembler.DomainEntityToDto(personPeriod)).IgnoreArguments().Return(
                    new PersonSkillPeriodDto());

                Expect.Call(_externalLogOnRepository.LoadAllExternalLogOns()).Return(new List<IExternalLogOn>());
                Expect.Call((() => unitOfWork.PersistAll()));
                Expect.Call(unitOfWork.Dispose);
            
            }
            using(_mock.Playback())
            {
                _target.Handle(changePersonEmploymentCommandDto);
            }


        }

    }
}
