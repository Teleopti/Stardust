﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
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
        private ExternalLogOnDto _externalLogOnDto;
        private IList<ExternalLogOnDto> _externalLogOnList;
        private readonly DateOnlyPeriodDto _dateOnlyPeriodDto = new DateOnlyPeriodDto(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
        private PersonDto _personDto;
        private PersonContractDto _personContractDto;
        private IPerson _person;
        private PersonSkillPeriodDto _personSkillPeriodDto;
        private IList<PersonSkillPeriodDto> _personSkillPeriodCollection;
        private ITeam _team;
        private TeamDto _teamDto;
        private ISkill _skill;
        private ChangePersonEmploymentCommandDto _changePersonEmploymentCommandDto;
        private IContract _contract;
        private IContractSchedule _contractSchedule;
        private IPartTimePercentage _partTimePercentage;


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

            _externalLogOnDto = new ExternalLogOnDto { Id = Guid.NewGuid(), AcdLogOnOriginalId = "test Id", AcdLogOnName="test Acd"};
            _externalLogOnList = new List<ExternalLogOnDto> { _externalLogOnDto };
            _personDto = new PersonDto { Id = Guid.NewGuid() };

            _personContractDto = new PersonContractDto
            {
                Id = Guid.NewGuid(),
                ContractScheduleId = Guid.NewGuid(),
                ContractId = Guid.NewGuid(),
                PartTimePercentageId = Guid.NewGuid()
            };

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());

            _personSkillPeriodDto = new PersonSkillPeriodDto
            {
                DateFrom = new DateOnlyDto(DateOnly.Today),
                DateTo = new DateOnlyDto(DateOnly.Today),
                Id = Guid.NewGuid(),
                PersonId = _person.Id.GetValueOrDefault()
            };

            _personSkillPeriodCollection = new List<PersonSkillPeriodDto> { _personSkillPeriodDto };

            _team = TeamFactory.CreateSimpleTeam("Test Team");
            _team.SetId(Guid.NewGuid());
            _team.Site = new Site("test site");
            _teamDto = new TeamDto(_team) { Id = Guid.NewGuid() };

            _skill = SkillFactory.CreateSkill("TestSkill");
            _skill.SetId(Guid.NewGuid());

            _changePersonEmploymentCommandDto = new ChangePersonEmploymentCommandDto
            {
                ExternalLogOn = _externalLogOnList,
                Period = _dateOnlyPeriodDto,
                Person = _personDto,
                PersonContract = _personContractDto,
                PersonSkillPeriodCollection = _personSkillPeriodCollection,
                Team = _teamDto,
                Note = "test Note"
            };

            _contract = new Contract("test contract");
            _contract.SetId(Guid.NewGuid());
            _contractSchedule = new ContractSchedule("temp contract schedule");
            _contractSchedule.SetId(Guid.NewGuid());
            _partTimePercentage = new PartTimePercentage("temp part time percentage");
            _partTimePercentage.SetId(Guid.NewGuid());
        }

        [Test]
        public void ShouldUpdateExistingPersonPeriodIfItExists()
        {
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
            _person.AddPersonPeriod(personPeriod);
            
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();

            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).
                    Return(_person);
                Expect.Call(
                    _partTimePercentageRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).
                    Return(_partTimePercentage);
                Expect.Call(
                    _contractRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(
                            _contract);
                Expect.Call(
                    _contractScheduleRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(
                            _contractSchedule);              
                Expect.Call(_teamRepository.Load(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault())).Return(
                    _team);
                Expect.Call(_externalLogOnRepository.LoadAllExternalLogOns()).Return(new List<IExternalLogOn>());
                Expect.Call((() => unitOfWork.PersistAll()));
                Expect.Call(unitOfWork.Dispose);
            }
            using(_mock.Playback())
            {
                _target.Handle(_changePersonEmploymentCommandDto);
            }
        }

        [Test]
        public void ShouldAddNewPersonPeriodIfExistingPersonPeriodNotExists()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            personPeriod.AddExternalLogOn(new ExternalLogOn(1,1,"test Id","test acd",true));
            _person.AddPersonPeriod(personPeriod);
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).
                    Return(_person);
                Expect.Call(_teamRepository.Load(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault())).Return(
                    _team);
                Expect.Call(
                    _partTimePercentageRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).
                    Return(_partTimePercentage);
                Expect.Call(
                    _contractRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(
                            _contract);
                Expect.Call(
                    _contractScheduleRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(
                            _contractSchedule);
                Expect.Call(_personSkillPeriodAssembler.DomainEntityToDto(personPeriod)).IgnoreArguments().Return(
                    new PersonSkillPeriodDto());
                Expect.Call(_externalLogOnRepository.LoadAllExternalLogOns()).Return(new List<IExternalLogOn>());
                Expect.Call((() => unitOfWork.PersistAll()));
                Expect.Call(unitOfWork.Dispose);
            
            }
            using(_mock.Playback())
            {
                _target.Handle(_changePersonEmploymentCommandDto);
            }
        }

        [Test]
        public void ShouldResetExternalLogOnsAndPersonSkillsIfPersonPeriodIsNew()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            personPeriod.AddExternalLogOn(new ExternalLogOn(1, 1, "test Id", "test acd", true));
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).
                    Return(_person);
                Expect.Call(_teamRepository.Load(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault())).Return(
                    _team);
                Expect.Call(
                    _partTimePercentageRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).
                    Return(_partTimePercentage);
                Expect.Call(
                    _contractRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(
                            _contract);
                Expect.Call(
                    _contractScheduleRepository.Load(
                        _changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(
                            _contractSchedule);
                Expect.Call(_externalLogOnRepository.LoadAllExternalLogOns()).Return(new List<IExternalLogOn>());
                Expect.Call((() => unitOfWork.PersistAll()));
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(_changePersonEmploymentCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfPersonContractOrTeamIsNull()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            personPeriod.AddExternalLogOn(new ExternalLogOn(1, 1, "test Id", "test acd", true));
            _changePersonEmploymentCommandDto.Team = null;

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).
                    Return(_person);
                Expect.Call((() => unitOfWork.PersistAll()));
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(_changePersonEmploymentCommandDto);
            }
        }
    }
}
