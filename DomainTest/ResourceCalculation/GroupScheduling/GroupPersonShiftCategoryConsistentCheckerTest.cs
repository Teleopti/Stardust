using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.GroupScheduling
{
	[TestFixture]
	public class GroupPersonShiftCategoryConsistentCheckerTest
	{
		private IGroupPersonShiftCategoryConsistentChecker _target;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _rangeUnscheduled;
		private IScheduleRange _rangeScheduled;
		private IScheduleRange _rangeScheduledOther;
		private IScheduleDay _unscheduledDay;

		private IScheduleDay _scheduledDay;
		private IScheduleDay _scheduledDayOtherCategory;

		private readonly IShiftCategory _category1 = new ShiftCategory("kat1");
		private readonly IShiftCategory _category2 = new ShiftCategory("kat2");

		private IPerson _person1;
		private IPerson _person2;
		private IList<IPerson> _persons;
		private MockRepository _mocks;
	    private IVirtualSchedulePeriod _virtualPeriod;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
	        _person1 = _mocks.StrictMock<IPerson>();
            _person2 = _mocks.StrictMock<IPerson>();
		    _virtualPeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_persons = new List<IPerson> { _person1, _person2 };
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_target = new GroupPersonShiftCategoryConsistentChecker();
			_rangeUnscheduled = _mocks.StrictMock<IScheduleRange>();
			_rangeScheduled = _mocks.StrictMock<IScheduleRange>();
			_rangeScheduledOther = _mocks.StrictMock<IScheduleRange>();
			_unscheduledDay = _mocks.StrictMock<IScheduleDay>();
			_scheduledDay = _mocks.StrictMock<IScheduleDay>();
			_scheduledDayOtherCategory = _mocks.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldReturnTrueIfAllUnscheduled()
		{
			var date = new DateOnly(2010,10,4);
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();

			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeUnscheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeUnscheduled);

			Expect.Call(_rangeUnscheduled.ScheduledDay(date)).Return(_unscheduledDay).Repeat.Twice();
			Expect.Call(_unscheduledDay.SignificantPart()).Return(SchedulePartView.None).Repeat.Twice();
			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneShiftCategoryScheduled(_scheduleDictionary, _persons, date);
			Assert.That(result,Is.True);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnTrueIfSameCategory()
		{
			var date = new DateOnly(2010, 10, 4);
			var ass = _mocks.StrictMock<IPersonAssignment>();
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();
			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeScheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeScheduled);

			Expect.Call(_rangeScheduled.ScheduledDay(date)).Return(_scheduledDay).Repeat.Twice();
			Expect.Call(_scheduledDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
			Expect.Call(_scheduledDay.PersonAssignment()).Return(ass).Repeat.Times(3);
			Expect.Call(ass.ShiftCategory).Return(_category1).Repeat.Times(3);
			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneShiftCategoryScheduled(_scheduleDictionary, _persons, date);
			Assert.That(result, Is.True);
			Assert.That(_target.CommonShiftCategory,Is.EqualTo(_category1));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfDifferentCategory()
		{
			var date = new DateOnly(2010, 10, 4);
			var ass = _mocks.StrictMock<IPersonAssignment>();
			var assOther = _mocks.StrictMock<IPersonAssignment>();
            Expect.Call(_person1.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_person2.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(true).Repeat.Twice();
			Expect.Call(_scheduleDictionary[_person1]).Return(_rangeScheduled);
			Expect.Call(_scheduleDictionary[_person2]).Return(_rangeScheduledOther);

			Expect.Call(_rangeScheduled.ScheduledDay(date)).Return(_scheduledDay);
			Expect.Call(_rangeScheduledOther.ScheduledDay(date)).Return(_scheduledDayOtherCategory);

			Expect.Call(_scheduledDay.SignificantPart()).Return(SchedulePartView.MainShift);
			Expect.Call(_scheduledDayOtherCategory.SignificantPart()).Return(SchedulePartView.MainShift);

			Expect.Call(_scheduledDay.PersonAssignment()).Return(ass).Repeat.Times(2);
			Expect.Call(_scheduledDayOtherCategory.PersonAssignment()).Return(assOther);

			Expect.Call(ass.ShiftCategory).Return(_category1).Repeat.Times(2);
			Expect.Call(assOther.ShiftCategory).Return(_category2);
			_mocks.ReplayAll();
			var result = _target.AllPersonsHasSameOrNoneShiftCategoryScheduled(_scheduleDictionary, _persons, date);
			Assert.That(result, Is.False);
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldNotFetchShiftCategoryWhenSchedulePeriodInvalid()
        {
            var date = new DateOnly(2010, 10, 4);
            var person = _mocks.StrictMock<IPerson>();
			_persons = new List<IPerson> { person };
            Expect.Call(person.VirtualSchedulePeriod(date)).Return(_virtualPeriod);
            Expect.Call(_virtualPeriod.IsValid).Return(false);

            _mocks.ReplayAll();
            var result = _target.AllPersonsHasSameOrNoneShiftCategoryScheduled(_scheduleDictionary, _persons, date);
            Assert.That(result, Is.True);
            _mocks.VerifyAll();
        }
	}

	
}