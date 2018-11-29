using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    public class ChangePersonEmploymentCommandHandlerTest
    {
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

	    private DateOnlyPeriodDto _dateOnlyPeriodDto;
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
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
	        _dateOnlyPeriodDto = new DateOnlyPeriodDto
	        {
		        StartDate = new DateOnlyDto {DateTime = DateTime.Today},
		        EndDate = new DateOnlyDto {DateTime = DateTime.Today}
	        };
            _personSkillPeriodAssembler =  MockRepository.GenerateMock<IAssembler<IPersonPeriod,PersonSkillPeriodDto>>();
            _unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
            _skillRepository = MockRepository.GenerateMock<ISkillRepository>();
            _externalLogOnRepository = MockRepository.GenerateMock<IExternalLogOnRepository>();
            _personRepository = MockRepository.GenerateMock<IPersonRepository>();
            _teamRepository = MockRepository.GenerateMock<ITeamRepository>();
            _partTimePercentageRepository = MockRepository.GenerateMock<IPartTimePercentageRepository>();
            _contractScheduleRepository = MockRepository.GenerateMock<IContractScheduleRepository>();
            _contractRepository = MockRepository.GenerateMock<IContractRepository>();
            _target = new ChangePersonEmploymentCommandHandler(_personSkillPeriodAssembler,_currentUnitOfWorkFactory,_skillRepository,_externalLogOnRepository,_personRepository,_teamRepository,_partTimePercentageRepository,_contractScheduleRepository,_contractRepository);

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
                DateFrom = new DateOnlyDto{DateTime = DateTime.Today},
				DateTo = new DateOnlyDto { DateTime = DateTime.Today },
                Id = Guid.NewGuid(),
                PersonId = _person.Id.GetValueOrDefault()
            };

            _personSkillPeriodCollection = new List<PersonSkillPeriodDto> { _personSkillPeriodDto };

            _team = TeamFactory.CreateSimpleTeam("Test Team");
            _team.SetId(Guid.NewGuid());
            _team.Site = SiteFactory.CreateSimpleSite("test site");
            _teamDto = new TeamDto { Id = Guid.NewGuid(), Description = _team.Description.Name, SiteAndTeam = _team.SiteAndTeam};

            _skill = SkillFactory.CreateSkill("TestSkill");
            _skill.SetId(Guid.NewGuid());

            _changePersonEmploymentCommandDto = new ChangePersonEmploymentCommandDto
            {
                ExternalLogOn = _externalLogOnList,
                Period = _dateOnlyPeriodDto,
                Person = _personDto,
                PersonContract = _personContractDto,
#pragma warning disable 618
                PersonSkillPeriodCollection = _personSkillPeriodCollection,
#pragma warning restore 618
                PersonSkillCollection = new List<PersonSkillDto>(),
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

            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);
            _partTimePercentageRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                .Return(_partTimePercentage);
            _contractRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault()))
                .Return(_contract);
            _contractScheduleRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                .Return(_contractSchedule);
            _teamRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault()))
                .Return(_team);
            _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			unitOfWork.AssertWasCalled(x => x.PersistAll());
        }

        [Test]
        public void ShouldThrowExceptionWhenTryingToSetSkillsUsingTheWrongCollection()
        {
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
            _person.AddPersonPeriod(personPeriod);

            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);
            _partTimePercentageRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                .Return(_partTimePercentage);
            _contractRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault()))
                .Return(_contract);
            _contractScheduleRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                .Return(_contractSchedule);
            _teamRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault()))
                .Return(_team);
            _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());
            _skillRepository.Stub(x => x.Load(_skill.Id.GetValueOrDefault())).Return(_skill);
#pragma warning disable 618
            _changePersonEmploymentCommandDto.PersonSkillPeriodCollection[0].PersonSkillCollection.Add(new PersonSkillDto { Active = false, Proficiency = 0.9, SkillId = _skill.Id.GetValueOrDefault() });
#pragma warning restore 618
            
            Assert.Throws<FaultException>(() => _target.Handle(_changePersonEmploymentCommandDto));
        }

	    [Test]
	    public void ShouldThrowExceptionWhenAddingPersonPeriodAfterLeavingDate()
	    {
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
				.Return(_person);

			_person.TerminatePerson(new DateOnly(2013, 10, 1), new PersonAccountUpdaterDummy());
			_dateOnlyPeriodDto.StartDate = new DateOnlyDto(2013, 10, 2);

			Assert.Throws<FaultException>(() => _target.Handle(_changePersonEmploymentCommandDto));
	    }

	    [Test]
        public void ShouldUseDetailedPersonSkillIfAvailable()
        {
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
            _person.AddPersonPeriod(personPeriod);

            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);
            _partTimePercentageRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                .Return(_partTimePercentage);
            _contractRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault()))
                .Return(_contract);
            _contractScheduleRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                .Return(_contractSchedule);
            _teamRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault()))
                .Return(_team);
            _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());
            _skillRepository.Stub(x => x.Load(_skill.Id.GetValueOrDefault())).Return(_skill);
