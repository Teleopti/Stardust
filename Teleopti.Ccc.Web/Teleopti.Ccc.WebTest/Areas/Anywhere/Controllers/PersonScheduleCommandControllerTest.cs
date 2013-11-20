using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class PersonScheduleCommandControllerTest
	{
		[Test]
		public void ShouldDispatchAddFullDayAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher);

			var command = new AddFullDayAbsenceCommand();

			target.AddFullDayAbsence(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldDispatchRemoveAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher);

			var command = new RemovePersonAbsenceCommand();

			target.RemovePersonAbsence(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldDispatchAddActivity()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher);

			var command = new AssignActivityCommand();

			target.AssignActivity(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

	}

}