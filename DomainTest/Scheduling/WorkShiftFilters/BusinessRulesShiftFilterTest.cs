using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftFilters
{
	[TestFixture]
	public class BusinessRulesShiftFilterTest
	{
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _resultStateHolder;
		private IValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private ILongestPeriodForAssignmentCalculator _longestPeriodForAssignmentCalculator;
		private BusinessRulesShiftFilter _target;
		private IScheduleRange _scheduleRange;
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
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_target = new BusinessRulesShiftFilter(_resultStateHolder, _validDateTimePeriodShiftFilter,
												   _longestPeriodForAssignmentCalculator);
		}

		[Test]
		public void ShouldFilterAccordingToBusinessRules()
		{
			var person = PersonFactory.CreatePerson("Bill");
			var finderResult = new WorkShiftFinderResult(person, new DateOnly());
			using (_mocks.Record())
			{
				Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[person]).Return(_scheduleRange);
				Expect.Call(_longestPeriodForAssignmentCalculator.PossiblePeriod(_scheduleRange, _dateOnly)).Return(null);
			}
			using (_mocks.Playback())
			{
				var ret = _target.Filter(person, getCashes(), _dateOnly, finderResult);
				Assert.AreEqual(0, ret.Count);
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
