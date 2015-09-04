using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.ResourcePlanner;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[TestFixture]
	[ResourcePlannerTest]
	public class ViolatedBusinessRulesTest
	{
		public ViolatedSchedulePeriodBusinessRule Target;

		[Test]
		public void ShouldReturnMissingScheduleIfNoScheduleIsFound()
		{
			var persons = new List<IPerson> { PersonFactory.CreatePerson("a")};
			var result = Target.GetResult(persons, new DateOnlyPeriod(2015, 05, 08, 2015, 05, 15));
			result.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnMissingScheduleIfSchedulePeriodIsOutsideRange()
		{
			var person = PersonFactory.CreatePerson("a");
			var persons = new List<IPerson>
			{
				PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2015, 05, 06))
			};
			var result = Target.GetResult(persons, new DateOnlyPeriod(2015, 05, 08, 2015, 05, 15));
			result.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnMissingScheduleIfSchedulePeriodIsInRange()
		{
			var person = PersonFactory.CreatePerson("a");
			person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2015, 05, 08));
			var persons = new List<IPerson> {person};
			var result = Target.GetResult(persons, new DateOnlyPeriod(2015, 05, 08, 2015, 05, 15));
			result.ToList().Count.Should().Be.EqualTo(0);
		}
	}
}
