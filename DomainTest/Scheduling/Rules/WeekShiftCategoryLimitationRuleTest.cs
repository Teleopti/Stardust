using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class WeekShiftCategoryLimitationRuleTest
    {
        private MockRepository _mocks;
        private INewBusinessRule _target;
        private IShiftCategoryLimitationChecker _limitationChecker;
        private Dictionary<IPerson, IScheduleRange> _dic;
        private IVirtualSchedulePeriodExtractor _virtualSchedulePeriodExtractor;
        private IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
        private IShiftCategoryLimitation _limitation;
        IList<IShiftCategoryLimitation> _limits;
        private ReadOnlyCollection<IShiftCategoryLimitation> _limitations;
        private IShiftCategory _shiftCategory;
        private IPermissionInformation _permissionInformation;
        private TimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _limitationChecker = _mocks.StrictMock<IShiftCategoryLimitationChecker>();
            _dic = _mocks.StrictMock<Dictionary<IPerson, IScheduleRange>>();
            _virtualSchedulePeriodExtractor = _mocks.StrictMock<IVirtualSchedulePeriodExtractor>();
            _weeksFromScheduleDaysExtractor = _mocks.StrictMock<IWeeksFromScheduleDaysExtractor>();
            _target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor, _weeksFromScheduleDaysExtractor);

            _limitation = _mocks.StrictMock<IShiftCategoryLimitation>();

            _limits = new List<IShiftCategoryLimitation> { _limitation };
            _limitations = new ReadOnlyCollection<IShiftCategoryLimitation>(_limits);
            _shiftCategory = new ShiftCategory("Dummy");
            _permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }

        [Test]
        public void CanCreateNewShiftCategoryLimitationRule()
        {
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            Assert.AreEqual("", _target.ErrorMessage);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnsEmptyListWhenNotTooManyOfSameCategoryInWeek()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2010, 8, 2, 2010, 8, 29);
            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange>();
            _dic.Add(person, range);
            _target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor, _weeksFromScheduleDaysExtractor);

            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };
            IVirtualSchedulePeriod vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IVirtualSchedulePeriod vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1, vPeriod2 };

            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            var oldResponses = new List<IBusinessRuleResponse>();
            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
                    Return(vPeriods);
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(
                    personWeeks);
                Expect.Call(vPeriod1.IsValid).Return(true).Repeat.Twice();
                Expect.Call(vPeriod2.IsValid).Return(true).Repeat.Twice();

                Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
                Expect.Call(vPeriod2.DateOnlyPeriod).Return(dateOnlyPeriod);

                Expect.Call(vPeriod1.Person).Return(person).Repeat.Times(4);
                Expect.Call(vPeriod2.Person).Return(person).Repeat.Times(3);

                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

                Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
                Expect.Call(vPeriod2.ShiftCategoryLimitationCollection()).Return(_limitations);

                Expect.Call(_limitation.Weekly).Return(true).Repeat.Twice();
                Expect.Call(person.Equals(person)).Return(true).Repeat.Twice();
                IList<DateOnly> datesWithCategory;
                Expect.Call(_limitationChecker.IsShiftCategoryOverWeekLimit(_limitation, range, new DateOnlyPeriod(new DateOnly(2010, 8, 23), new DateOnly(2010, 8, 29)), out datesWithCategory)).
                    Return(false).Repeat.Twice();

            }
            using (_mocks.Playback())
            {
                IEnumerable<IBusinessRuleResponse> ret = _target.Validate(_dic, lstOfDays);
                Assert.AreEqual(0, ret.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnsListOfResponsesWhenTooManyOfSameCategoryInWeek()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2010, 8, 2, 2010, 8, 29);
            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange>();
            _dic.Add(person, range);
            _target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor, _weeksFromScheduleDaysExtractor);

            var scheduleDay = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay };
            IVirtualSchedulePeriod vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1 };

            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            IList<DateOnly> datesWithCategory = new List<DateOnly>();
            datesWithCategory.Add(new DateOnly(2010, 8, 22));
            datesWithCategory.Add(new DateOnly(2010, 8, 23));
            datesWithCategory.Add(new DateOnly(2010, 8, 24));
            datesWithCategory.Add(new DateOnly(2010, 8, 25));
            var oldResponses = new List<IBusinessRuleResponse>();
            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
                    Return(vPeriods);
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(
                    personWeeks);
                Expect.Call(vPeriod1.IsValid).Return(true).Repeat.Twice();

                Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);

                Expect.Call(vPeriod1.Person).Return(person).Repeat.Times(4);

                Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);

                Expect.Call(_limitation.Weekly).Return(true);
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.Equals(person)).Return(true);

                Expect.Call(_limitationChecker.IsShiftCategoryOverWeekLimit(_limitation, range, new DateOnlyPeriod(new DateOnly(2010, 8, 23), new DateOnly(2010, 8, 29)), out datesWithCategory)).
                    Return(true).OutRef(datesWithCategory);

                Expect.Call(_limitation.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                IEnumerable<IBusinessRuleResponse> ret = _target.Validate(_dic, lstOfDays);
                Assert.AreNotEqual(0, ret.Count());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ValidateReturnsListWhenTooManyOfSameCategoryInWeekIsEmpty()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2010, 8, 2, 2010, 8, 29);
            var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
            var person = _mocks.StrictMock<IPerson>();
            var range = _mocks.StrictMock<IScheduleRange>();
            _dic = new Dictionary<IPerson, IScheduleRange>();
            _dic.Add(person, range);
            _target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor, _weeksFromScheduleDaysExtractor);

            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

            var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };
            IVirtualSchedulePeriod vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IVirtualSchedulePeriod vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1, vPeriod2 };

            var personWeek = new PersonWeek(person, weekPeriod);

            var personWeeks = new List<PersonWeek> { personWeek };
            IList<DateOnly> datesWithCategory = new List<DateOnly>();
            datesWithCategory.Add(new DateOnly(2010, 8, 22));
            datesWithCategory.Add(new DateOnly(2010, 8, 23));
            datesWithCategory.Add(new DateOnly(2010, 8, 24));
            datesWithCategory.Add(new DateOnly(2010, 8, 25));
            var oldResponses = new List<IBusinessRuleResponse>();
            using (_mocks.Record())
            {
                Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
                    Return(vPeriods);
                Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(
                    personWeeks);
                Expect.Call(vPeriod1.IsValid).Return(true).Repeat.Twice() ;
                Expect.Call(vPeriod2.IsValid).Return(true).Repeat.Twice();

                Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
                Expect.Call(vPeriod2.DateOnlyPeriod).Return(dateOnlyPeriod);

                Expect.Call(vPeriod1.Person).Return(person).Repeat.Times(4);
                Expect.Call(vPeriod2.Person).Return(person).Repeat.Times(3);

                Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
                Expect.Call(vPeriod2.ShiftCategoryLimitationCollection()).Return(_limitations);

                Expect.Call(_limitation.Weekly).Return(true).Repeat.Twice();
                Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
                Expect.Call(person.Equals(person)).Return(true).Repeat.Twice();

                Expect.Call(_limitationChecker.IsShiftCategoryOverWeekLimit(_limitation, range, new DateOnlyPeriod(new DateOnly(2010, 8, 23), new DateOnly(2010, 8, 29)), out datesWithCategory)).
                    Return(true).Repeat.Twice().OutRef(datesWithCategory);

                Expect.Call(_limitation.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
                Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
                Expect.Call(person.Equals(person)).Return(true).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                IEnumerable<IBusinessRuleResponse> ret = _target.Validate(_dic, lstOfDays);
                Assert.AreEqual(4, ret.Count());
            }
        }

	    [Test]
	    public void ShouldValidateAllRangeClones()
	    {
			var dateOnlyPeriod = new DateOnlyPeriod(2010, 8, 2, 2010, 8, 29);
			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var person = _mocks.StrictMock<IPerson>();
		    var person2 = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var range2 = _mocks.StrictMock<IScheduleRange>();

		    _dic = new Dictionary<IPerson, IScheduleRange> {{person, range}, {person2, range2}};		    
		    _target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor, _weeksFromScheduleDaysExtractor);

			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };
			var vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1, vPeriod2 };

			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> { personWeek };
			IList<DateOnly> datesWithCategory = new List<DateOnly>();
			datesWithCategory.Add(new DateOnly(2010, 8, 22));
			datesWithCategory.Add(new DateOnly(2010, 8, 23));
			datesWithCategory.Add(new DateOnly(2010, 8, 24));
			datesWithCategory.Add(new DateOnly(2010, 8, 25));
			var oldResponses = new List<IBusinessRuleResponse>();
			using (_mocks.Record())
			{
				Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
					Return(vPeriods);
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).Return(
					personWeeks);
				Expect.Call(vPeriod1.IsValid).Return(true).Repeat.Twice();
				Expect.Call(vPeriod2.IsValid).Return(true).Repeat.Twice();

				Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
				Expect.Call(vPeriod2.DateOnlyPeriod).Return(dateOnlyPeriod);

				Expect.Call(vPeriod1.Person).Return(person).Repeat.Twice();
				Expect.Call(vPeriod2.Person).Return(person);

				Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
				Expect.Call(vPeriod2.ShiftCategoryLimitationCollection()).Return(_limitations);

				Expect.Call(_limitation.Weekly).Return(true).Repeat.Twice();
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.Equals(person)).Return(true).Repeat.Twice();

				Expect.Call(_limitationChecker.IsShiftCategoryOverWeekLimit(_limitation, range, weekPeriod, out datesWithCategory)).
					Return(true).Repeat.Twice().OutRef(datesWithCategory);
				Expect.Call(_limitationChecker.IsShiftCategoryOverWeekLimit(_limitation, range2, weekPeriod, out datesWithCategory))
					.Return(true).Repeat.Twice().OutRef(datesWithCategory);

				Expect.Call(_limitation.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
				Expect.Call(person.Equals(person)).Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				IEnumerable<IBusinessRuleResponse> ret = _target.Validate(_dic, lstOfDays);
				Assert.AreEqual(4, ret.Count());
			}
		}

	    [Test]
	    public void ShouldNotCrash_EmptyVirtualScheduleFromScheduleDay()
	    {
		    var scheduleDays = new List<IScheduleDay>();
		    var rangeClones = new Dictionary<IPerson, IScheduleRange>();

		    _virtualSchedulePeriodExtractor.Expect(v => v.CreateVirtualSchedulePeriodsFromScheduleDays(null))
		                                   .IgnoreArguments()
		                                   .Return(new List<IVirtualSchedulePeriod>());
		    _weeksFromScheduleDaysExtractor.Expect(w => w.CreateWeeksFromScheduleDaysExtractor(null))
		                                   .IgnoreArguments()
		                                   .Return(new List<PersonWeek>());
		    _mocks.ReplayAll();

		    Assert.DoesNotThrow(() => _target.Validate(rangeClones, scheduleDays));
		    _mocks.VerifyAll();
	    }
    }
}
