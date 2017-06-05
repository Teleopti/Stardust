using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	public class PersonSchedulePeriodValidatorTest : ISetup
	{
		public SchedulingValidator Target;

		[Test]
		public void NoSchedulePeriodShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();

			var result = Target.Validate(new[] { person }, planningPeriod, true).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSchedulePeriodValidator)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Contain(Resources.MissingSchedulePeriodForPeriod);
		}

		[Test]
		public void SchedulePeriodExactlyMatchesShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddSchedulePeriod(new SchedulePeriod(startDate, SchedulePeriodType.Week, 1));

			var result = Target.Validate(new[] { person }, planningPeriod, true).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSchedulePeriodValidator)));

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

			var result = Target.Validate(new[] { person }, planningPeriod, true).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSchedulePeriodValidator)));

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

			var result = Target.Validate(new[] { person }, planningPeriod, true).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSchedulePeriodValidator)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Contain(Resources.NoMatchingSchedulePeriod);
		}

		[Test]
		public void SchedulePeriodsStartsBeforePlanningPeriodShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddSchedulePeriod(new SchedulePeriod(new DateOnly(2017, 01, 16), SchedulePeriodType.Week, 2));

			var result = Target.Validate(new[] { person }, planningPeriod, true).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSchedulePeriodValidator)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Contain(Resources.NoMatchingSchedulePeriod);
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

			var result = Target.Validate(new[] { person }, planningPeriod, true).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSchedulePeriodValidator)));

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonWithTerminationDateInPeriodShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 02, 12);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var schedulePeriod = new SchedulePeriod(new DateOnly(2017, 01, 23), SchedulePeriodType.Week, 1);
			person.AddSchedulePeriod(schedulePeriod);
			person.TerminatePerson(new DateOnly(2017, 02, 05), new PersonAccountUpdaterDummy());

			var result = Target.Validate(new[] { person }, planningPeriod, true).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSchedulePeriodValidator)));

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonWithNotAllSchedulePeriodsMatchingPlanningPeriodShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 02, 12);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var schedulePeriod = new SchedulePeriod(new DateOnly(2017, 01, 23), SchedulePeriodType.Week, 2);
			person.AddSchedulePeriod(schedulePeriod);

			var result = Target.Validate(new[] { person }, planningPeriod, true).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonSchedulePeriodValidator)));

			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Contain(Resources.NoMatchingSchedulePeriod);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}