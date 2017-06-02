using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	public class PersonSkillValidatorTest
	{
		public BasicSchedulingValidator Target;

		[Test]
		public void PersonWithSkillsShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2017, 01, 20), SkillFactory.CreateSkill("Juggling")));

			var result = Target.Validate(new[] { person }, planningPeriod).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSkillValidator)));

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonPeriodWithoutSkillShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 20)).WithId();

			var result = Target.Validate(new[] { person }, planningPeriod).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSkillValidator)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		[Test]
		public void PersonWithMultiplePeriodsCoveringEntirePlanningPeriodShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 01, 20)));
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2017, 01, 25), SkillFactory.CreateSkill("Juggling")));

			var result = Target.Validate(new[] { person }, planningPeriod).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSkillValidator)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Not.Be.Null().And.Not.Be.Empty();
		}
	}
}