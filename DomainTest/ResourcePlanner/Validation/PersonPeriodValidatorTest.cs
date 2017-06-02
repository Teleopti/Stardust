using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	public class PersonPeriodValidatorTest
	{
		public PersonPeriodValidator Target;

		[Test]
		public void PersonWithoutPersonPeriodsShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();

			var result = Target.GetPeopleMissingPeriod(new[] {person}, planningPeriod).ToList();

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.PersonId.Should().Be.EqualTo(person.Id);
			validationError.PersonName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationError.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		[Test]
		public void PersonWithPersonPeriodsShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 20)).WithId();

			var result = Target.GetPeopleMissingPeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonWithPersonPeriodStartingAfterPlanningPeriodShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 31)).WithId();

			var result = Target.GetPeopleMissingPeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.PersonId.Should().Be.EqualTo(person.Id);
			validationError.PersonName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationError.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		[Test]
		public void PersonWithPersonPeriodStartingWithinPlanningPeriodShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 25)).WithId();

			var result = Target.GetPeopleMissingPeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.PersonId.Should().Be.EqualTo(person.Id);
			validationError.PersonName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationError.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		[Test]
		public void PersonWithMultiplePeriodsCoveringEntirePlanningPeriodShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 01, 20)));
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 01, 25)));

			var result = Target.GetPeopleMissingPeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonWithPersonPeriodAndLeavingDateWithinPlanningPeriodShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 20)).WithId();
			person.TerminatePerson(new DateOnly(2017, 01, 25), new PersonAccountUpdaterDummy());

			var result = Target.GetPeopleMissingPeriod(new[] { person }, planningPeriod).ToList();

			result.Should().Be.Empty();
		}
	}
}