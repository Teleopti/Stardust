using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	public class ViolatedBusinessRulesTest
	{
		[Test]
		public void ShouldReturnMissingScheduleIfNoScheduleIsFound()
		{
			var persons = new List<IPerson> { PersonFactory.CreatePerson("a")};
			var result = new ViolatedSchedulePeriodBusinessRule().GetResult(persons, new DateOnlyPeriod(2015, 05, 08, 2015, 05, 15));
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
			var result = new ViolatedSchedulePeriodBusinessRule().GetResult(persons, new DateOnlyPeriod(2015, 05, 08, 2015, 05, 15));
			result.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnMissingScheduleIfSchedulePeriodIsInRange()
		{
			var person = PersonFactory.CreatePerson("a");
			person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2015, 05, 08));
			var persons = new List<IPerson> {person};
			var result = new ViolatedSchedulePeriodBusinessRule().GetResult(persons, new DateOnlyPeriod(2015, 05, 08, 2015, 05, 15));
			result.ToList().Count.Should().Be.EqualTo(0);
		}
	}
}
