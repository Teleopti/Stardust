using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	public class PersonSkillHintTest : IIsolateSystem
	{
		public CheckScheduleHints Target;

		[Test]
		public void PersonWithSkillsShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2017, 01, 20), SkillFactory.CreateSkill("Juggling")));

			var result = Target.Execute(new ScheduleHintInput(new[] { person }, planningPeriod, 0)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSkillHint)));

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonPeriodWithoutSkillShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 20)).WithId();

			var result = Target.Execute(new ScheduleHintInput(new[] { person }, planningPeriod, 0)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSkillHint)));

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

			var result = Target.Execute(new ScheduleHintInput(new[] { person }, planningPeriod, 0)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSkillHint)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}