#pragma warning disable 618
            _changePersonEmploymentCommandDto.PersonSkillPeriodCollection[0].SkillCollection.Add(_skill.Id.GetValueOrDefault());
#pragma warning restore 618
            _changePersonEmploymentCommandDto.PersonSkillCollection.Add(new PersonSkillDto{Active = false,Proficiency = 0.9,SkillId = _skill.Id.GetValueOrDefault()});

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			var personSkill = _person.PersonPeriodCollection.First().PersonSkillCollection.First();
            Assert.AreEqual(_skill, personSkill.Skill);
            Assert.AreEqual(.9, personSkill.SkillPercentage.Value);
            Assert.AreEqual(false, personSkill.Active);
        }

        [Test]
        public void ShouldHandlePersonSkillPeriodCollectionIsNotSet()
        {
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
            _person.AddPersonPeriod(personPeriod);

            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);
            _partTimePercentageRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                .Return(_partTimePercentage);
            _contractRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault()))
                .Return(_contract);
            _contractScheduleRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                .Return(_contractSchedule);
            _teamRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault()))
                .Return(_team);
            _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());
            _skillRepository.Stub(x => x.Load(_skill.Id.GetValueOrDefault())).Return(_skill);
#pragma warning disable 618
            _changePersonEmploymentCommandDto.PersonSkillPeriodCollection = null;
#pragma warning restore 618
            _changePersonEmploymentCommandDto.PersonSkillCollection.Add(new PersonSkillDto { Active = false, Proficiency = 0.9, SkillId = _skill.Id.GetValueOrDefault() });

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			var personSkill = _person.PersonPeriodCollection.First().PersonSkillCollection.First();
            Assert.AreEqual(_skill, personSkill.Skill);
            Assert.AreEqual(.9, personSkill.SkillPercentage.Value);
            Assert.AreEqual(false, personSkill.Active);
        }

        [Test]
        public void ShouldHandlePersonSkillCollectionIsNotSet()
        {
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
            _person.AddPersonPeriod(personPeriod);

            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);
            _partTimePercentageRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                .Return(_partTimePercentage);
            _contractRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault()))
                .Return(_contract);
            _contractScheduleRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                .Return(_contractSchedule);
            _teamRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault()))
                .Return(_team);
            _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());
            _skillRepository.Stub(x => x.Load(_skill.Id.GetValueOrDefault())).Return(_skill);
#pragma warning disable 618
            _changePersonEmploymentCommandDto.PersonSkillPeriodCollection[0].SkillCollection.Add(_skill.Id.GetValueOrDefault());
#pragma warning restore 618
            _changePersonEmploymentCommandDto.PersonSkillCollection = null;

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			var personSkill = _person.PersonPeriodCollection.First().PersonSkillCollection.First();
            Assert.AreEqual(_skill, personSkill.Skill);
            Assert.AreEqual(1, personSkill.SkillPercentage.Value);
            Assert.AreEqual(true, personSkill.Active);
        }

        [Test]
        public void ShouldHaveSkill()
        {
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today);
            _person.AddPersonPeriod(personPeriod);


            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);
            _partTimePercentageRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                .Return(_partTimePercentage);
            _contractRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault()))
                .Return(_contract);
            _contractScheduleRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                .Return(_contractSchedule);
            _teamRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault()))
                .Return(_team);
            _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());

            _skillRepository.Stub(x => x.Load(_skill.Id.GetValueOrDefault())).Return(_skill);
#pragma warning disable 618
            _changePersonEmploymentCommandDto.PersonSkillPeriodCollection[0].SkillCollection.Add( _skill.Id.GetValueOrDefault());
