using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	public class PersonShiftBagValidatorTest
	{
		public IPersonShiftBagValidator Target;

		[Test]
		public void PersonWithSkillsShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 01, 20));
			personPeriod.RuleSetBag = new RuleSetBag();
			person.AddPersonPeriod(personPeriod);

			var result = Target.GetPeopleMissingShiftBag(new[] { person }, planningPeriod).ToList();

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonPeriodWithoutSkillShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 20)).WithId();

			var result = Target.GetPeopleMissingShiftBag(new[] { person }, planningPeriod).ToList();

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
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 01, 25));
			personPeriod.RuleSetBag = new RuleSetBag();
			person.AddPersonPeriod(personPeriod);

			var result = Target.GetPeopleMissingShiftBag(new[] { person }, planningPeriod).ToList();

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.PersonId.Should().Be.EqualTo(person.Id);
			validationError.PersonName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationError.Should().Not.Be.Null().And.Not.Be.Empty();
		}
	}
}