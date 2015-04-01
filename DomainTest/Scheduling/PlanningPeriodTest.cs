using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class PlanningPeriodTest
	{
		[Test, SetCulture("sv-SE")]
		public void ShouldGetUpcommingMonthAsDefaultPlanningPeriod()
		{
			var target = new PlanningPeriod(new TestableNow(new DateTime(2015,4,1)));
			
			target.Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31));
		}
	}
}
