using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class DisallowedShiftProjectionCashesFilterTest
	{
		private DisallowedShiftProjectionCachesFilter _target;
		private ShiftProjectionCache _shiftProjectionCache1;

		[SetUp]
		public void SetUp()
		{
			_target = new DisallowedShiftProjectionCachesFilter();
			_shiftProjectionCache1 = new ShiftProjectionCache(new WorkShift(new ShiftCategory("Late")), new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
		}

		[Test]
		public void ShouldReturnNullWhenShiftProjectinCashesIsNull()
		{
			var result = _target.Filter(new List<ShiftProjectionCache>(), null);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldReturnShiftProjectionCashesWhenNoShiftProjectionCashesInList()
		{
			var shiftProjectionCashes = new List<ShiftProjectionCache>();
			var result = _target.Filter(new List<ShiftProjectionCache>(), shiftProjectionCashes);
			Assert.AreEqual(shiftProjectionCashes, result);		
		}

		[Test]
		public void ShouldReturnShiftProjectionCashesWhenNoShiftProjectionCashesInNotAllowedList()
		{
			var shiftProjectionCashes = new List<ShiftProjectionCache>{_shiftProjectionCache1};

			var result = _target.Filter(new List<ShiftProjectionCache>(), shiftProjectionCashes);
			Assert.AreEqual(shiftProjectionCashes, result);
		}

		[Test]
		public void ShouldFilter()
		{
			var shiftProjectionCash1 = new shiftProjectionCasheForTestReturn1();
			var shiftProjectionCash2 = new shiftProjectionCasheForTestReturn2();

			var shiftProjectionCashes = new List<ShiftProjectionCache> { shiftProjectionCash1, shiftProjectionCash2 };
			var notAllowedShiftProjectionCashes = new List<ShiftProjectionCache> { shiftProjectionCash1 };

			var result = _target.Filter(notAllowedShiftProjectionCashes, shiftProjectionCashes);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(shiftProjectionCash2, result[0]);
				
		}

		private class shiftProjectionCasheForTestReturn1 : ShiftProjectionCache
		{
			public override int GetHashCode()
			{
				return 1;
			}

			public shiftProjectionCasheForTestReturn1() 
				: base(null, null)
			{
			}
		}

		private class shiftProjectionCasheForTestReturn2 : ShiftProjectionCache
		{
			public override int GetHashCode()
			{
				return 2;
			}

			public shiftProjectionCasheForTestReturn2() 
				: base(null, null)
			{
			}
		}
	}
}
