using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class DisallowedShiftCategoriesShiftFilterTest
	{
		private DisallowedShiftCategoriesShiftFilter _target;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new DisallowedShiftCategoriesShiftFilter();
		}

		[Test]
		public void CanFilterOnNotAllowedCategoriesWithEmptyList()
		{
			IShiftCategory shiftCategory1 = new ShiftCategory("allowed");
			var ret = _target.Filter(new HashSet<IShiftCategory> { shiftCategory1 }, new List<ShiftProjectionCache>());
			Assert.IsNotNull(ret);
		}

		[Test]
		public void CanFilterOnNotAllowedShiftCategories()
		{
			IShiftCategory shiftCategory1 = new ShiftCategory("allowed");
			IShiftCategory shiftCategory2 = new ShiftCategory("not allowed");
			IShiftCategory shiftCategory3 = new ShiftCategory("not allowed 2");

			var workShift1 = _mocks.StrictMock<IWorkShift>();
			var workShift2 = _mocks.StrictMock<IWorkShift>();
			var workShift3 = _mocks.StrictMock<IWorkShift>();

			var cache1 = new ShiftProjectionCache(workShift1, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			var cache2 = new ShiftProjectionCache(workShift2, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			var cache3 = new ShiftProjectionCache(workShift3, new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));

			IList<ShiftProjectionCache> caches = new List<ShiftProjectionCache> { cache1, cache2, cache3 };
			var categoriesNotAllowed = new HashSet<IShiftCategory> { shiftCategory2, shiftCategory3 };
			using (_mocks.Record())
			{
				Expect.Call(workShift1.ShiftCategory).Return(shiftCategory1).Repeat.AtLeastOnce();
				Expect.Call(workShift2.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();
				Expect.Call(workShift3.ShiftCategory).Return(shiftCategory3).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var ret = _target.Filter(categoriesNotAllowed, caches);
				Assert.AreEqual(1, ret.Count);
				Assert.AreEqual(shiftCategory1, ret[0].TheWorkShift.ShiftCategory);
				ret = _target.Filter(new HashSet<IShiftCategory>(), caches);
				Assert.AreEqual(3, ret.Count);
			}
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(new HashSet<IShiftCategory>(), null);
			Assert.IsNull(result);
		}
	}
}
