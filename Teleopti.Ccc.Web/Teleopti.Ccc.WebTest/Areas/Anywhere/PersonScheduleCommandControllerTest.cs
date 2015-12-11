using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.WebTest.TestHelper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PersonScheduleCommandControllerTest
	{
		[Test]
		public void ShouldDispatchAddFullDayAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher, MockRepository.GenerateMock<ILoggedOnUser>(), null);

			var command = new AddFullDayAbsenceCommand();

			target.AddFullDayAbsence(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldTrackAddFullDayAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var personWithId = PersonFactory.CreatePersonWithId();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(personWithId);
			var target = new PersonScheduleCommandController(commandDispatcher, loggedOnUser, null);

			var command = new AddFullDayAbsenceCommand
			{
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};

			target.AddFullDayAbsence(command);

			var arguments = commandDispatcher.GetArgumentsForCallsMadeOn(x => x.Execute(null), a => a.IgnoreArguments());
			var firstCall = arguments.Single();
			var calledCommand = (AddFullDayAbsenceCommand)firstCall.Single();
			calledCommand.TrackedCommandInfo.OperatedPersonId.Should().Be(personWithId.Id);
		}

		[Test]
		public void ShouldDispatchAddIntradayAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher, MockRepository.GenerateMock<ILoggedOnUser>(), null);

			var command = new AddIntradayAbsenceCommand();

			target.AddIntradayAbsence(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldTrackAddIntradayAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var personWithId = PersonFactory.CreatePersonWithId();
			var target = new PersonScheduleCommandController(commandDispatcher, new FakeLoggedOnUser(personWithId), null);

			var command = new AddIntradayAbsenceCommand
			{
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};

			target.AddIntradayAbsence(command);

			var arguments=commandDispatcher.GetArgumentsForCallsMadeOn(x => x.Execute(null), a => a.IgnoreArguments());
			var firstCall = arguments.Single();
			var calledCommand = (AddIntradayAbsenceCommand)firstCall.Single();
			calledCommand.TrackedCommandInfo.OperatedPersonId.Should().Be(personWithId.Id);
		}

		[Test]
		public void ShouldNotDispatchInvalidAddIntradayAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher, MockRepository.GenerateMock<ILoggedOnUser>(), null);
			
			var command = new AddIntradayAbsenceCommand
				{
					StartTime = new DateTime(2013, 11, 27, 14, 00, 00, DateTimeKind.Utc),
					EndTime = new DateTime(2013, 11, 27, 13, 00, 00, DateTimeKind.Utc)
				};

			var result = (BadRequestErrorMessageResult)target.AddIntradayAbsence(command);
			result.Message.Should().Be.EqualTo(UserTexts.Resources.InvalidEndTime);

			commandDispatcher.AssertWasNotCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldDispatchRemoveAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher, MockRepository.GenerateMock<ILoggedOnUser>(), null);

			var command = new RemovePersonAbsenceCommand();

			target.RemovePersonAbsence(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldTrackRemoveAbsenceCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var personWithId = PersonFactory.CreatePersonWithId();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(personWithId);
			var target = new PersonScheduleCommandController(commandDispatcher, loggedOnUser, null);

			var command = new RemovePersonAbsenceCommand
			{
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};

			target.RemovePersonAbsence(command);

			var arguments = commandDispatcher.GetArgumentsForCallsMadeOn(x => x.Execute(null), a => a.IgnoreArguments());
			var firstCall = arguments.Single();
			var calledCommand = (RemovePersonAbsenceCommand)firstCall.Single();
			calledCommand.TrackedCommandInfo.OperatedPersonId.Should().Be(personWithId.Id);
		}

		[Test]
		public void ShouldDispatchAddActivity()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(commandDispatcher, MockRepository.GenerateMock<ILoggedOnUser>(), null);

			var command = new AddActivityCommand();

			target.AddActivity(command);

			commandDispatcher.AssertWasCalled(x => x.Execute(command));
		}

		[Test]
		public void ShouldTrackAddActivityCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var personWithId = PersonFactory.CreatePersonWithId();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(personWithId);
			var target = new PersonScheduleCommandController(commandDispatcher, loggedOnUser, null);

			var command = new AddActivityCommand
			{
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};

			target.AddActivity(command);

			var arguments = commandDispatcher.GetArgumentsForCallsMadeOn(x => x.Execute(null), a => a.IgnoreArguments());
			var firstCall = arguments.Single();
			var calledCommand = (AddActivityCommand)firstCall.Single();
			calledCommand.TrackedCommandInfo.OperatedPersonId.Should().Be(personWithId.Id);
		}

		[Test]
		public void ShouldDispatchMoveActivity()
		{
			var dispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var target = new PersonScheduleCommandController(dispatcher, MockRepository.GenerateMock<ILoggedOnUser>(), null);
			var command = new MoveActivityCommand();
			target.MoveActivity(command);
			dispatcher.AssertWasCalled(x=>x.Execute(command));
		}

		[Test]
		public void ShouldTrackMoveActivityCommand()
		{
			var commandDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var personWithId = PersonFactory.CreatePersonWithId();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(personWithId);
			var target = new PersonScheduleCommandController(commandDispatcher, loggedOnUser, null);

			var command = new MoveActivityCommand
			{
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};

			target.MoveActivity(command);

			var arguments = commandDispatcher.GetArgumentsForCallsMadeOn(x => x.Execute(null), a => a.IgnoreArguments());
			var firstCall = arguments.Single();
			var calledCommand = (MoveActivityCommand)firstCall.Single();
			calledCommand.TrackedCommandInfo.OperatedPersonId.Should().Be(personWithId.Id);
		}

		[Test]
		public void ShouldGetPersonScheduleDayViewModel()
		{
			var id = Guid.Empty;
			var date = new DateTime();
			var model = new PersonScheduleDayViewModel();
			var personScheduleDayViewModelFactory = MockRepository.GenerateMock<IPersonScheduleDayViewModelFactory>();
			personScheduleDayViewModelFactory.Stub(x => x.CreateViewModel(id, date)).Return(model);

			var target = new PersonScheduleCommandController(null, null, personScheduleDayViewModelFactory);

			var result = target.GetPersonSchedule(id, date);
			result.Result<PersonScheduleDayViewModel>().Should().Be.SameInstanceAs(model);
		}
	}

}