using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	public class WeekShiftCategoryLimitationRuleTest
	{
		private WeekShiftCategoryLimitationRule _target;
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
			_limitationChecker = MockRepository.GenerateMock<IShiftCategoryLimitationChecker>();
			_dic = MockRepository.GenerateMock<Dictionary<IPerson, IScheduleRange>>();
			_virtualSchedulePeriodExtractor = MockRepository.GenerateMock<IVirtualSchedulePeriodExtractor>();
			_weeksFromScheduleDaysExtractor = MockRepository.GenerateMock<IWeeksFromScheduleDaysExtractor>();
			_target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor,
				_weeksFromScheduleDaysExtractor);

			_limitation = MockRepository.GenerateMock<IShiftCategoryLimitation>();

			_limits = new List<IShiftCategoryLimitation> {_limitation};
			_limitations = new ReadOnlyCollection<IShiftCategoryLimitation>(_limits);
			_shiftCategory = new ShiftCategory("Dummy");
			_permissionInformation = MockRepository.GenerateMock<IPermissionInformation>();
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
		}

		[Test]
		public void CanCreateNewShiftCategoryLimitationRule()
		{
			Assert.IsNotNull(_target);
			Assert.IsFalse(_target.IsMandatory);
			Assert.IsTrue(_target.HaltModify);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability",
			 "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ValidateReturnsEmptyListWhenNotTooManyOfSameCategoryInWeek()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2010, 8, 2, 2010, 8, 29);
			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var person = MockRepository.GenerateMock<IPerson>();
			var range = MockRepository.GenerateMock<IScheduleRange>();
			_dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			_target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor,
				_weeksFromScheduleDaysExtractor);

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var scheduleDay2 = MockRepository.GenerateMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};
			var vPeriod1 = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var vPeriods = new List<IVirtualSchedulePeriod> {vPeriod1};
			var personWeek = new PersonWeek(person, weekPeriod);
			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();

			_virtualSchedulePeriodExtractor.Stub(x => x.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays))
				.Return(vPeriods);
			_weeksFromScheduleDaysExtractor.Stub(x => x.CreateWeeksFromScheduleDaysExtractor(lstOfDays))
				.Return(personWeeks);
			vPeriod1.Stub(x => x.IsValid).Return(true).Repeat.AtLeastOnce();
			vPeriod1.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);
			vPeriod1.Stub(x => x.Person).Return(person).Repeat.AtLeastOnce();
			range.Stub(x => x.BusinessRuleResponseInternalCollection).Return(oldResponses);
			person.Stub(x => x.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
			_permissionInformation.Stub(x => x.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
			vPeriod1.Stub(x => x.ShiftCategoryLimitationCollection()).Return(_limitations);
			_limitation.Stub(x => x.Weekly).Return(true);
			person.Stub(x => x.Equals(person)).Return(true).Repeat.AtLeastOnce();
			IList<DateOnly> datesWithCategory;
			_limitationChecker.Stub(
					x =>
						x.IsShiftCategoryOverWeekLimit(_limitation, range,
							new DateOnlyPeriod(new DateOnly(2010, 8, 23), new DateOnly(2010, 8, 29)), out datesWithCategory))
				.
				Return(false);

			var ret = _target.Validate(_dic, lstOfDays);
			Assert.AreEqual(0, ret.Count());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability",
			 "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ValidateReturnsListOfResponsesWhenTooManyOfSameCategoryInWeek()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2010, 8, 2, 2010, 8, 29);
			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var person = MockRepository.GenerateMock<IPerson>();
			var range = MockRepository.GenerateMock<IScheduleRange>();
			_dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			_target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor,
				_weeksFromScheduleDaysExtractor);

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay};
			var vPeriod1 = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var vPeriods = new List<IVirtualSchedulePeriod> {vPeriod1};

			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			IList<DateOnly> datesWithCategory = new List<DateOnly>();
			datesWithCategory.Add(new DateOnly(2010, 8, 22));
			datesWithCategory.Add(new DateOnly(2010, 8, 23));
			datesWithCategory.Add(new DateOnly(2010, 8, 24));
			datesWithCategory.Add(new DateOnly(2010, 8, 25));
			var oldResponses = new List<IBusinessRuleResponse>();

			_virtualSchedulePeriodExtractor.Stub(x => x.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays))
				.Return(vPeriods);
			_weeksFromScheduleDaysExtractor.Stub(x => x.CreateWeeksFromScheduleDaysExtractor(lstOfDays))
				.Return(personWeeks);
			vPeriod1.Stub(x => x.IsValid).Return(true).Repeat.Twice();
			vPeriod1.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);
			vPeriod1.Stub(x => x.Person).Return(person).Repeat.Times(4);
			vPeriod1.Stub(x => x.ShiftCategoryLimitationCollection()).Return(_limitations);
			_limitation.Stub(x => x.Weekly).Return(true);
			range.Stub(x => x.BusinessRuleResponseInternalCollection).Return(oldResponses);
			person.Stub(x => x.Equals(person)).Return(true).Repeat.AtLeastOnce();
			_limitationChecker.Stub(
					x =>
						x.IsShiftCategoryOverWeekLimit(_limitation, range,
							new DateOnlyPeriod(new DateOnly(2010, 8, 23), new DateOnly(2010, 8, 29)), out datesWithCategory))
				.
				Return(true).OutRef(datesWithCategory);

			_limitation.Stub(x => x.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
			person.Stub(x => x.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
			_permissionInformation.Stub(x => x.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

			var ret = _target.Validate(_dic, lstOfDays);
			Assert.AreNotEqual(0, ret.Count());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability",
			 "CA1506:AvoidExcessiveClassCoupling"), Test, SetUICulture("sv-SE")]
		public void ValidateReturnsListWhenTooManyOfSameCategoryInWeekIsEmpty()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2010, 8, 2, 2010, 8, 29);
			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var person = MockRepository.GenerateMock<IPerson>();
			var range = MockRepository.GenerateMock<IScheduleRange>();
			_dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			_target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor,
				_weeksFromScheduleDaysExtractor);

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var scheduleDay2 = MockRepository.GenerateMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};
			var vPeriod1 = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var vPeriod2 = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var vPeriods = new List<IVirtualSchedulePeriod> {vPeriod1, vPeriod2};

			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			IList<DateOnly> datesWithCategory = new List<DateOnly>();
			datesWithCategory.Add(new DateOnly(2010, 8, 22));
			datesWithCategory.Add(new DateOnly(2010, 8, 23));
			datesWithCategory.Add(new DateOnly(2010, 8, 24));
			datesWithCategory.Add(new DateOnly(2010, 8, 25));
			var oldResponses = new List<IBusinessRuleResponse>();

			_virtualSchedulePeriodExtractor.Stub(x => x.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
				Return(vPeriods);
			_weeksFromScheduleDaysExtractor.Stub(x => x.CreateWeeksFromScheduleDaysExtractor(lstOfDays)).
				Return(personWeeks);
			vPeriod1.Stub(x => x.IsValid).Return(true).Repeat.Twice();
			vPeriod2.Stub(x => x.IsValid).Return(true).Repeat.Twice();

			vPeriod1.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);
			vPeriod2.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);

			vPeriod1.Stub(x => x.Person).Return(person).Repeat.Times(4);
			vPeriod2.Stub(x => x.Person).Return(person).Repeat.Times(3);

			vPeriod1.Stub(x => x.ShiftCategoryLimitationCollection()).
				Return(_limitations);
			vPeriod2.Stub(x => x.ShiftCategoryLimitationCollection()).
				Return(_limitations);

			_limitation.Stub(x => x.Weekly).Return(true).Repeat.Twice();
			range.Stub(x => x.BusinessRuleResponseInternalCollection).Return(oldResponses);
			person.Stub(x => x.Equals(person)).Return(true).Repeat.Twice();

			_limitationChecker.Stub(
					x =>
						x.IsShiftCategoryOverWeekLimit(_limitation, range,
							new DateOnlyPeriod(new DateOnly(2010, 8, 23), new DateOnly(2010, 8, 29)), out datesWithCategory))
				.
				Return(true).Repeat.Twice().OutRef(datesWithCategory);

			_limitation.Stub(x => x.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
			person.Stub(x => x.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
			_permissionInformation.Stub(x => x.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
			person.Stub(x => x.Equals(person)).Return(true).Repeat.AtLeastOnce();

			var ret = _target.Validate(_dic, lstOfDays).ToArray();
			Assert.AreEqual(4, ret.Length);
			foreach (var response in ret)
			{
				Assert.IsTrue(response.FriendlyName.StartsWith("Skiftkategoribegränsningarna är"));
				Assert.IsTrue(response.Message.StartsWith("Veckan har för"));
			}
		}

		[Test]
		public void ShouldValidateAllRangeClones()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2010, 8, 2, 2010, 8, 29);
			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var person = MockRepository.GenerateMock<IPerson>();
			var person2 = MockRepository.GenerateMock<IPerson>();
			var range = MockRepository.GenerateMock<IScheduleRange>();
			var range2 = MockRepository.GenerateMock<IScheduleRange>();

			_dic = new Dictionary<IPerson, IScheduleRange> {{person, range}, {person2, range2}};
			_target = new WeekShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor,
				_weeksFromScheduleDaysExtractor);

			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var scheduleDay2 = MockRepository.GenerateMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};
			var vPeriod1 = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var vPeriod2 = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var vPeriods = new List<IVirtualSchedulePeriod> {vPeriod1, vPeriod2};

			var personWeek = new PersonWeek(person, weekPeriod);
			var personWeek2 = new PersonWeek(person2, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek, personWeek2};
			IList<DateOnly> datesWithCategory = new List<DateOnly>();
			datesWithCategory.Add(new DateOnly(2010, 8, 22));
			datesWithCategory.Add(new DateOnly(2010, 8, 23));
			datesWithCategory.Add(new DateOnly(2010, 8, 24));
			datesWithCategory.Add(new DateOnly(2010, 8, 25));
			var oldResponses = new List<IBusinessRuleResponse>();

			_virtualSchedulePeriodExtractor.Stub(x => x.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays))
				.Return(vPeriods);
			_weeksFromScheduleDaysExtractor.Stub(x => x.CreateWeeksFromScheduleDaysExtractor(lstOfDays))
				.Return(personWeeks);
			vPeriod1.Stub(x => x.IsValid).Return(true).Repeat.Twice();
			vPeriod2.Stub(x => x.IsValid).Return(true).Repeat.Twice();

			vPeriod1.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);
			vPeriod2.Stub(x => x.DateOnlyPeriod).Return(dateOnlyPeriod);

			vPeriod1.Stub(x => x.Person).Return(person).Repeat.AtLeastOnce();
			vPeriod2.Stub(x => x.Person).Return(person2).Repeat.AtLeastOnce();

			vPeriod1.Stub(x => x.ShiftCategoryLimitationCollection()).Return(_limitations);
			vPeriod2.Stub(x => x.ShiftCategoryLimitationCollection()).Return(_limitations);

			_limitation.Stub(x => x.Weekly).Return(true).Repeat.Twice();
			range.Stub(x => x.BusinessRuleResponseInternalCollection).Return(oldResponses);
			person.Stub(x => x.Equals(person)).Return(true).Repeat.AtLeastOnce();
			person2.Stub(x => x.Equals(person2)).Return(true).Repeat.AtLeastOnce();

			_limitationChecker.Stub(
					x => x.IsShiftCategoryOverWeekLimit(_limitation, range, weekPeriod, out datesWithCategory)).
				Return(true).OutRef(datesWithCategory);
			_limitationChecker.Stub(
					x => x.IsShiftCategoryOverWeekLimit(_limitation, range2, weekPeriod, out datesWithCategory))
				.Return(true).OutRef(datesWithCategory);

			_limitation.Stub(x => x.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
			person.Stub(x => x.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
			person2.Stub(x => x.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
			_permissionInformation.Stub(x => x.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
			person2.Stub(x => x.Equals(person)).Return(false).Repeat.AtLeastOnce();
			person.Stub(x => x.Equals(person2)).Return(false).Repeat.AtLeastOnce();

			var ret = _target.Validate(_dic, lstOfDays);
			Assert.AreEqual(8, ret.Count());
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

			Assert.DoesNotThrow(() => _target.Validate(rangeClones, scheduleDays));
		}
	}
}
