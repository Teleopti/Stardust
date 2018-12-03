using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class CommonMainShiftFilterTest
	{
		private CommonMainShiftFilter _target;
		private MockRepository _mocks;
		private DateOnly _dateOnly;
		private static IActivity _activity;
		private static IShiftCategory _category;
		private TimeZoneInfo _timeZoneInfo;
		private IScheduleDayEquator _scheduleDayEquator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dateOnly = new DateOnly(2013, 3, 1);
			_activity = ActivityFactory.CreateActivity("sd");
			_activity.SetId(Guid.NewGuid());
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
			_category.SetId(Guid.NewGuid());
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_target = new CommonMainShiftFilter(_scheduleDayEquator);
		}

		[Test]
		public void ShouldFilterIfThereIsAnyCommonMainShift()
		{
			var mainShift = EditableShiftFactory.CreateEditorShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
															 _activity, _category);
			var effectiveRestriction = new EffectiveRestriction(
				new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
				new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
				new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
				null, null, null, new List<IActivityRestriction>()) { CommonMainShift = mainShift };
			var shifts = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDayEquator.MainShiftBasicEquals(shifts[2].TheMainShift, mainShift)).IgnoreArguments().Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.Filter(getCashes(), effectiveRestriction);

				Assert.That(result.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void ShouldReturnNullNoCommonMainShiftAvailable()
		{
			var mainShift = EditableShiftFactory.CreateEditorShift(new TimeSpan(11, 0, 0), new TimeSpan(19, 0, 0),
															 _activity, _category);
			var effectiveRestriction = new EffectiveRestriction(
				new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
				new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
				new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
				null, null, null, new List<IActivityRestriction>()) { CommonMainShift = mainShift };
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDayEquator.MainShiftBasicEquals(mainShift, mainShift)).IgnoreArguments().Return(false).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var result = _target.Filter(getCashes(), effectiveRestriction);

				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var effectiveRestriction = new EffectiveRestriction(
				new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
				new EndTimeLimitation(new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0)),
				new WorkTimeLimitation(new TimeSpan(5, 0, 0), new TimeSpan(8, 0, 0)),
				null, null, null, new List<IActivityRestriction>());
			var result = _target.Filter(new List<ShiftProjectionCache>(), effectiveRestriction);
			Assert.That(result.Count, Is.EqualTo(0));

			result = _target.Filter(getCashes(), effectiveRestriction);
			Assert.That(result.Count, Is.EqualTo(3));

			result = _target.Filter(null, effectiveRestriction);
			Assert.IsNull(result);
		}

		private IList<ShiftProjectionCache> getCashes()
		{
			var tmpList = getWorkShifts();
			var retList = new List<ShiftProjectionCache>();
			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(_dateOnly, _timeZoneInfo);
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, dateOnlyAsDateTimePeriod);
				retList.Add(cache);
			}
			return retList;
		}

		private static IEnumerable<IWorkShift> getWorkShifts()
		{
			var workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
															  _activity, _category);
			var workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														  _activity, _category);
			var workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
																	  _activity, _category);
			return new List<IWorkShift> { workShift1, workShift2, workShift3 };
		}
	}
}
