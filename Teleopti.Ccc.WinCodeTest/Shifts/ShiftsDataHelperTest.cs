using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Events;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class ShiftsDataHelperTest
    {
        private MockRepository _mock;
        private ShiftsDataHelper _target;
        private IUnitOfWork _uow;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
        private IEventAggregator _eventAggregator;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _repositoryFactory = _mock.StrictMock<IRepositoryFactory>();
            _uow = _mock.StrictMock<IUnitOfWork>();
            _eventAggregator = new EventAggregator();
            _target = new ShiftsDataHelper(_unitOfWorkFactory, _repositoryFactory, _eventAggregator);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }


        [Test]
        public void VerifyCreateDefaultRuleSet()
        {
            IWorkShiftRuleSet ruleSet = _target.CreateDefaultRuleSet(ActivityFactory.CreateActivity("Phone"),
                                                                     ShiftCategoryFactory.CreateShiftCategory(
                                                                         "MyCategory"),
                                                                     new TimePeriod(8, 0, 9, 0),
                                                                     TimeSpan.FromMinutes(15),
                                                                     new TimePeriod(17, 0, 18, 0),
                                                                     TimeSpan.FromMinutes(15));

            Assert.IsNotNull(ruleSet);
        }

        [Test]
        public void VerifyCreateDefaultRuleSetBag()
        {
            Assert.IsNotNull(_target.CreateDefaultRuleSetBag());
        }

        [Test]
        public void VerifyCreateDefaultActivityTimeLimiter()
        {
            Assert.IsNotNull(_target.CreateDefaultActivityTimeLimiter(_target.CreateDefaultRuleSet(ActivityFactory.CreateActivity("Phone"),
                ShiftCategoryFactory.CreateShiftCategory("MyCategory"),
                new TimePeriod(8, 0, 9, 0),
                TimeSpan.FromMinutes(15),
                new TimePeriod(17, 0, 18, 0),
                TimeSpan.FromMinutes(15)), TimeSpan.FromMinutes(60)));
        }

        [Test]
        public void VerifyFindAllOperatorLimits()
        {
            Assert.IsTrue(_target.FindAllOperatorLimits().Count == 5);
        }

        [Test]
        public void VerifyFindAllAccessibilities()
        {
            Assert.IsTrue(_target.FindAllAccessibilities().Count == 2);
        }

        [Test]
        public void ShouldFindShiftCategories()
        {
            var repository = _mock.StrictMock<IShiftCategoryRepository>();
            var list = new List<IShiftCategory> { ShiftCategoryFactory.CreateShiftCategory("Test2"), ShiftCategoryFactory.CreateShiftCategory("Test1") };
            using (_mock.Record())
            {
                Expect.Call(_repositoryFactory.CreateShiftCategoryRepository(_uow)).Return(repository);
                Expect.Call(repository.FindAll()).Return(list);
            }
            using (_mock.Playback())
            {
                var result = _target.FindAllCategories(_uow);
                Assert.AreEqual(list.Last(),result.First());
            }
        }

        [Test]
        public void ShouldFindRuleSetBags()
        {
            var repository = _mock.StrictMock<IRuleSetBagRepository>();
            var list = new List<IRuleSetBag> { new RuleSetBag { Description = new Description("Test2") }, new RuleSetBag { Description = new Description("Test1") } };
            using (_mock.Record())
            {
                Expect.Call(_repositoryFactory.CreateRuleSetBagRepository(_uow)).Return(repository);
                Expect.Call(repository.LoadAllWithRuleSets()).Return(list);
            }
            using (_mock.Playback())
            {
                var result = _target.FindRuleSetBags(_uow);
                Assert.AreEqual(list.Last(), result.First());
            }
        }

        [Test]
        public void ShouldFindRuleSets()
        {
            var repository = _mock.StrictMock<IWorkShiftRuleSetRepository>();
            var list = new List<IWorkShiftRuleSet> { WorkShiftRuleSetFactory.Create(), WorkShiftRuleSetFactory.Create() };
            list[0].Description = new Description("Test2");
            list[1].Description = new Description("Test1");
            using (_mock.Record())
            {
                Expect.Call(_repositoryFactory.CreateWorkShiftRuleSetRepository(_uow)).Return(repository);
                Expect.Call(repository.FindAllWithLimitersAndExtenders()).Return(list);
            }
            using (_mock.Playback())
            {
                var result = _target.FindRuleSets(_uow);
                Assert.AreEqual(list.Last(), result.First());
            }
        }
        
        [Test]
        public void ShouldSaveBothRuleSetAndBag()
        {
            var ruleSetRepository = _mock.StrictMock<IWorkShiftRuleSetRepository>();
            var bagRepository = _mock.StrictMock<IRuleSetBagRepository>();
            var ruleSet = _mock.DynamicMock<IWorkShiftRuleSet>();
            var bag = _mock.DynamicMock<IRuleSetBag>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
                Expect.Call(_repositoryFactory.CreateWorkShiftRuleSetRepository(_uow)).Return(ruleSetRepository);
                Expect.Call(_repositoryFactory.CreateRuleSetBagRepository(_uow)).Return(bagRepository);
                
                Expect.Call(()=>bagRepository.Add(bag));
                Expect.Call(()=>ruleSetRepository.Add(ruleSet));
                Expect.Call(_uow.PersistAll()).Return(null);
                Expect.Call(_uow.Dispose);
            }
            using (_mock.Playback())
            {
                _eventAggregator.GetEvent<RuleSetBagChanged>().Publish(bag);
                _eventAggregator.GetEvent<RuleSetChanged>().Publish(new List<IWorkShiftRuleSet> { ruleSet });
                
                _target.PersistAll();
            }
        }

        [Test]
        public void ShouldRemoveBothRuleSetAndBag()
        {
            var ruleSetRepository = _mock.StrictMock<IWorkShiftRuleSetRepository>();
            var bagRepository = _mock.StrictMock<IRuleSetBagRepository>();
            var ruleSet = _mock.DynamicMock<IWorkShiftRuleSet>();
            var bag = _mock.DynamicMock<IRuleSetBag>();

            using (_mock.Record())
            {
                Expect.Call(bag.RuleSetCollection).Return(
                    new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet>()));
                Expect.Call(ruleSet.RuleSetBagCollection).Return(
                    new ReadOnlyCollection<IRuleSetBag>(new List<IRuleSetBag>()));
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
                Expect.Call(_repositoryFactory.CreateWorkShiftRuleSetRepository(_uow)).Return(ruleSetRepository);
                Expect.Call(_repositoryFactory.CreateRuleSetBagRepository(_uow)).Return(bagRepository);

            	Expect.Call(() => bagRepository.Add(bag));
                Expect.Call(() => bagRepository.Remove(bag));
                Expect.Call(() => ruleSetRepository.Remove(ruleSet));
                Expect.Call(_uow.PersistAll()).Return(null);
                Expect.Call(_uow.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Delete(bag);
                _target.Delete(ruleSet);
                _target.PersistAll();
            }
        }
        [Test]
        public void ShouldRemoveRuleSetFromAllBagsOnDelete()
        {
            var bag1 = _mock.StrictMock<IRuleSetBag>();
            var bag2 = _mock.StrictMock<IRuleSetBag>();
            var generator = _mock.StrictMock<IWorkShiftTemplateGenerator>();
            var ruleset = new WorkShiftRuleSet(generator);
            ruleset.RuleSetBagCollectionWritable.Add(bag1);
            ruleset.RuleSetBagCollectionWritable.Add(bag2);
            Expect.Call(() => bag1.RemoveRuleSet(ruleset));
            Expect.Call(() => bag2.RemoveRuleSet(ruleset));
            _mock.ReplayAll();
            ruleset.RuleSetBagCollection.Count().Should().Be.EqualTo(2);
            _target.Delete(ruleset);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldRemoveAllRuleSetFromBagsOnDelete()
        {
            var bag1 = _mock.StrictMock<IRuleSetBag>();
            var generator = _mock.StrictMock<IWorkShiftTemplateGenerator>();
            var ruleset = new WorkShiftRuleSet(generator);
            var ruleset2 = new WorkShiftRuleSet(generator);
            var rulesets = new List<IWorkShiftRuleSet> {ruleset, ruleset2};
            ruleset.RuleSetBagCollectionWritable.Add(bag1);
            ruleset2.RuleSetBagCollectionWritable.Add(bag1);

            Expect.Call(bag1.RuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(rulesets));
            Expect.Call(() => bag1.RemoveRuleSet(ruleset));
            Expect.Call(() => bag1.RemoveRuleSet(ruleset2));
            _mock.ReplayAll();
            _target.Delete(bag1);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldFindActivities()
        {
            var repository = _mock.StrictMock<IActivityRepository>();
            var masterRepository = _mock.StrictMock<IMasterActivityRepository>();

            var list = new List<IActivity> { ActivityFactory.CreateActivity("Test1"), ActivityFactory.CreateActivity("Test2") };
            var masterlist = new List<IMasterActivity>
                                 {new MasterActivity {Description = new Description("Master of the universe")}};
            using (_mock.Record())
            {
                Expect.Call(_uow.DisableFilter(QueryFilter.Deleted));
                Expect.Call(_repositoryFactory.CreateActivityRepository(_uow)).Return(repository);
                Expect.Call(_repositoryFactory.CreateMasterActivityRepository(_uow)).Return(masterRepository);

                Expect.Call(repository.LoadAllSortByName()).Return(list);
                Expect.Call(masterRepository.LoadAll()).Return(masterlist);
            }
            using (_mock.Playback())
            {
                var result = _target.FindAllActivities(_uow);
                result.First().Should().Be.EqualTo(list.First());
                result.Count().Should().Be.EqualTo(3);
                result.Last().Should().Be.EqualTo(masterlist.First());
            }
        }

        [Test]
        public void ShouldAddToRuleSetsToSaveOnChangeEvent()
        {
            _target.HasUnsavedData().Should().Be(false);
            var ruleset = _mock.StrictMock<IWorkShiftRuleSet>();
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(new List<IWorkShiftRuleSet> { ruleset });
            _target.HasUnsavedData().Should().Be(true);
        }

        [Test]
        public void ShouldNotAddToRuleSetsToSaveOnChangeEventIfNull()
        {
            _target.HasUnsavedData().Should().Be(false);
            _eventAggregator.GetEvent<RuleSetChanged>().Publish(null);
            _target.HasUnsavedData().Should().Be(false);
        }

        [Test]
        public void ShouldAddToBagsToSaveOnChangeEvent()
        {
            _target.HasUnsavedData().Should().Be(false);
            var rulesetBag = _mock.StrictMock<IRuleSetBag>();
            _eventAggregator.GetEvent<RuleSetBagChanged>().Publish(rulesetBag);
            _target.HasUnsavedData().Should().Be(true);
        }

        [Test]
        public void ShouldNotAddToBagsToSaveOnChangeEventIfNull()
        {
            _target.HasUnsavedData().Should().Be(false);
            _eventAggregator.GetEvent<RuleSetBagChanged>().Publish(null);
            _target.HasUnsavedData().Should().Be(false);
        }

        [Test]
        public void ShouldReturnDefaultSegmentLength()
        {
            var globalRepository = _mock.StrictMock<ISettingDataRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
            Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(_uow)).Return(globalRepository);
            Expect.Call(globalRepository.FindValueByKey("DefaultSegment", new DefaultSegment())).Return(
                new DefaultSegment {SegmentLength = 15}).IgnoreArguments();
            Expect.Call(_uow.Dispose);
            _mock.ReplayAll();
            _target.DefaultSegment().Should().Be.EqualTo(15);
            _mock.VerifyAll();
        }
    }
}
