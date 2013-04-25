using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class PersonScheduleCommandControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
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

			var command = new RemoveAbsenceCommand();

			target.RemoveAbsence(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}
	}
}