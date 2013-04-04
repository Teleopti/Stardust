using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class BusinessRulesShiftFilterTest
	{
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _resultStateHolder;
		private IValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private ILongestPeriodForAssignmentCalculator _longestPeriodForAssignmentCalculator;
		private BusinessRulesShiftFilter _target;
		private IScheduleDictionary _scheduleDictionary;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private DateOnly _dateOnly;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dateOnly = new DateOnly(2013, 3, 1);
			_resultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_validDateTimePeriodShiftFilter = _mocks.StrictMock<IValidDateTimePeriodShiftFilter>();
			_longestPeriodForAssignmentCalculator = _mocks.StrictMock<ILongestPeriodForAssignmentCalculator>();

			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_target = new BusinessRulesShiftFilter(_resultStateHolder, _validDateTimePeriodShiftFilter,
			                                       _longestPeriodForAssignmentCalculator);
		}

		[Test]
		public void ShouldFilterAccordingToBusinessRulesIfNoCommonPeriod()
		{
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			var groupPerson = new GroupPerson(new List<IPerson> {person1, person2}, DateOnly.MinValue, "team1", null);
			var finderResult = new WorkShiftFinderResult(groupPerson, new DateOnly());
			var scheduleRange1 = _mocks.StrictMock<IScheduleRange>();
			var scheduleRange2 = _mocks.StrictMock<IScheduleRange>();
			var period1 = new DateTimePeriod(new DateTime(2013, 3, 1, 7, 30, 0, DateTimeKind.Utc),
			                                 new DateTime(2013, 3, 1, 17, 30, 0, DateTimeKind.Utc));
			var shiftList = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.Twice();
				Expect.Call(_scheduleDictionary[person1]).Return(scheduleRange1);
				Expect.Call(_scheduleDictionary[person2]).Return(scheduleRange2);
				Expect.Call(_longestPeriodForAssignmentCalculator.PossiblePeriod(scheduleRange1, _dateOnly)).Return(period1);
				Expect.Call(_longestPeriodForAssignmentCalculator.PossiblePeriod(scheduleRange2, _dateOnly)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Filter(groupPerson, shiftList, _dateOnly, finderResult);
				Assert.That(result.Count, Is.EqualTo(0));
			}
		}

		[Test]
		public void ShouldFilterAccordingToIntersectionOfBusinessRules()
		{
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			var groupPerson = new GroupPerson(new List<IPerson> {person1, person2}, DateOnly.MinValue, "team1", null);
			var finderResult = new WorkShiftFinderResult(groupPerson, new DateOnly());
			var scheduleRange1 = _mocks.StrictMock<IScheduleRange>();
			var scheduleRange2 = _mocks.StrictMock<IScheduleRange>();
			var period1 = new DateTimePeriod(new DateTime(2013, 3, 1, 7, 30, 0, DateTimeKind.Utc),
			                                 new DateTime(2013, 3, 1, 17, 30, 0, DateTimeKind.Utc));
			var period2 = new DateTimePeriod(new DateTime(2013, 3, 1, 8, 0, 0, DateTimeKind.Utc),
			                                 new DateTime(2013, 3, 1, 18, 00, 0, DateTimeKind.Utc));
			var intesectedPeriod = new DateTimePeriod(new DateTime(2013, 3, 1, 8, 0, 0, DateTimeKind.Utc),
			                                          new DateTime(2013, 3, 1, 17, 30, 0, DateTimeKind.Utc));
			var shiftList = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.Twice();
				Expect.Call(_scheduleDictionary[person1]).Return(scheduleRange1);
				Expect.Call(_scheduleDictionary[person2]).Return(scheduleRange2);
				Expect.Call(_longestPeriodForAssignmentCalculator.PossiblePeriod(scheduleRange1, _dateOnly)).Return(period1);
				Expect.Call(_longestPeriodForAssignmentCalculator.PossiblePeriod(scheduleRange2, _dateOnly)).Return(period2);
				Expect.Call(_validDateTimePeriodShiftFilter.Filter(shiftList, intesectedPeriod, finderResult))
				      .Return(new List<IShiftProjectionCache>());
			}
			using (_mocks.Playback())
			{
				_target.Filter(groupPerson, shiftList, _dateOnly, finderResult);
			}
		}

		private IList<IShiftProjectionCache> getCashes()
		{
			var timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var tmpList = getWorkShifts();
			var retList = new List<IShiftProjectionCache>();
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(_dateOnly, timeZoneInfo);
				retList.Add(cache);
			}
			return retList;
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			var groupPerson = new GroupPerson(new List<IPerson> { person1, person2 }, DateOnly.MinValue, "team1", null);
			var finderResult = new WorkShiftFinderResult(groupPerson, new DateOnly());
			var shiftList = getCashes();
			var result = _target.Filter(null, shiftList, _dateOnly, finderResult);
			Assert.IsNull(result);
			 result = _target.Filter(groupPerson, null, _dateOnly, finderResult);
			Assert.IsNull(result);

			result = _target.Filter(groupPerson, new List<IShiftProjectionCache>(), _dateOnly, finderResult);
			Assert.That(result.Count, Is.EqualTo(0));
		}

		private static IEnumerable<IWorkShift> getWorkShifts()
		{
			var activity = ActivityFactory.CreateActivity("sd");
			var category = ShiftCategoryFactory.CreateShiftCategory("dv");
			var workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
														  activity, category);
			var workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														  activity, category);
			var workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
																	  activity, category);

			return new List<IWorkShift> { workShift1, workShift2, workShift3 };
		}
	}
}
