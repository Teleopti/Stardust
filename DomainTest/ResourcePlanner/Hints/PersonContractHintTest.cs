using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	public class PersonContractHintTest : IIsolateSystem
	{
		public CheckScheduleHints Target;

		[Test]
		public void PersonHasContractShouldNotReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(startDate));

			var result = Target.Execute(new ScheduleHintInput(new[] { person }, planningPeriod, false)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonContractHint)));

			result.Should().Be.Empty();
		}

		[Test]
		public void PersonHasDeletedContractShouldReturnValidationError()
		{
			var startDate = new DateOnly(2017, 01, 23);
			var endDate = new DateOnly(2017, 01, 29);
			var planningPeriod = new DateOnlyPeriod(startDate, endDate);

			var person = PersonFactory.CreatePerson().WithId();
			var personContract = new PersonContract(new Contract("Contract"),new PartTimePercentage("_"),new ContractSchedule("_")  );
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate, personContract,new Team());
			((IDeleteTag)personPeriod.PersonContract.Contract).SetDeleted();
			person.AddPersonPeriod(personPeriod);

			var result = Target.Execute(new ScheduleHintInput(new[] { person }, planningPeriod, false)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonContractHint)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.Single(x=>x.ErrorResource==nameof(Resources.DeletedContractAssigned)), UserTimeZone.Make())
				.Should()
				.Be.EqualTo(string.Format(Resources.DeletedContractAssigned, personPeriod.PersonContract.Contract.Description));
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}