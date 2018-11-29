using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[TestFixture]
	public class UnderstaffingDetailsTest
	{
		private UnderstaffingDetails target;

		[SetUp]
		public void Setup()
		{
			target = new UnderstaffingDetails();
			target.AddUnderstaffingDay(new DateOnly(2012, 1, 8));
			target.AddUnderstaffingDay(new DateOnly(2012, 1, 9));
		}

		[Test]
		public void ShouldHaveUnderstaffingDaysSetTwice()
		{
			target.UnderstaffingDays.Count().Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldNotHaveDuplicateUnderstaffingDay()
		{
			var testDate = new DateOnly(2012, 12, 31);
			target.AddUnderstaffingDay(testDate);
			target.AddUnderstaffingDay(testDate);
			target.UnderstaffingDays.Count(x => x == testDate).Should().Be.EqualTo(1);
		}

	}
}
