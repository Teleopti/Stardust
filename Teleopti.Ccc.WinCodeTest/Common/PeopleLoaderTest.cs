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
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
#pragma warning disable 618

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class PeopleLoaderTest
    {
        private PeopleLoader _target;
        private readonly IList<IEntity> _selectedEntities = new List<IEntity>();
        private DateOnlyPeriod _requestedPeriod;
        private MockRepository _mockRep;
        private IPersonRepository _personRepository;
        private IContractRepository _contractRepository;
        private ISchedulerStateHolder _schedulerStateHolder;
        private ITeam _team;
        private IPerson _person;
        private readonly ICollection<IPerson> _persons = new List<IPerson>();
        private IContract _contract;
        private readonly IList<IContract> _contracts = new List<IContract>();
        private IUnitOfWork _uow;
        private ISelectedEntitiesForPeriod _selectedEntitiesForPeriod;
        private ISkillRepository _skillRepository;

        private class disposeStub :IDisposable
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
            public void Dispose()
            {
            }
        }

        [SetUp]
        public void Setup()
        {
            _team = TeamFactory.CreateTeam("My Team", "My Site");
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(new DateTime(2009, 1, 1)), new List<ISkill>());
            _persons.Add(_person);
            _person.PersonPeriodCollection[0].Team = _team;
            _selectedEntities.Add(_team);
            _mockRep = new MockRepository();
            _personRepository = _mockRep.StrictMock<IPersonRepository>();
            _contractRepository = _mockRep.StrictMock<IContractRepository>();
            _skillRepository = _mockRep.StrictMock<ISkillRepository>();
            _schedulerStateHolder = _mockRep.StrictMock<ISchedulerStateHolder>();
            _requestedPeriod = new DateOnlyPeriod(2009, 9, 6,2009, 9, 7);
            _contract = ContractFactory.CreateContract("Part time");
            _contracts.Add(_contract);
            _uow = _mockRep.StrictMock<IUnitOfWork>();

            _selectedEntitiesForPeriod = new SelectedEntitiesForPeriod(_selectedEntities,_requestedPeriod);

            _target = new PeopleLoader(_personRepository, _contractRepository, _schedulerStateHolder, _selectedEntitiesForPeriod, _skillRepository);
        }

        [Test]
        public void VerifyIsCreated()
        {
            Assert.IsNotNull(_target);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldInitializeWithIntradaySelectedEntities()
        {
            var intradayMainModel = new IntradayMainModel
            {
                EntityCollection = _selectedEntities,
                Period = _requestedPeriod
            };
            _selectedEntitiesForPeriod = new IntradaySelectedEntitiesForPeriod(intradayMainModel);

            _target = new PeopleLoader(_personRepository, _contractRepository, _schedulerStateHolder, _selectedEntitiesForPeriod, _skillRepository);

            Expect.Call(_personRepository.FindPeopleInOrganization(_requestedPeriod, true)).Return(new List<IPerson>());
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(new SchedulingResultStateHolder());
            Expect.Call(_schedulerStateHolder.ResetFilteredPersons);
            Expect.Call(_schedulerStateHolder.AllPermittedPersons).Return(new List<IPerson>());
            Expect.Call(_contractRepository.FindAllContractByDescription()).Return(_contracts);
            Expect.Call(_contractRepository.UnitOfWork).Return(_uow);
            Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(new disposeStub());
            Expect.Call(_skillRepository.LoadAll());
            _mockRep.ReplayAll();

            ISchedulerStateHolder stateHolder = _target.Initialize();
            Assert.AreEqual(0, stateHolder.AllPermittedPersons.Count);
            _mockRep.VerifyAll();
        }

        [Test]
        public void VerifyInitialize()
        {
            Expect.Call(_personRepository.FindPeopleInOrganization(_requestedPeriod, true)).Return(new List<IPerson>());
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(new SchedulingResultStateHolder());
            Expect.Call(_schedulerStateHolder.ResetFilteredPersons);
            Expect.Call(_schedulerStateHolder.AllPermittedPersons).Return(new List<IPerson>());
            Expect.Call(_contractRepository.FindAllContractByDescription()).Return(_contracts);
            Expect.Call(_contractRepository.UnitOfWork).Return(_uow);
            Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(new disposeStub());
            Expect.Call(_skillRepository.LoadAll());
            _mockRep.ReplayAll();

            ISchedulerStateHolder stateHolder = _target.Initialize();
            Assert.AreEqual(0, stateHolder.AllPermittedPersons.Count);
            _mockRep.VerifyAll();
        }
        [Test]
        public void VerifyInitializeAndTeam()
        {
            _selectedEntities.Clear();
            _selectedEntities.Add(_team);
            _target = new PeopleLoader(_personRepository, _contractRepository, _schedulerStateHolder, _selectedEntitiesForPeriod, _skillRepository);
            IList<IPerson> returnPersons = new List<IPerson>();
            Expect.Call(_personRepository.FindPeopleInOrganization(_requestedPeriod, true)).Return(_persons);
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(new SchedulingResultStateHolder());
            Expect.Call(_schedulerStateHolder.ResetFilteredPersons);
            Expect.Call(_schedulerStateHolder.AllPermittedPersons).Return(returnPersons).Repeat.AtLeastOnce();
            Expect.Call(_contractRepository.FindAllContractByDescription()).Return(_contracts);
            Expect.Call(_contractRepository.UnitOfWork).Return(_uow);
            Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(new disposeStub());
            Expect.Call(_skillRepository.LoadAll());
            _mockRep.ReplayAll();

            ISchedulerStateHolder stateHolder = _target.Initialize();
            Assert.AreEqual(1, stateHolder.AllPermittedPersons.Count);
            _mockRep.VerifyAll();
        }
        [Test]
        public void VerifyInitializeAndPerson()
        {
            _selectedEntities.Clear();
            _selectedEntities.Add(_person);
            _target = new PeopleLoader(_personRepository, _contractRepository, _schedulerStateHolder, _selectedEntitiesForPeriod, _skillRepository);
            IList<IPerson> returnPersons = new List<IPerson>();
            Expect.Call(_personRepository.FindPeopleInOrganization(_requestedPeriod, true)).Return(_persons);
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(new SchedulingResultStateHolder());
            Expect.Call(_schedulerStateHolder.ResetFilteredPersons);
            Expect.Call(_schedulerStateHolder.AllPermittedPersons).Return(returnPersons).Repeat.AtLeastOnce();
            Expect.Call(_contractRepository.FindAllContractByDescription()).Return(_contracts);
            Expect.Call(_contractRepository.UnitOfWork).Return(_uow);
            Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(new disposeStub());
            Expect.Call(_skillRepository.LoadAll());
            _mockRep.ReplayAll();

            ISchedulerStateHolder stateHolder = _target.Initialize();
            Assert.AreEqual(1, stateHolder.AllPermittedPersons.Count);
            _mockRep.VerifyAll();
        }

        [Test]
        public void VerifyPeopleInOrg()
        {
            Expect.Call(_personRepository.FindPeopleInOrganization(_requestedPeriod, true)).Return(new List<IPerson>());
            _mockRep.ReplayAll();

            Assert.IsNotNull(_target.PeopleInOrg());
            _mockRep.VerifyAll();
        }
    }
}
