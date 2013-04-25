using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Interfaces.Domain;

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
			var ret = _target.Filter(new List<IShiftCategory> { shiftCategory1 }, new List<IShiftProjectionCache>(), new WorkShiftFinderResultForTest());
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

			var cache1 = _mocks.StrictMock<IShiftProjectionCache>();
			var cache2 = _mocks.StrictMock<IShiftProjectionCache>();
			var cache3 = _mocks.StrictMock<IShiftProjectionCache>();

			IList<IShiftProjectionCache> caches = new List<IShiftProjectionCache> { cache1, cache2, cache3 };
			IList<IShiftCategory> categoriesNotAllowed = new List<IShiftCategory> { shiftCategory2, shiftCategory3 };
			IWorkShiftFinderResult finderResult = new WorkShiftFinderResultForTest();
			using (_mocks.Record())
			{
				Expect.Call(cache1.TheWorkShift).Return(workShift1).Repeat.AtLeastOnce();
				Expect.Call(cache2.TheWorkShift).Return(workShift2).Repeat.AtLeastOnce();
				Expect.Call(cache3.TheWorkShift).Return(workShift3).Repeat.AtLeastOnce();

				Expect.Call(workShift1.ShiftCategory).Return(shiftCategory1).Repeat.AtLeastOnce();
				Expect.Call(workShift2.ShiftCategory).Return(shiftCategory2).Repeat.AtLeastOnce();
				Expect.Call(workShift3.ShiftCategory).Return(shiftCategory3).Repeat.AtLeastOnce();

			}

			using (_mocks.Playback())
			{
				var ret = _target.Filter(categoriesNotAllowed, caches, finderResult);
				Assert.AreEqual(1, ret.Count);
				Assert.AreEqual(shiftCategory1, ret[0].TheWorkShift.ShiftCategory);
				ret = _target.Filter(new List<IShiftCategory>(), caches, finderResult);
				Assert.AreEqual(3, ret.Count);
			}
		}

		[Test]
		public void ShouldCheckParameters()
		{
			IWorkShiftFinderResult finderResult = new WorkShiftFinderResultForTest();
			
			var result = _target.Filter(new List<IShiftCategory>(), null, finderResult);
			Assert.IsNull(result);
			
			result = _target.Filter(new List<IShiftCategory>(), new List<IShiftProjectionCache>(), null);
			Assert.IsNull(result);
		}
	}
}
