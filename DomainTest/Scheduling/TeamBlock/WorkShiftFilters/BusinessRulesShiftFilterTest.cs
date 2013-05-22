using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

			_mocks.StrictMock<IScheduleDictionary>();
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_target = new BusinessRulesShiftFilter(_resultStateHolder, _validDateTimePeriodShiftFilter,
			                                       _longestPeriodForAssignmentCalculator);
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
			var finderResult = new WorkShiftFinderResult(person1, new DateOnly());
			var shiftList = getCashes();
			var result = _target.Filter(null, shiftList, _dateOnly, finderResult);
			Assert.IsNull(result);
			result = _target.Filter(person1, null, _dateOnly, finderResult);
			Assert.IsNull(result);

			result = _target.Filter(person1, shiftList, _dateOnly, null);
			Assert.IsNull(result);

			result = _target.Filter(person1, new List<IShiftProjectionCache>(), _dateOnly, finderResult);
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
