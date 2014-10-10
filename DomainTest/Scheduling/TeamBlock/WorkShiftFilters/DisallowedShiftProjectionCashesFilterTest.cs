using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class DisallowedShiftProjectionCashesFilterTest
	{
		private DisallowedShiftProjectionCashesFilter _target;
		private MockRepository _mock;
		private IShiftProjectionCache _shiftProjectionCache1;
		private IShiftProjectionCache _shiftProjectionCache2;
		private IShiftProjectionCache _notAllowedShiftProjectionCache;

		[SetUp]
		public void SetUp()
		{
			_target = new DisallowedShiftProjectionCashesFilter();	
			_mock = new MockRepository();
			_shiftProjectionCache1 = _mock.StrictMock<IShiftProjectionCache>();
			_shiftProjectionCache2 = _mock.StrictMock<IShiftProjectionCache>();
			_notAllowedShiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
		}

		[Test]
		public void ShouldReturnNullWhenShiftProjectinCashesIsNull()
		{
			var result = _target.Filter(new List<IShiftProjectionCache>(), null, new WorkShiftFinderResultForTest());
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldReturnNullWhenFinderResultIsNull()
		{
			var result = _target.Filter(new List<IShiftProjectionCache>(), new List<IShiftProjectionCache>(), null);
			Assert.IsNull(result);	
		}

		[Test]
		public void ShouldReturnShiftProjectionCashesWhenNoShiftProjectionCashesInList()
		{
			var shiftProjectionCashes = new List<IShiftProjectionCache>();
			var result = _target.Filter(new List<IShiftProjectionCache>(), shiftProjectionCashes, new WorkShiftFinderResultForTest());
			Assert.AreEqual(shiftProjectionCashes, result);		
		}

		[Test]
		public void ShouldReturnShiftProjectionCashesWhenNoShiftProjectionCashesInNotAllowedList()
		{
			var shiftProjectionCashes = new List<IShiftProjectionCache>{_shiftProjectionCache1};

			var result = _target.Filter(new List<IShiftProjectionCache>(), shiftProjectionCashes, new WorkShiftFinderResultForTest());
			Assert.AreEqual(shiftProjectionCashes, result);
		}

		[Test]
		public void ShouldFilter()
		{
			var shiftProjectionCash1 = new shiftProjectionCasheForTestReturn1();
			var shiftProjectionCash2 = new shiftProjectionCasheForTestReturn2();

			var shiftProjectionCashes = new List<IShiftProjectionCache> { shiftProjectionCash1, shiftProjectionCash2 };
			var notAllowedShiftProjectionCashes = new List<IShiftProjectionCache> { shiftProjectionCash1 };

			var result = _target.Filter(notAllowedShiftProjectionCashes, shiftProjectionCashes, new WorkShiftFinderResultForTest());
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(shiftProjectionCash2, result[0]);
				
		}

		private class shiftProjectionCasheForTestReturn1 : ShiftProjectionCache
		{
			public override int GetHashCode()
			{
				return 1;
			}
		}

		private class shiftProjectionCasheForTestReturn2 : ShiftProjectionCache
		{
			public override int GetHashCode()
			{
				return 2;
			}
		}
	}
}
