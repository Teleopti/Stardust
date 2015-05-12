﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner
{
	[TestFixture]
	public class VoilatedBusinessRulesTest
	{
		[Test]
		public void ShouldReturnMissingScheduleIfNoScheduleIsFound()
		{
			var target = new VoilatedSchedulePeriodBusinessRule();
			var persons = new List<IPerson>() { PersonFactory.CreatePerson("a")};
			var result = target.GetResult(persons, new DateOnlyPeriod(2015,05,08,2015,05,15));
			result.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnMissingScheduleIfSchedulePeriodIsOutsideRange()
		{
			var target = new VoilatedSchedulePeriodBusinessRule();
			var person = PersonFactory.CreatePerson("a");
			var persons = new List<IPerson>()
			{
				PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2015, 05, 06))
			};
			var result = target.GetResult(persons, new DateOnlyPeriod(2015, 05, 08, 2015, 05, 15));
			result.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotReturnMissingScheduleIfSchedulePeriodIsInRange()
		{
			var target = new VoilatedSchedulePeriodBusinessRule();
			var person = PersonFactory.CreatePerson("a");
			person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2015, 05, 08));
			var persons = new List<IPerson> {person};
			var result = target.GetResult(persons, new DateOnlyPeriod(2015, 05, 08, 2015, 05, 15));
			result.ToList().Count.Should().Be.EqualTo(0);
		}
	}
}
