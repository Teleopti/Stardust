using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class PeriodResourceDetailTest
	{
		 [Test]
		 public void ShouldRoundResourceToFiveDecimalsToAvoidLeftOversWhenRemovingResource_Bug26851()
		 {
			 var target = new PeriodResourceDetail(1, 0.000004);
			 Assert.AreEqual(0d, target.Resource);

			 target.Resource = 0.00001;
			 Assert.AreEqual(0.00001, target.Resource);

			 target.Resource = -0.000001;
			 Assert.AreEqual(0.00000, target.Resource);
		 }


		 [Test]
		 [ExpectedException(typeof(ArgumentOutOfRangeException))]
		 public void ShouldNotAcceptValueValueLowerThanZero()
		 {
			 var target = new PeriodResourceDetail(1, -1);
		 }

		 [Test]
		 public void ShouldRoundResourceToFiveDecimalsToAvoidLeftOversWhenRemovingCount()
		 {
			 var target = new PeriodResourceDetail(0.000004, 1);
			 Assert.AreEqual(0d, target.Count);

			 target.Count = 0.00001;
			 Assert.AreEqual(0.00001, target.Count);

			 target.Count = -0.000001;
			 Assert.AreEqual(0.00000, target.Count);
		 }


		 [Test]
		 [ExpectedException(typeof(ArgumentOutOfRangeException))]
		 public void CountShouldNotAcceptValueValueLowerThanZero()
		 {
			 var target = new PeriodResourceDetail(-1, 1);
		 }
	}
}