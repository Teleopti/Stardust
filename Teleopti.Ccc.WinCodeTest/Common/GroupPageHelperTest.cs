using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class GroupPageHelperTest: IDisposable
    {
        private GroupPageHelper _target;
        private MockRepository _mocks;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _target = new GroupPageHelper(_repositoryFactory, _unitOfWorkFactory);
        }

        [Test]
        public void ShouldSetSelectedPeriod()
        {
            var date = new DateOnlyPeriod(2010, 10, 10, 2010, 11, 11);
            _target.SetSelectedPeriod(date);
            Assert.AreEqual(_target.SelectedPeriod, date);
        }


        [Test]
        public void ShouldGetCollectionsAfterLoadAll()
        {
            IUnitOfWork unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            IDisposable disposable = _mocks.StrictMock<IDisposable>();

            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(unitOfWork.DisableFilter(QueryFilter.Deleted)).Return(disposable);
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(disposable.Dispose);
                ExpectLoadContract(unitOfWork);
                ExpectLoadPartTimePercentage(unitOfWork);
                ExpectLoadContractSchedule(unitOfWork);
                ExpectLoadRuleSetBag(unitOfWork);
                ExpectLoadPerson(unitOfWork);
                //ExpectLoadGroupPage(unitOfWork);
                ExpectLoadSkill(unitOfWork);
            
            }
            using (_mocks.Playback())
            {
                _target.LoadAll();
                Assert.AreEqual(1,_target.ContractCollection.Count());
                Assert.AreEqual(1, _target.ContractScheduleCollection.Count());
                Assert.AreEqual(1, _target.PartTimePercentageCollection.Count());
                Assert.AreEqual(1, _target.PersonCollection.Count());
                Assert.AreEqual(1, _target.RuleSetBagCollection.Count());
                Assert.AreEqual(0, _target.UserDefinedGroupings(null).Count());
                Assert.AreEqual(1, _target.SkillCollection.Count());
            }
        }

        //private void ExpectLoadGroupPage(IUnitOfWork unitOfWork)
        //{
        //    IGroupPageRepository groupPageRepository = _mocks.StrictMock<IGroupPageRepository>();
        //    Expect.Call(_repositoryFactory.CreateGroupPageRepository(unitOfWork)).Return(groupPageRepository);
        //    Expect.Call(groupPageRepository.LoadAllGroupPageWhenPersonCollectionReAssociated()).Return(new List<IGroupPage> { null });
        //}

        private void ExpectLoadPerson(IUnitOfWork unitOfWork)
        {
            IPerson agent = PersonFactory.CreatePerson();
            IPersonRepository personRepository = _mocks.StrictMock<IPersonRepository>();
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(
                personRepository);
            Expect.Call(personRepository.LoadAllPeopleWithHierarchyDataSortByName(_target.SelectedPeriod.StartDate)).Return(new List<IPerson> {agent});
        }

        private void ExpectLoadRuleSetBag(IUnitOfWork unitOfWork)
        {
            IRuleSetBagRepository ruleSetBagRepository = _mocks.StrictMock<IRuleSetBagRepository>();
            Expect.Call(_repositoryFactory.CreateRuleSetBagRepository(unitOfWork)).Return(
                ruleSetBagRepository);
            Expect.Call(ruleSetBagRepository.LoadAll()).Return(new List<IRuleSetBag> {null});
        }

        private void ExpectLoadContractSchedule(IUnitOfWork unitOfWork)
        {
            IContractScheduleRepository contractScheduleRepository = _mocks.StrictMock<IContractScheduleRepository>();
            Expect.Call(_repositoryFactory.CreateContractScheduleRepository(unitOfWork)).Return(
                contractScheduleRepository);
            Expect.Call(contractScheduleRepository.FindAllContractScheduleByDescription()).Return(
                new List<IContractSchedule> {null});
        }

        private void ExpectLoadSkill(IUnitOfWork unitOfWork)
        {
            ISkillRepository repository = _mocks.StrictMock<ISkillRepository>();
            Expect.Call(_repositoryFactory.CreateSkillRepository(unitOfWork)).Return(
                repository);
            Expect.Call(repository.LoadAll()).Return(
                new List<ISkill> { null });
        }

        private void ExpectLoadPartTimePercentage(IUnitOfWork unitOfWork)
        {
            IPartTimePercentageRepository partTimePercentageRepository = _mocks.StrictMock<IPartTimePercentageRepository>();
            Expect.Call(_repositoryFactory.CreatePartTimePercentageRepository(unitOfWork)).Return(
                partTimePercentageRepository);
            Expect.Call(partTimePercentageRepository.FindAllPartTimePercentageByDescription()).Return(
                new List<IPartTimePercentage> {null});
        }

        private void ExpectLoadContract(IUnitOfWork unitOfWork)
        {
            IContractRepository contractRepository = _mocks.StrictMock<IContractRepository>();
            Expect.Call(_repositoryFactory.CreateContractRepository(unitOfWork)).Return(contractRepository);
            Expect.Call(contractRepository.FindAllContractByDescription()).Return(new List<IContract>{null});
        }


        [Test]
        public void ShouldReturnCurrentBusinessUnit()
        {
            Assert.IsNotNull(_target.BusinessUnit);
        }

        [Test]
        public void ShouldUpdateCurrentGroupPage()
        {
            IUnitOfWork unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            IGroupPage groupPageMerged = _mocks.StrictMock<IGroupPage>();
            IGroupPage groupPageToUpdate = _mocks.StrictMock<IGroupPage>();

            using (_mocks.Record())
            {
                Expect.Call(()=>unitOfWork.Reassociate<IPerson>()).IgnoreArguments();
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(unitOfWork.Merge(groupPageToUpdate)).Return(groupPageMerged);
                Expect.Call(unitOfWork.PersistAll()).Return(null);
            }
            using (_mocks.Playback())
            {
                _target.CurrentGroupPage = groupPageToUpdate;
                _target.UpdateCurrent();
                Assert.AreEqual(groupPageMerged,_target.CurrentGroupPage);
            }
        }

        [Test]
        public void ShouldNotAddOrUpdateGroupPageGivenNullAsGroupPage()
        {
            _target.AddOrUpdateGroupPage(null);
        }

        [Test]
        public void ShouldAddOrUpdateGroupPage()
        {
            IUnitOfWork unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            IGroupPageRepository groupPageRepository = _mocks.StrictMock<IGroupPageRepository>();
            IGroupPage groupPage = _mocks.StrictMock<IGroupPage>();

            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(_repositoryFactory.CreateGroupPageRepository(unitOfWork)).Return(groupPageRepository);
                Expect.Call(() => groupPageRepository.Add(groupPage));
                Expect.Call(unitOfWork.PersistAll()).Return(null);

            }
            using (_mocks.Playback())
            {
                _target.AddOrUpdateGroupPage(groupPage);
            }
        }

        [Test]
        public void ShouldRemoveGroupPage()
        {
            IUnitOfWork unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            IGroupPageRepository groupPageRepository = _mocks.StrictMock<IGroupPageRepository>();
            IGroupPage groupPage = _mocks.StrictMock<IGroupPage>();

            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(_repositoryFactory.CreateGroupPageRepository(unitOfWork)).Return(groupPageRepository);
                Expect.Call(() => groupPageRepository.Remove(groupPage));
                Expect.Call(unitOfWork.PersistAll()).Return(null);
            }
            using (_mocks.Playback())
            {
                _target.RemoveGroupPage(groupPage);
            }
        }

        [Test]
        public void ShouldRequireLoadAllBeforeCollectionsAreUsed()
        {
	        Assert.Throws<InvalidOperationException>(() =>
	        {
				var collection = _target.ContractCollection;
				Assert.Fail($"Should have thrown exception. Number of items in list = {collection.Count()}");
			});
        }

        [Test]
        public void ShouldLoadRenameAndSaveOnRename()
        {
            var guid = Guid.NewGuid();
            var groupPage = new GroupPage("oldName");
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var groupPageRep = _mocks.StrictMock<IGroupPageRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateGroupPageRepository(unitOfWork)).Return(groupPageRep);
            Expect.Call(groupPageRep.Load(guid)).Return(groupPage);
            Expect.Call(unitOfWork.PersistAll());
            Expect.Call(unitOfWork.Dispose);
            _mocks.ReplayAll();
            _target.RenameGroupPage(guid,"newName");
            Assert.That(groupPage.Description.Name,Is.EqualTo("newName"));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadAndSetCurrentById()
        {
            var guid = Guid.NewGuid();
            var groupPage = new GroupPage("GP");
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var groupPageRep = _mocks.StrictMock<IGroupPageRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateGroupPageRepository(unitOfWork)).Return(groupPageRep);
            Expect.Call(groupPageRep.Load(guid)).Return(groupPage);
            Expect.Call(unitOfWork.Dispose);
            _mocks.ReplayAll();
            _target.SetCurrentGroupPageById(guid);
            Assert.That(_target.CurrentGroupPage, Is.EqualTo(groupPage));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveGroupPageFromRepositoryById()
        {
            var guid = Guid.NewGuid();
            var groupPage = new GroupPage("GP");
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var groupPageRep = _mocks.StrictMock<IGroupPageRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateGroupPageRepository(unitOfWork)).Return(groupPageRep);
            Expect.Call(groupPageRep.Load(guid)).Return(groupPage);
            Expect.Call(() => groupPageRep.Remove(groupPage));

            Expect.Call(unitOfWork.PersistAll());
            Expect.Call(unitOfWork.Dispose);
            _mocks.ReplayAll();
            _target.RemoveGroupPageById(guid);         
            _mocks.VerifyAll();
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _target.Dispose();
                _repositoryFactory = null;
                _unitOfWorkFactory = null;
            }
        }
    }
}

    