#pragma warning restore 618

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			var personSkill = _person.PersonPeriodCollection.First().PersonSkillCollection.First();
            Assert.AreEqual(_skill, personSkill.Skill);
            Assert.AreEqual(1, personSkill.SkillPercentage.Value);
            Assert.AreEqual(true, personSkill.Active);

        }
        [Test]
        public void ShouldAddNewPersonPeriodIfExistingPersonPeriodNotExists()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            _person.AddExternalLogOn(new ExternalLogOn(1, 1, "test Id", "test acd", true), personPeriod);
            _person.AddPersonPeriod(personPeriod);

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);
            _teamRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault()))
                .Return(_team);
            _partTimePercentageRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault()))
                .Return(_partTimePercentage);
            _contractRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault()))
                .Return(_contract);
            _contractScheduleRepository.Stub(
                x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault()))
                .Return(_contractSchedule);
            _personSkillPeriodAssembler.Stub(x => x.DomainEntityToDto(personPeriod))
                .IgnoreArguments()
                .Return(new PersonSkillPeriodDto());
            _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			unitOfWork.AssertWasCalled(x => x.PersistAll());
        }

        [Test]
        public void ShouldResetExternalLogOnsAndPersonSkillsIfPersonPeriodIsNew()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
				_person.AddExternalLogOn(new ExternalLogOn(1, 1, "test Id", "test acd", true), personPeriod);
            
                _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
                _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).Return(_person);
                _teamRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault())).Return(_team);
                _partTimePercentageRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).Return(_partTimePercentage);
                _contractRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(_contract);
                _contractScheduleRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(_contractSchedule);
                _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			unitOfWork.AssertWasCalled(x => x.PersistAll());
        }

		[Test]
		public void ShouldCreateNewPeriodWithShiftBagAndBudgetGroupFromPrevious()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
			personPeriod.RuleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			personPeriod.BudgetGroup = MockRepository.GenerateMock<IBudgetGroup>();
			_person.AddPersonPeriod(personPeriod);
			
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).Return(_person);
			_teamRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault())).Return(_team);
			_partTimePercentageRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).Return(_partTimePercentage);
			_contractRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(_contract);
			_contractScheduleRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(_contractSchedule);
			_externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());
			_personSkillPeriodAssembler.Stub(x => x.DomainEntityToDto(personPeriod)).Return(new PersonSkillPeriodDto());
			
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			var newPeriod = _person.Period(DateOnly.Today);
			newPeriod.BudgetGroup.Should().Be.EqualTo(personPeriod.BudgetGroup);
			newPeriod.RuleSetBag.Should().Be.EqualTo(personPeriod.RuleSetBag);
			unitOfWork.AssertWasCalled(x => x.PersistAll());
		}

        [Test]
        public void ShouldResetPersonSkillsIfPersonSkillsCollectionIsSetAndPeriodIsNew()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            _person.AddExternalLogOn(new ExternalLogOn(1, 1, "test Id", "test acd", true), personPeriod);

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault())).Return(_person);
            _teamRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault())).Return(_team);
            _partTimePercentageRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.PartTimePercentageId.GetValueOrDefault())).Return(_partTimePercentage);
            _contractRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractId.GetValueOrDefault())).Return(_contract);
            _contractScheduleRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.PersonContract.ContractScheduleId.GetValueOrDefault())).Return(_contractSchedule);
            _externalLogOnRepository.Stub(x => x.LoadAll()).Return(new List<IExternalLogOn>());
            _skillRepository.Stub(x => x.Load(_skill.Id.GetValueOrDefault())).Return(_skill);

#pragma warning disable 618
            _changePersonEmploymentCommandDto.PersonSkillPeriodCollection = null;
#pragma warning restore 618
            _changePersonEmploymentCommandDto.PersonSkillCollection.Add(new PersonSkillDto{Active = false,Proficiency = .9,SkillId = _skill.Id.GetValueOrDefault()});

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_changePersonEmploymentCommandDto);
			}

			_person.PersonPeriodCollection[0].PersonSkillCollection.Count().Should().Be.EqualTo(1);
            unitOfWork.AssertWasCalled(x => x.PersistAll());
        }

        [Test]
        public void ShouldThrowExceptionIfPersonContractIsNull()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            _person.AddExternalLogOn(new ExternalLogOn(1, 1, "test Id", "test acd", true), personPeriod);
            _changePersonEmploymentCommandDto.PersonContract = null;

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);

            Assert.Throws<FaultException>(() => _target.Handle(_changePersonEmploymentCommandDto));
        }

        [Test]
        public void ShouldThrowExceptionIfTeamIsNull()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            _person.AddExternalLogOn(new ExternalLogOn(1, 1, "test Id", "test acd", true), personPeriod);
            _changePersonEmploymentCommandDto.Team = null;

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);

            Assert.Throws<FaultException>(() => _target.Handle(_changePersonEmploymentCommandDto));
        }

        [Test]
		public void ShouldThrowExceptionIfTeamIsFromWrongBusinessUnit()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today.AddDays(-7));
            _person.AddExternalLogOn(new ExternalLogOn(1, 1, "test Id", "test acd", true), personPeriod);
            _team.Site.SetBusinessUnit(new BusinessUnit("asdf"));
            _team.Site.BusinessUnit.SetId(Guid.NewGuid());

            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
            _personRepository.Stub(x => x.Get(_changePersonEmploymentCommandDto.Person.Id.GetValueOrDefault()))
                .Return(_person);
            _teamRepository.Stub(x => x.Load(_changePersonEmploymentCommandDto.Team.Id.GetValueOrDefault()))
                .Return(_team);

            Assert.Throws<FaultException>(() => _target.Handle(_changePersonEmploymentCommandDto));
        }
    }
}
