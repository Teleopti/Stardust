﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	public class PersonShiftBagHintTest : ISetup
	{
		public CheckScheduleHints Target;

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

			var result = Target.Execute(new HintInput(null, new[] { person }, planningPeriod)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonShiftBagHint)));

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonPeriodWithoutSkillShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 20)).WithId();

			var result = Target.Execute(new HintInput(null, new[] { person }, planningPeriod)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonShiftBagHint)));

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
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2017, 01, 25));
			personPeriod.RuleSetBag = new RuleSetBag();
			person.AddPersonPeriod(personPeriod);

			var result = Target.Execute(new HintInput(null, new[] { person }, planningPeriod)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonShiftBagHint)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}