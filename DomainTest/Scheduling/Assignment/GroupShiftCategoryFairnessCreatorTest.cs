using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class GroupShiftCategoryFairnessCreatorTest
    {
        private IGroupShiftCategoryFairnessCreator _target;
        private MockRepository _mockRepository;
        private IGroupPagePerDate _groupPagePerDate;
        private IScheduleDictionary _scheduleDictionary;
        private IPerson _person;
        private IPerson _anotherPerson;
        private DateOnly _dateOnly;
        private IGroupPage _groupPage;
        private IRootPersonGroup _rootPersonGroup;
        private IChildPersonGroup _childPersonGroup;
        private IScheduleRange _rangePerson;
        private IScheduleRange _rangeAnotherPerson;
        private IShiftCategory _personShiftCategory;
        private IShiftCategory _anotherPersonShiftCategory;
        private IShiftCategoryFairnessHolder _shiftCategoryFairnessHolder;
        private IShiftCategoryFairnessHolder _anotherShiftCategoryFairnessHolder;
        private ISchedulingResultStateHolder _stateHolder;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;


        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _groupPagePerDate = _mockRepository.StrictMock<IGroupPagePerDate>();
            _stateHolder = _mockRepository.StrictMock<ISchedulingResultStateHolder>();
            _scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
            _groupPage = _mockRepository.StrictMock<IGroupPage>();
            _person = _mockRepository.StrictMock<IPerson>();
            _anotherPerson = _mockRepository.StrictMock<IPerson>();
            _virtualSchedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
            _rangePerson = _mockRepository.StrictMock<IScheduleRange>();
            _rangeAnotherPerson = _mockRepository.StrictMock<IScheduleRange>();
            _rootPersonGroup = new RootPersonGroup();
            _childPersonGroup = new ChildPersonGroup();
            _childPersonGroup.AddPerson(_person);
            _childPersonGroup.AddPerson(_anotherPerson);
            _rootPersonGroup.AddChildGroup(_childPersonGroup);
            _personShiftCategory = ShiftCategoryFactory.CreateShiftCategory("ShiftCategory");
            _anotherPersonShiftCategory = ShiftCategoryFactory.CreateShiftCategory("AnotherShiftCategory");
            _dateOnly = new DateOnly();

            IDictionary<IShiftCategory, int> shiftDictionary = new Dictionary<IShiftCategory, int>
                                                                   {{_personShiftCategory, 1}};
            IDictionary<IShiftCategory, int> anotherShiftDictionary = new Dictionary<IShiftCategory, int>
                                                                          {{_anotherPersonShiftCategory, 1}};
            _shiftCategoryFairnessHolder = new ShiftCategoryFairnessHolder(shiftDictionary, new FairnessValueResult());
            _anotherShiftCategoryFairnessHolder = new ShiftCategoryFairnessHolder(anotherShiftDictionary, new FairnessValueResult());

            _target = new GroupShiftCategoryFairnessCreator(()=>new GroupPagePerDateHolder { ShiftCategoryFairnessGroupPagePerDate = _groupPagePerDate }, ()=>_stateHolder);
        }

        [Test]
        public void VerifyCalculateGroupShiftCategoryFairness()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_groupPagePerDate.GetGroupPageByDate(_dateOnly)).Return(_groupPage);
                Expect.Call(_groupPage.RootGroupCollection).Return(
                    new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { _rootPersonGroup }));
            	Expect.Call(_person.TerminalDate).Return(null);
				Expect.Call(_anotherPerson.TerminalDate).Return(null);
                Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod);
                Expect.Call(_anotherPerson.VirtualSchedulePeriod(_dateOnly)).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.IsValid).Return(true).Repeat.Twice();
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary).Repeat.Twice();
                Expect.Call(_scheduleDictionary[_person]).Return(_rangePerson);
                Expect.Call(_scheduleDictionary[_anotherPerson]).Return(_rangeAnotherPerson);

                Expect.Call(_rangePerson.CachedShiftCategoryFairness()).Return(_shiftCategoryFairnessHolder);
                Expect.Call(_rangeAnotherPerson.CachedShiftCategoryFairness()).Return(_anotherShiftCategoryFairnessHolder);

            }
            using (_mockRepository.Playback())
            {
                IShiftCategoryFairnessHolder result =
                    _target.CalculateGroupShiftCategoryFairness(_person, _dateOnly);
                Assert.IsTrue(result.ShiftCategoryFairnessDictionary.ContainsKey(_personShiftCategory));
                Assert.IsTrue(result.ShiftCategoryFairnessDictionary.ContainsKey(_anotherPersonShiftCategory));
            }
        }

        [Test]
        public void ShouldNotFetchFairnessForTerminatedPerson()
        {
           using (_mockRepository.Record())
            {
                Expect.Call(_groupPagePerDate.GetGroupPageByDate(new DateOnly(2011, 5, 1))).Return(_groupPage);
                Expect.Call(_groupPage.RootGroupCollection).Return(
                    new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup> { _rootPersonGroup }));
				Expect.Call(_person.TerminalDate).Return(null);
				Expect.Call(_anotherPerson.TerminalDate).Return(new DateOnly(2000,1,1)).Repeat.Twice();
                Expect.Call(_person.VirtualSchedulePeriod(new DateOnly(2011, 5, 1))).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.IsValid).Return(true);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[_person]).Return(_rangePerson);            
                Expect.Call(_rangePerson.CachedShiftCategoryFairness()).Return(_shiftCategoryFairnessHolder);
            }
            using (_mockRepository.Playback())
            {
                IShiftCategoryFairnessHolder result =
                    _target.CalculateGroupShiftCategoryFairness(_person, new DateOnly(2011, 5, 1));
                Assert.IsTrue(result.ShiftCategoryFairnessDictionary.ContainsKey(_personShiftCategory));
                Assert.IsFalse(result.ShiftCategoryFairnessDictionary.ContainsKey(_anotherPersonShiftCategory));
            }
        }
    }
}
