using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class ScheduleDayDataTest
	{
		private IScheduleDayData _target;

		[SetUp]
		public void Setup()
		{
			_target = new ScheduleDayData(DateOnly.MinValue);
		}

		[Test]
		public void DefaultProperties()
		{
			Assert.AreEqual(DateOnly.MinValue, _target.Date);
			Assert.IsFalse(_target.IsContractDayOff);
			Assert.IsFalse(_target.IsDayOff);
			Assert.IsFalse(_target.IsScheduled);
			Assert.IsFalse(_target.HaveRestriction);
		}

		[Test]
		public void PropertiesShouldStick()
		{
			_target.IsContractDayOff = true;
			_target.IsDayOff = true;
			_target.IsScheduled = true;
			_target.HaveRestriction = true;

			Assert.IsTrue(_target.IsContractDayOff);
			Assert.IsTrue(_target.IsDayOff);
			Assert.IsTrue(_target.IsScheduled);
			Assert.IsTrue(_target.HaveRestriction);
		}
	}
}