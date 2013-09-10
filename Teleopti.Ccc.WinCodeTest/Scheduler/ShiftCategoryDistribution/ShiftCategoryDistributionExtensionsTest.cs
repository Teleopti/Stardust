using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftCategoryDistributionExtensionsTest
	{
		private IList<IShiftCategory> _list;
		private IShiftCategory _shiftCategory1;
		private IShiftCategory _shiftCategory2;

		[SetUp]
		public void Sertup()
		{
			_shiftCategory1 = new ShiftCategory("1");
			_shiftCategory2 = new ShiftCategory("2");

			_list = new List<IShiftCategory> { _shiftCategory1, _shiftCategory2};	
		}

		[Test]
		public void ShouldOrderWithDirection()
		{
			var sorted = _list.OrderByWithDirection(s => s.Description.Name, false).ToList();
			Assert.AreEqual(sorted[0].Description.Name, _shiftCategory1.Description.Name);
			Assert.AreEqual(sorted[1].Description.Name, _shiftCategory2.Description.Name);

			sorted = _list.OrderByWithDirection(s => s.Description.Name, true).ToList();
			Assert.AreEqual(sorted[0].Description.Name, _shiftCategory2.Description.Name);
			Assert.AreEqual(sorted[1].Description.Name, _shiftCategory1.Description.Name);
		}
	}
}
