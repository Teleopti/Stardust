using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class PeopleLoaderForTeamLeaderModeTest
    {
        private IPeopleLoader _target;
        private IList<IEntity> _selectedEntities;
        private DateOnlyPeriod _requestedPeriod;
        private MockRepository _mockRep;
        private IPersonRepository _personRepository;
        private IContractRepository _contractRepository;
        private ISchedulerStateHolder _schedulerStateHolder;
        private ITeam _team;
        private IPerson _person;
        private ICollection<IPerson> _persons;
        private IContract _contract;
        private IList<IContract> _contracts;
        private IUnitOfWork _uow;
        private ISelectedEntitiesForPeriod _selectedEntitiesForPeriod;
        private ISkillRepository _skillRepository;
    	private IRepositoryFactory _repositoryFactory;
    	private IPartTimePercentageRepository _partTimePercentageRepository;
    	private IRuleSetBagRepository _ruleSetBagRepository;
    	private IWorkShiftRuleSetRepository _ruleSetRepository;
        private IWorkflowControlSetRepository _workflowControlSetRepository;
    	private ISiteRepository _siteRepository;
    	private ITeamRepository _teamRepository;

    	private class disposeStub : IDisposable
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
            public void Dispose()
            {
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _contracts = new List<IContract>();
            _persons = new List<IPerson>();
            _selectedEntities = new List<IEntity>();
            _team = TeamFactory.CreateTeam("My Team", "My Site");
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(new DateTime(2009, 1, 1)), new List<ISkill>());
            _persons.Add(_person);
            _person.PersonPeriodCollection[0].Team = _team;
            _selectedEntities.Add(_team);
            _mockRep = new MockRepository();
            _personRepository = _mockRep.StrictMock<IPersonRepository>();
            _contractRepository = _mockRep.StrictMock<IContractRepository>();
        	_partTimePercentageRepository = _mockRep.StrictMock<IPartTimePercentageRepository>();
            _skillRepository = _mockRep.StrictMock<ISkillRepository>();
        	_ruleSetBagRepository = _mockRep.StrictMock<IRuleSetBagRepository>();
        	_ruleSetRepository = _mockRep.StrictMock<IWorkShiftRuleSetRepository>();
            _workflowControlSetRepository = _mockRep.StrictMock<IWorkflowControlSetRepository>();
			_siteRepository = _mockRep.StrictMock<ISiteRepository>();
			_teamRepository = _mockRep.StrictMock<ITeamRepository>();

            _schedulerStateHolder = _mockRep.StrictMock<ISchedulerStateHolder>();
            _requestedPeriod = new DateOnlyPeriod(2009, 9, 6, 2009, 9, 7);
            _contract = ContractFactory.CreateContract("Part time");
            _contracts.Add(_contract);
            _uow = _mockRep.StrictMock<IUnitOfWork>();
        	_repositoryFactory = _mockRep.StrictMock<IRepositoryFactory>();
            _selectedEntitiesForPeriod = new SelectedEntitiesForPeriod(_selectedEntities, _requestedPeriod);

			_target = new PeopleLoaderForTeamLeaderMode(_uow, _schedulerStateHolder, _selectedEntitiesForPeriod, _repositoryFactory);
        }

		private void expectsForRepositoryFactory()
		{
			Expect.Call(_repositoryFactory.CreateContractRepository(_uow)).Return(_contractRepository);
			Expect.Call(_repositoryFactory.CreateSkillRepository(_uow)).Return(_skillRepository);
			Expect.Call(_repositoryFactory.CreatePartTimePercentageRepository(_uow)).Return(_partTimePercentageRepository);
			Expect.Call(_repositoryFactory.CreatePersonRepository(_uow)).Return(_personRepository);
			Expect.Call(_repositoryFactory.CreateRuleSetBagRepository(_uow)).Return(_ruleSetBagRepository);
			Expect.Call(_repositoryFactory.CreateWorkShiftRuleSetRepository(_uow)).Return(_ruleSetRepository);
		    Expect.Call(_repositoryFactory.CreateWorkflowControlSetRepository(_uow)).Return(_workflowControlSetRepository);
		    Expect.Call(_repositoryFactory.CreateSiteRepository(_uow)).Return(_siteRepository);
		    Expect.Call(_repositoryFactory.CreateTeamRepository(_uow)).Return(_teamRepository);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyInitializeAndTeam()
        {
            _selectedEntities.Clear();
            _selectedEntities.Add(_team);
			_target = new PeopleLoaderForTeamLeaderMode(_uow, _schedulerStateHolder, _selectedEntitiesForPeriod, _repositoryFactory);
            IList<IPerson> returnPersons = new List<IPerson>();
        	expectsForRepositoryFactory();
			Expect.Call(_personRepository.FindPeopleBelongTeamWithSchedulePeriod(_team, _requestedPeriod)).Return(_persons);
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(new SchedulingResultStateHolder());
            Expect.Call(_schedulerStateHolder.ResetFilteredPersons);
            Expect.Call(_schedulerStateHolder.AllPermittedPersons).Return(returnPersons).Repeat.AtLeastOnce();
            Expect.Call(_contractRepository.FindAllContractByDescription()).Return(_contracts);
            Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(new disposeStub());
            Expect.Call(_skillRepository.LoadAll());
            Expect.Call(_partTimePercentageRepository.LoadAll());
			Expect.Call(_ruleSetBagRepository.LoadAllWithRuleSets());
			Expect.Call(_ruleSetRepository.FindAllWithLimitersAndExtenders());
        	Expect.Call(_siteRepository.LoadAll());
        	Expect.Call(_teamRepository.LoadAll());
            Expect.Call(_workflowControlSetRepository.LoadAll());

            _mockRep.ReplayAll();

            ISchedulerStateHolder stateHolder = _target.Initialize();
            Assert.AreEqual(1, stateHolder.AllPermittedPersons.Count);
            _mockRep.VerifyAll();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void VerifyInitializeAndPerson()
        {
            _selectedEntities.Clear();
            _selectedEntities.Add(_person);
			_target = new PeopleLoaderForTeamLeaderMode(_uow, _schedulerStateHolder, _selectedEntitiesForPeriod, _repositoryFactory);
            IList<IPerson> returnPersons = new List<IPerson>();
			expectsForRepositoryFactory();
            Expect.Call(_personRepository.FindPeople(_persons)).Return(_persons);
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(new SchedulingResultStateHolder());
            Expect.Call(_schedulerStateHolder.ResetFilteredPersons);
            Expect.Call(_schedulerStateHolder.AllPermittedPersons).Return(returnPersons).Repeat.AtLeastOnce();
            Expect.Call(_contractRepository.FindAllContractByDescription()).Return(_contracts);
            Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(new disposeStub());
            Expect.Call(_skillRepository.LoadAll());
			Expect.Call(_partTimePercentageRepository.LoadAll());
			Expect.Call(_ruleSetBagRepository.LoadAllWithRuleSets());
			Expect.Call(_ruleSetRepository.FindAllWithLimitersAndExtenders());
            Expect.Call(_workflowControlSetRepository.LoadAll());
			Expect.Call(_siteRepository.LoadAll());
			Expect.Call(_teamRepository.LoadAll());
            _mockRep.ReplayAll();

            ISchedulerStateHolder stateHolder = _target.Initialize();
            Assert.AreEqual(1, stateHolder.AllPermittedPersons.Count);
            _mockRep.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void PersonWithTerminalDateBeforeRequestedPeriodShouldNotBeLoaded()
        {
            _selectedEntities.Clear();
            _selectedEntities.Add(_team);
            _person.TerminatePerson(new DateOnly(2009, 9, 5), new MockRepository().StrictMock<IPersonAccountUpdater>());
            _target = new PeopleLoaderForTeamLeaderMode(_uow, _schedulerStateHolder, _selectedEntitiesForPeriod, _repositoryFactory);
            IList<IPerson> returnPersons = new List<IPerson>();
            expectsForRepositoryFactory();
            Expect.Call(_personRepository.FindPeopleBelongTeamWithSchedulePeriod(_team, _requestedPeriod)).Return(_persons);
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(new SchedulingResultStateHolder());
            Expect.Call(_schedulerStateHolder.ResetFilteredPersons);
            Expect.Call(_schedulerStateHolder.AllPermittedPersons).Return(returnPersons).Repeat.AtLeastOnce();
            Expect.Call(_contractRepository.FindAllContractByDescription()).Return(_contracts);
            Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(new disposeStub());
            Expect.Call(_skillRepository.LoadAll());
            Expect.Call(_partTimePercentageRepository.LoadAll());
            Expect.Call(_ruleSetBagRepository.LoadAllWithRuleSets());
            Expect.Call(_ruleSetRepository.FindAllWithLimitersAndExtenders());
            Expect.Call(_workflowControlSetRepository.LoadAll());
			Expect.Call(_siteRepository.LoadAll());
			Expect.Call(_teamRepository.LoadAll());
            _mockRep.ReplayAll();

            ISchedulerStateHolder stateHolder = _target.Initialize();
            Assert.AreEqual(0, stateHolder.AllPermittedPersons.Count);
            _mockRep.VerifyAll();
        }

		[Test]
		public void ShouldLoadPersonWithTerminalDateSameAsRequestedPeriodStart()
		{
			_selectedEntities.Clear();
			_selectedEntities.Add(_team);
			_person.TerminatePerson(new DateOnly(2009, 9, 6), new MockRepository().StrictMock<IPersonAccountUpdater>());
			_target = new PeopleLoaderForTeamLeaderMode(_uow, _schedulerStateHolder, _selectedEntitiesForPeriod, _repositoryFactory);
			IList<IPerson> returnPersons = new List<IPerson>();

			using (_mockRep.Record())
			{
				expectsForRepositoryFactory();
				Expect.Call(_personRepository.FindPeopleBelongTeamWithSchedulePeriod(_team, _requestedPeriod)).Return(_persons);
				Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(new SchedulingResultStateHolder());
				Expect.Call(_schedulerStateHolder.ResetFilteredPersons);
				Expect.Call(_schedulerStateHolder.AllPermittedPersons).Return(returnPersons).Repeat.AtLeastOnce();
				Expect.Call(_contractRepository.FindAllContractByDescription()).Return(_contracts);
				Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(new disposeStub());
				Expect.Call(_skillRepository.LoadAll());
				Expect.Call(_partTimePercentageRepository.LoadAll());
				Expect.Call(_ruleSetBagRepository.LoadAllWithRuleSets());
				Expect.Call(_ruleSetRepository.FindAllWithLimitersAndExtenders());
				Expect.Call(_workflowControlSetRepository.LoadAll());
				Expect.Call(_siteRepository.LoadAll());
				Expect.Call(_teamRepository.LoadAll());
			}

			using (_mockRep.Playback())
			{
				ISchedulerStateHolder stateHolder = _target.Initialize();
				Assert.AreEqual(1, stateHolder.AllPermittedPersons.Count);
			}
		}
    }
}