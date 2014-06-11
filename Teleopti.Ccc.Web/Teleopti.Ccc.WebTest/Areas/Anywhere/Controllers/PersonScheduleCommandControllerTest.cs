using System;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
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
		public void ShouldDispatchAddIntradayAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher);

			var command = new AddIntradayAbsenceCommand();

			target.AddIntradayAbsence(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldNotDispatchInvalidAddIntradayAbsenceCommand()
		{
			var response = MockRepository.GenerateStub<FakeHttpResponse>();
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher);
			var context = new FakeHttpContext("/");
			context.SetResponse(response);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			var command = new AddIntradayAbsenceCommand
				{
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
					EndTime = new DateTime(2013, 11, 27, 13, 00, 00, DateTimeKind.Utc)
				};

			target.AddIntradayAbsence(command);

			commandDispatcher.AssertWasNotCalled(x => x.Execute(command));
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

			var command = new AddActivityCommand();

			target.AddActivity(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldDispatchMoveActivity()
		{
			var dispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(dispatcher);
			var command = new MoveActivityCommand();
			target.MoveActivity(command);
			dispatcher.AssertWasCalled(x=>x.Execute(command));
		}
	}

}