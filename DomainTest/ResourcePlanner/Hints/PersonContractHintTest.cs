using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	public class PersonContractHintTest : ISetup
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

			var result = Target.Execute(new HintInput(null, new[] { person }, planningPeriod, null, false)).InvalidResources
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
			var personcontract = new PersonContract(new Contract("Contract"),new PartTimePercentage("_"),new ContractSchedule("_")  );
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate, personcontract,new Team());
			((IDeleteTag)personPeriod.PersonContract.Contract).SetDeleted();
			person.AddPersonPeriod(personPeriod);

			var result = Target.Execute(new HintInput(null, new[] { person }, planningPeriod, null, false)).InvalidResources
				.Where(x => x.ValidationTypes.Contains(typeof(PersonContractHint)));

			result.Should().Not.Be.Empty();
			var validationError = result.SingleOrDefault();
			validationError.ResourceId.Should().Be.EqualTo(person.Id);
			validationError.ResourceName.Should().Be.EqualTo(person.Name.ToString());
			HintsHelper.BuildErrorMessage(result.First().ValidationErrors.Single(x=>x.ErrorResource==nameof(Resources.DeletedContractAssigned)))
				.Should()
				.Be.EqualTo(string.Format(Resources.DeletedContractAssigned, personPeriod.PersonContract.Contract.Description));
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeScenarioRepository(ScenarioFactory.CreateScenario("_", true, true))).For<IScenarioRepository>();
		}
	}
}