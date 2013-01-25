using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
	[TestFixture]
	public class SkillIntervalDataTest
	{
		private ISkillIntervalData _target;
		private DateTimePeriod _dtp;

		[SetUp]
		public void Setup()
		{
			_dtp = new DateTimePeriod(2012, 11, 28, 2012, 11, 28);
			_target = new SkillIntervalData(_dtp, 3.5, 1.5, 3, 2, null);
		}

		[Test]
		public void ShouldContainCurrentDemand()
		{
			Assert.AreEqual(1.5, _target.CurrentDemand);
		}

		[Test]
		public void CouldContainMinMaxHeads()
		{
			Assert.IsNull(_target.MaximumHeads);
			Assert.AreEqual(2, _target.MinimumHeads.Value);
		}

		[Test]
		public void ShouldContainCurrentHeads()
		{
			Assert.AreEqual(3, _target.CurrentHeads);
		}

		[Test]
		public void ShouldContainPeriod()
		{
			Assert.AreEqual(_dtp, _target.Period);
		}

		[Test]
		public void ShouldContainForecastedDemand()
		{
			Assert.AreEqual(3.5, _target.ForecastedDemand);
		}
	}
}