using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class ShiftCategoryRestrictionShiftFilterTest
	{
		private MockRepository _mocks;
		private IShiftCategoryRestrictionShiftFilter _target;
		private IPerson _person;
		private WorkShiftFinderResult _finderResult;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;
		private DateOnly _dateOnly;
		private TimeZoneInfo _timeZoneInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person = PersonFactory.CreatePerson("Bill");
			_dateOnly = new DateOnly(2013, 3, 1);
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_finderResult = new WorkShiftFinderResult(_person, new DateOnly(2009, 2, 3));
			_target = new ShiftCategoryRestrictionShiftFilter();
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var category = ShiftCategoryFactory.CreateShiftCategory("dv");
			var ret = _target.Filter(category, null, _finderResult);
			Assert.IsNull(ret);
		}

		[Test]
		public void CanFilterOnCategoryWithEmptyList()
		{
			var category = ShiftCategoryFactory.CreateShiftCategory("dv");
			var ret = _target.Filter(category, new List<ShiftProjectionCache>(), _finderResult);
			Assert.IsNotNull(ret);
		}

		[Test]
		public void CanFilterOnCategoryWithCategoryIsNull()
		{
			var ret = _target.Filter(null, getCashes(), _finderResult);
			Assert.AreEqual(3, ret.Count);
		}

		[Test]
		public void CanFilterOnShiftCategories()
		{
			IShiftCategory shiftCategory1 = new ShiftCategory("wanted");
			IShiftCategory shiftCategory2 = new ShiftCategory("not wanted");

			var workShift1 = _mocks.StrictMock<IWorkShift>();
			var workShift2 = _mocks.StrictMock<IWorkShift>();
			var workShift3 = _mocks.StrictMock<IWorkShift>();

			var personalShiftMeetingTimeChecker = new PersonalShiftMeetingTimeChecker();
			var cache1 = new ShiftProjectionCache(workShift1,personalShiftMeetingTimeChecker);
			var cache2 = new ShiftProjectionCache(workShift2,personalShiftMeetingTimeChecker);
			var cache3 = new ShiftProjectionCache(workShift3,personalShiftMeetingTimeChecker);

			IList<ShiftProjectionCache> caches = new List<ShiftProjectionCache> { cache1, cache2, cache3 };
			var finderResult = new WorkShiftFinderResult(new Person(), new DateOnly());
			using (_mocks.Record())
			{
				Expect.Call(workShift1.ShiftCategory).Return(shiftCategory1).Repeat.AtLeastOnce();
				Expect.Call(workShift2.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();
				Expect.Call(workShift3.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();

			}

			using (_mocks.Playback())
			{
				var ret = _target.Filter(shiftCategory1, caches, finderResult);
				Assert.AreEqual(1, ret.Count);
				Assert.AreEqual(shiftCategory1, ret[0].TheWorkShift.ShiftCategory);
				ret = _target.Filter(shiftCategory2, caches, finderResult);
				Assert.AreEqual(2, ret.Count);
			}
		}

		private IList<ShiftProjectionCache> getCashes()
		{
			var tmpList = getWorkShifts();
			var retList = new List<ShiftProjectionCache>();
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(_dateOnly, _timeZoneInfo);
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(dateOnlyAsDateTimePeriod);
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
