using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class PersonShiftCategoryPointCalculatorTest
    {
        private IPersonShiftCategoryPointCalculator _target;
        private IShiftCategoryPoints _shiftCategoryPoints;
        private MockRepository _mock;
        private IScheduleRange _scheduleRange;
        private IList<IShiftCategory> _shiftCategories;
        private IShiftCategory _shiftCategory1;
        private IShiftCategory _shiftCategory2;
        private IScheduleDay _scheduleDay;
        private IPersonAssignment _personAssignment;
        private IDictionary<IShiftCategory, int> _shiftCategoryPointDic;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _shiftCategoryPoints = _mock.StrictMock<IShiftCategoryPoints>();
            _target = new PersonShiftCategoryPointCalculator(_shiftCategoryPoints);
            _shiftCategory1 = _mock.StrictMock<IShiftCategory>();
            _shiftCategory2 = _mock.StrictMock<IShiftCategory>();
            _scheduleRange = _mock.StrictMock<IScheduleRange>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _personAssignment = _mock.StrictMock<IPersonAssignment>();
            _shiftCategories = new List<IShiftCategory>{_shiftCategory1,_shiftCategory2};
            _shiftCategoryPointDic = new Dictionary<IShiftCategory, int>();
            _shiftCategoryPointDic.Add(_shiftCategory1 ,1);
            _shiftCategoryPointDic.Add(_shiftCategory2 ,2);
        }

        [Test]
        public void TestUsingShiftCategoriesWithoutDayOff()
        {
            var requestedPeriod = new DateOnlyPeriod(2014,02,16,2014,02,18);
            using (_mock.Record())
            {
                Expect.Call(_shiftCategoryPoints.ExtractShiftCategoryPoints(_shiftCategories))
                      .IgnoreArguments().Return(_shiftCategoryPointDic);
                expectCall(_scheduleDay, new DateOnly(2014, 02, 16), _personAssignment, _shiftCategory1);
                expectCall(_scheduleDay, new DateOnly(2014, 02, 17), _personAssignment, _shiftCategory1);
                expectCall(_scheduleDay, new DateOnly(2014, 02, 18), _personAssignment, _shiftCategory2);
            }
            using (_mock.Playback())
            {
                
                var result = _target.CalculateShiftCategorySeniorityValue(_scheduleRange, requestedPeriod,
                                                                          _shiftCategories);
                Assert.AreEqual(result,4);
            }
        }

        [Test]
        public void TestWithNullPersonAssignment()
        {
            var requestedPeriod = new DateOnlyPeriod(2014, 02, 16, 2014, 02, 18);
            using (_mock.Record())
            {
                Expect.Call(_shiftCategoryPoints.ExtractShiftCategoryPoints(_shiftCategories))
                      .IgnoreArguments().Return(_shiftCategoryPointDic);
                Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2014,02,16))).Return(_scheduleDay);
                Expect.Call(_scheduleDay.PersonAssignment()).Return(null);
                Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2014, 02, 17))).Return(_scheduleDay);
                Expect.Call(_scheduleDay.PersonAssignment()).Return(null);
                Expect.Call(_scheduleRange.ScheduledDay(new DateOnly(2014, 02, 18))).Return(_scheduleDay);
                Expect.Call(_scheduleDay.PersonAssignment()).Return(null);
            }
            using (_mock.Playback())
            {

                var result = _target.CalculateShiftCategorySeniorityValue(_scheduleRange, requestedPeriod,
                                                                          _shiftCategories);
                Assert.AreEqual(result, 0);
            }
        }

        [Test]
        public void TestWithNullShiftCategories()
        {
            var requestedPeriod = new DateOnlyPeriod(2014, 02, 16, 2014, 02, 18);
            using (_mock.Record())
            {
                Expect.Call(_shiftCategoryPoints.ExtractShiftCategoryPoints(_shiftCategories))
                      .IgnoreArguments().Return(_shiftCategoryPointDic);
                expectCall(_scheduleDay, new DateOnly(2014, 02, 16), _personAssignment, null);
                expectCall(_scheduleDay, new DateOnly(2014, 02, 17), _personAssignment, null);
                expectCall(_scheduleDay, new DateOnly(2014, 02, 18), _personAssignment, null);
            }
            using (_mock.Playback())
            {

                var result = _target.CalculateShiftCategorySeniorityValue(_scheduleRange, requestedPeriod,
                                                                          _shiftCategories);
                Assert.AreEqual(result, 0);
            }
        }
        private void expectCall(IScheduleDay scheduleDay, DateOnly dateOnly,  IPersonAssignment personAssignment, IShiftCategory shiftCategory)
        {
            Expect.Call(_scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
            Expect.Call(scheduleDay.PersonAssignment()).Return(personAssignment).Repeat.Twice();
            Expect.Call(personAssignment.ShiftCategory).Return(shiftCategory);
        }
    }
}
