using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	public class PersonPartTimePercentageHintTest : IIsolateSystem
	{
		public CheckScheduleHints Target;

		[Test]
		public void PersonHasPartTimePercentageShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(startDate));

			var result = Target.Execute(new ScheduleHintInput(new[] { person }, planningPeriod, false)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonPartTimePercentageHint)));

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonHasDeletedPartTimePercentageShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate);
			((IDeleteTag)personPeriod.PersonContract.PartTimePercentage).SetDeleted();
			person.AddPersonPeriod(personPeriod);

			var result = Target.Execute(new ScheduleHintInput(new[] { person }, planningPeriod, false)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonPartTimePercentageHint)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			validationError.ValidationErrors.Should().Not.Be.Null().And.Not.Be.Empty();
		}

		[Test]
		public void ShouldNotReturnHintsForHourlyEmployees()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate);
			personPeriod.PersonContract = new PersonContract(new Contract("_") {EmploymentType = EmploymentType.HourlyStaff},
				new PartTimePercentage("_"), new ContractSchedule("_"));
			((IDeleteTag)personPeriod.PersonContract.PartTimePercentage).SetDeleted();
			person.AddPersonPeriod(personPeriod);

			var result = Target.Execute(new ScheduleHintInput(new[] { person }, planningPeriod, false)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonPartTimePercentageHint)));

			result.Should().Be.Empty();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}