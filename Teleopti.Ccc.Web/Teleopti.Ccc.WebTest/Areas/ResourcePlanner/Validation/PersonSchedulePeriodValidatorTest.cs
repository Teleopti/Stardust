﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner.Validation
{
	[ResourcePlannerTest]
	public class PersonSchedulePeriodValidatorTest
	{
		public IPersonSchedulePeriodValidator Target;

		[Test]
		public void NoSchedulePeriodShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();

			var result = Target.GetPeopleMissingSchedulePeriod(new[] {person}, planningPeriod).ToList();

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.PersonId.Should().Be.EqualTo(person.Id);
			validationError.PersonName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationError.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		[Test]
		public void SchedulePeriodExactlyMatchesShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Week, 1));

			var result = Target.GetPeopleMissingSchedulePeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Be.Empty();
		}

		[Test]
		public void TwoSchedulePeriodsExactlyMatchShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 02, 05);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Week, 1));

			var result = Target.GetPeopleMissingSchedulePeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Be.Empty();
		}

		[Test]
		public void SchedulePeriodsEndsAfterPlanningPeriodShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Week, 2));

			var result = Target.GetPeopleMissingSchedulePeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.PersonId.Should().Be.EqualTo(person.Id);
			validationError.PersonName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationError.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		[Test]
		public void SchedulePeriodsStartsBeforePlanningPeriodShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddSchedulePeriod(new SchedulePeriod(new DateOnly(2017, 01, 16), SchedulePeriodType.Week, 2));

			var result = Target.GetPeopleMissingSchedulePeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.PersonId.Should().Be.EqualTo(person.Id);
			validationError.PersonName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationError.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		[Test]
		public void TwoDifferentSchedulePeriodsThatCombinedMatchPlanningPeriodShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 02, 12);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var schedulePeriod = new SchedulePeriod(new DateOnly(2017, 01, 23), SchedulePeriodType.Week, 2);
			person.AddSchedulePeriod(schedulePeriod);
			person.AddSchedulePeriod(new SchedulePeriod(new DateOnly(2017, 02, 06), SchedulePeriodType.Week, 1));

			var result = Target.GetPeopleMissingSchedulePeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Be.Empty();
		}
	}
}