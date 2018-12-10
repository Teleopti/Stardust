using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class ChangeShiftCategoryCommandHandlerTest
	{
		[Test]
		public void ShouldChangeShiftCategory()
		{
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { PersonFactory.CreatePersonWithId() };
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("sc").WithId();
			shiftCategoryRepository.Add(shiftCategory);
			var mainActivity = ActivityFactory.CreateActivity("Phone");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(personRepository.Single(),
				mainActivity, new DateTimePeriod(2013, 11, 13, 23, 2013, 11, 14, 8));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null) { pa };

			var target = new ChangeShiftCategoryCommandHandler(shiftCategoryRepository, personRepository,
				new ThisCurrentScenario(personAssignmentRepository.Single().Scenario), personAssignmentRepository);

			var command = new ChangeShiftCategoryCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 13),
				ShiftCategoryId = shiftCategory.Id.Value
			};
			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(0);
			pa.ShiftCategory.Id.Should().Be.EqualTo(shiftCategory.Id);
		}

		[Test]
		public void ShouldReportErrorWhenThereIsNoMainShift()
		{
			var personRepository = new FakeWriteSideRepository<IPerson>(null) { PersonFactory.CreatePersonWithId() };
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("sc").WithId();
			shiftCategoryRepository.Add(shiftCategory);
			
			var scenario = ScenarioFactory.CreateScenarioWithId("s", true);
			var pa = PersonAssignmentFactory.CreateEmptyAssignment(personRepository.Single(), scenario, new DateTimePeriod(2013, 11, 13, 12, 2013, 11, 13, 17));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository(null) { pa};			

			var target = new ChangeShiftCategoryCommandHandler(shiftCategoryRepository, personRepository,
				new ThisCurrentScenario(scenario), personAssignmentRepository);

			var command = new ChangeShiftCategoryCommand
			{
				PersonId = personRepository.Single().Id.Value,
				Date = new DateOnly(2013, 11, 13),
				ShiftCategoryId = shiftCategory.Id.Value
			};
			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(1);
			command.ErrorMessages.First().Should().Be.EqualTo(Resources.NoShiftsFound);
		}
	}
}
