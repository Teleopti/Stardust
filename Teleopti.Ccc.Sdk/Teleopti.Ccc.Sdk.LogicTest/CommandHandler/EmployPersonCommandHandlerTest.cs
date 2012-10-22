﻿using System;
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
        private IPerson _person;
        private PersonDto _personDto;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(_startDate), new DateOnly(_startDate.AddDays(1)));
        private ITeam _team;
        private TeamDto _teamDto;
        private PersonContractDto _personContractDto;
        private EmployPersonCommandDto _employPersonCommandDto;
        private IContract _contract;
        private IContractSchedule _contractSchedule;
        private IPartTimePercentage _partTimePercentage;

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

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());
            _personDto = new PersonDto { Id = Guid.NewGuid() };

            _team = TeamFactory.CreateSimpleTeam("test team");
            _team.SetId(Guid.NewGuid());
            _team.Site = new Site("test site");
			_teamDto = new TeamDto { Id = Guid.NewGuid(), Description = _team.Description.Name, SiteAndTeam = _team .SiteAndTeam};

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
				Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto { DateTime = _dateOnlyPeriod.StartDate }, EndDate = new DateOnlyDto { DateTime = _dateOnlyPeriod .EndDate} },
                PersonContract = _personContractDto,
                Team = _teamDto
            };

            _contract = new Contract("test contract");
            _contract.SetId(Guid.NewGuid());
            _contractSchedule = new ContractSchedule("temp contract schedule");
            _contractSchedule.SetId(Guid.NewGuid());
            _partTimePercentage = new PartTimePercentage("temp part time percentage");
            _partTimePercentage.SetId(Guid.NewGuid());
        }

        [Test]
        public void ShouldHandlerEmployeePersonCommand()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personAssembler.DtoToDomainEntity(_employPersonCommandDto.Person)).Return(_person);
                Expect.Call(_personAssembler.EnableSaveOrUpdate = true);
                Expect.Call(_partTimePercentageRepository.Load(_employPersonCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).Return(_partTimePercentage);
                
                Expect.Call(
                    _contractScheduleRepository.Load(
                        _employPersonCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(
                            _contractSchedule);
                Expect.Call(
                    _teamRepository.Load(
                        _employPersonCommandDto.Team.Id.GetValueOrDefault())).Return(_team);
                Expect.Call(
                    _contractRepository.Load(
                        _employPersonCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(_contract);
                Expect.Call(() => _personRepository.Add(_person));
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
            }
            using(_mock.Playback())
            {
                _target.Handle(_employPersonCommandDto);
            }
        }
    }
}
