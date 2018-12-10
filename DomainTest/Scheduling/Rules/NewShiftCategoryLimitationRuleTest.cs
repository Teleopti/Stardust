using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class NewShiftCategoryLimitationRuleTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		private MockRepository _mocks;
		private NewShiftCategoryLimitationRule _target;
		private IShiftCategoryLimitationChecker _limitationChecker;
		private Dictionary<IPerson, IScheduleRange> _dic;
		private IVirtualSchedulePeriodExtractor _virtualSchedulePeriodExtractor;
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
			_target = new NewShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor);

			_limitation = _mocks.StrictMock<IShiftCategoryLimitation>();

			_limits = new List<IShiftCategoryLimitation> { _limitation };
			_limitations = new ReadOnlyCollection<IShiftCategoryLimitation>(_limits);
			_shiftCategory = new ShiftCategory("Dummy");
			_permissionInformation = _mocks.StrictMock<IPermissionInformation>();
			_timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
		}

		[Test]
		public void ShouldValidateEachShiftCategoryIsolatedFromOthersBug38715()
		{
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_");
			var dateOnly = new DateOnly(2016, 05, 23);
			var shiftCategory1 = new ShiftCategory("_").WithId();
			var shiftCategory2 = new ShiftCategory("_").WithId();

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;
			schedulePeriod.AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory1) { MaxNumberOf = 0, Weekly = false });
			schedulePeriod.AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory2) { MaxNumberOf = 0, Weekly = false });

			var ass1 = new PersonAssignment(agent, scenario, dateOnly); //should alert
			ass1.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass1.SetShiftCategory(shiftCategory1);

			var ass2 = new PersonAssignment(agent, scenario, dateOnly.AddDays(1)); //should alert
			ass2.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass2.SetShiftCategory(shiftCategory2);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1),
				new[] { agent }, new[] { ass1, ass2 }, Enumerable.Empty<ISkillDay>());

			var scheduleDayToModify = stateHolder.Schedules.SchedulesForDay(dateOnly).First();
			scheduleDayToModify.CreateAndAddPublicNote("_");

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			var brokenRules = stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDayToModify, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			foreach (var businessRuleResponse in brokenRules)
			{
				bussinesRuleCollection.DoNotHaltModify(businessRuleResponse);
			}

			stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDayToModify, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(1);

			scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly.AddDays(1));
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void CanCreateNewShiftCategoryLimitationRule()
		{
			Assert.IsNotNull(_target);
			Assert.IsFalse(_target.IsMandatory);
			Assert.IsTrue(_target.HaltModify);
		}

		[Test]
		public void ValidateReturnsEmptyListWhenNotTooManyOfSameCategoryInPeriod()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			_dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
			_target = new NewShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor);

			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };

			var vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1, vPeriod2 };
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
					Return(vPeriods);

				Expect.Call(vPeriod1.IsValid).Return(true);
				Expect.Call(vPeriod2.IsValid).Return(true);

				Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
				Expect.Call(vPeriod2.DateOnlyPeriod).Return(dateOnlyPeriod);

				Expect.Call(vPeriod1.Person).Return(person);
				Expect.Call(vPeriod2.Person).Return(person);

				Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
				Expect.Call(vPeriod2.ShiftCategoryLimitationCollection()).Return(_limitations);

				Expect.Call(_limitation.Weekly).Return(false).Repeat.Twice();
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses).Repeat.Twice();

				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				IList<DateOnly> datesWithCategory;
				Expect.Call(_limitationChecker.IsShiftCategoryOverPeriodLimit(_limitation, dateOnlyPeriod, range, out datesWithCategory)).
					Return(false).Repeat.Twice();

			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(_dic, lstOfDays);
				Assert.AreEqual(0, ret.Count());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
			Test]
		public void ValidateReturnsListOfResponseWhenTooManyOfSameCategoryInPeriod()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			_dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
			_target = new NewShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor);

			var scheduleDay = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> { scheduleDay };

			var vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1 };
			IList<DateOnly> datesWithCategory = new List<DateOnly>();
			datesWithCategory.Add(new DateOnly(2009, 2, 5));
			datesWithCategory.Add(new DateOnly(2009, 2, 10));
			datesWithCategory.Add(new DateOnly(2009, 2, 12));
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
					Return(vPeriods);
				Expect.Call(vPeriod1.IsValid).Return(true);
				Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
				Expect.Call(vPeriod1.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
				Expect.Call(_limitation.Weekly).Return(false);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(_limitationChecker.IsShiftCategoryOverPeriodLimit(_limitation, dateOnlyPeriod, range, out datesWithCategory)).
					Return(true).OutRef(datesWithCategory);
				Expect.Call(_limitation.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(_dic, lstOfDays);
				Assert.AreNotEqual(0, ret.Count());
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ValidateReturnsListWithTooManyShiftCategoryButNoErrorInList()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2009, 2, 2, 2009, 3, 1);
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			_dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };
			_target = new NewShiftCategoryLimitationRule(_limitationChecker, _virtualSchedulePeriodExtractor);

			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> { scheduleDay, scheduleDay2 };

			var vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var vPeriods = new List<IVirtualSchedulePeriod> { vPeriod1, vPeriod2 };
			IList<DateOnly> datesWithCategory = new List<DateOnly>();
			datesWithCategory.Add(new DateOnly(2009, 2, 5));
			datesWithCategory.Add(new DateOnly(2009, 2, 10));
			datesWithCategory.Add(new DateOnly(2009, 2, 12));
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(lstOfDays)).
					Return(vPeriods);

				Expect.Call(vPeriod1.IsValid).Return(true);
				Expect.Call(vPeriod2.IsValid).Return(true);

				Expect.Call(vPeriod1.DateOnlyPeriod).Return(dateOnlyPeriod);
				Expect.Call(vPeriod2.DateOnlyPeriod).Return(dateOnlyPeriod);

				Expect.Call(vPeriod1.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(vPeriod2.Person).Return(person).Repeat.AtLeastOnce();

				Expect.Call(vPeriod1.ShiftCategoryLimitationCollection()).Return(_limitations);
				Expect.Call(vPeriod2.ShiftCategoryLimitationCollection()).Return(_limitations);

				Expect.Call(_limitation.Weekly).Return(false).Repeat.Twice();
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses).Repeat.Twice();
				Expect.Call(_limitationChecker.IsShiftCategoryOverPeriodLimit(_limitation, dateOnlyPeriod, range, out datesWithCategory)).
					Return(true).Repeat.Twice().OutRef(datesWithCategory);
				Expect.Call(_limitation.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
				Expect.Call(person.Equals(person)).Return(true).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(_dic, lstOfDays);
				Assert.AreEqual(0, ret.Count());
			}
		}
	}
}
