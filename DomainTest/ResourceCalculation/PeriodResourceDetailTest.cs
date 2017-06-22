using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class PeriodResourceDetailTest
	{
		

		 [Test]
		 public void ShouldNotAcceptValueValueLowerThanZero()
		 {
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var target = new PeriodResourceDetail(1, -1);
			});
		}

		
		 [Test]
		 public void CountShouldNotAcceptValueValueLowerThanZero()
		 {
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var target = new PeriodResourceDetail(-1, 1);
			});
			
		 }
	}
}