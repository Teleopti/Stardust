using System;
using System.Collections.Generic;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture]
	public class TeamScheduleControllerTest
	{
		private IAuthorization principalAuthorization;
		private TeamScheduleController target;
		[SetUp]
		public void Setup()
		{
			principalAuthorization = MockRepository.GenerateMock<IAuthorization>();
			target = new TeamScheduleController(null, null,  principalAuthorization, null, null, null);
		}
		[Test]
		public void ShouldGetFullDayAbsencePermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.IsAddFullDayAbsenceAvailable.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetIntradayAbsencePermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.IsAddIntradayAbsenceAvailable.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetSwapShiftsPermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.SwapShifts))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.IsSwapShiftsAvailable.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetAddingActivityPermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddActivity))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.HasAddingActivityPermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetAddingPersonalActivityPermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddPersonalActivity))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.HasAddingPersonalActivityPermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetAddingOvertimeActivityPermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddOvertimeActivity))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.HasAddingOvertimeActivityPermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetRemoveActivityPermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveActivity))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.HasRemoveActivityPermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetMoveActivityPermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveActivity))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.HasMoveActivityPermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetMoveInvalidOverlappedActivityPermission()
		{
			const bool expectedResult = true;
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity))
				.Return(expectedResult);

			var result = target.GetPermissions();

			result.Content.HasMoveInvalidOverlappedActivityPermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldAssignOperatePersonForAddFullDayAbsence()
		{
			var expectedPerson = new Person();
			expectedPerson.SetId(Guid.NewGuid());
			var loggonUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggonUser.Stub(x => x.CurrentUser()).Return(expectedPerson);
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);

			target = new TeamScheduleController(null, loggonUser, principalAuthorization, null, null, null);

			var form = new FullDayAbsenceForm {PersonIds = new List<Guid>(), TrackedCommandInfo = new TrackedCommandInfo()};
			target.AddFullDayAbsence(form);

			form.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(expectedPerson.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldAddFullDayAbsenceForMoreThanOneAgent()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);
			target = new TeamScheduleController(null, null, principalAuthorization, absencePersister, null, null);

			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var form = new FullDayAbsenceForm
			{
				PersonIds = new List<Guid> {person1, person2}
			};
			target.AddFullDayAbsence(form);

			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.PersonId == person1)));
			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.PersonId == person2)));
		}

		[Test]
		public void ShouldAddFullDayAbsenceThroughInputForm()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);
			target = new TeamScheduleController(null, null, principalAuthorization, absencePersister, null, null);

			var form = new FullDayAbsenceForm
			{
				PersonIds = new List<Guid> {Guid.NewGuid()},
				AbsenceId = Guid.NewGuid(),
				Start = DateTime.MinValue,
				End = DateTime.MaxValue,
			};
			target.AddFullDayAbsence(form);

			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.AbsenceId == form.AbsenceId)));
			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.StartDate == form.Start)));
			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.EndDate == form.End)));
		}

		[Test]
		public void ShouldAssignOperatePersonForAddIntradayAbsence()
		{
			var expectedPerson = new Person();
			expectedPerson.SetId(Guid.NewGuid());
			var loggonUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggonUser.Stub(x => x.CurrentUser()).Return(expectedPerson);
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);

			target = new TeamScheduleController(null, loggonUser, principalAuthorization, null, null, null);

			var form = new IntradayAbsenceForm
			{
				PersonIds = new List<Guid>(),
				TrackedCommandInfo = new TrackedCommandInfo(),
				Start = DateTime.MinValue,
				End = DateTime.MaxValue
			};
			target.AddIntradayAbsence(form);

			form.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(expectedPerson.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldAddIntradayAbsenceForMoreThanOneAgent()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);
			target = new TeamScheduleController(null, null, principalAuthorization, absencePersister, null, null);

			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var form = new IntradayAbsenceForm
			{
				Start = DateTime.MinValue,
				End = DateTime.MaxValue,
				PersonIds = new List<Guid> {person1, person2}
			};
			target.AddIntradayAbsence(form);

			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.PersonId == person1)));
			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.PersonId == person2)));
		}

		[Test]
		public void ShouldAddIntradayAbsenceThroughInputForm()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);
			target = new TeamScheduleController(null, null,  principalAuthorization, absencePersister, null, null);

			var form = new IntradayAbsenceForm
			{
				PersonIds = new List<Guid> {Guid.NewGuid()},
				AbsenceId = Guid.NewGuid(),
				Start = DateTime.MinValue,
				End = DateTime.MaxValue,
			};
			target.AddIntradayAbsence(form);

			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.AbsenceId == form.AbsenceId)));
			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.StartTime == form.Start)));
			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.EndTime == form.End)));
		}

		[Test]
		public void ShouldReturnBadRequestWhenEndTimeEarlierThanStartTime()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			target = new TeamScheduleController(null, null, null, absencePersister, null, null);

			var form = new IntradayAbsenceForm
			{
				Start = DateTime.MaxValue,
				End = DateTime.MinValue,
			};
			var result = target.AddIntradayAbsence(form);

			result.Should().Be.OfType<BadRequestErrorMessageResult>();
			absencePersister.AssertWasNotCalled(x => x.PersistIntradayAbsence(null), y => y.IgnoreArguments());
		}

		[Test]
		public void ShouldUpdateAgentsPerPage()
		{
			const int expectedAgents = 30;
			var agentsPerPageSettingersisterAndProvider =
				MockRepository.GenerateMock<ISettingsPersisterAndProvider<AgentsPerPageSetting>>();
			target = new TeamScheduleController(null, null, null, null, agentsPerPageSettingersisterAndProvider, null);

			target.UpdateAgentsPerPageSetting(expectedAgents);

			agentsPerPageSettingersisterAndProvider.AssertWasCalled(
				x => x.Persist(Arg<AgentsPerPageSetting>.Matches(s => s.AgentsPerPage == expectedAgents)));
		}

		[Test]
		public void ShouldGetAgentsPerPage()
		{
			const int expectedAgents = 30;
			var loggonUser = new FakeLoggedOnUser();
			var agentsPerPageSettingersisterAndProvider =
				MockRepository.GenerateMock<ISettingsPersisterAndProvider<AgentsPerPageSetting>>();
			agentsPerPageSettingersisterAndProvider.Stub(x => x.GetByOwner(loggonUser.CurrentUser()))
				.Return(new AgentsPerPageSetting() {AgentsPerPage = expectedAgents});
			target = new TeamScheduleController(null, loggonUser, null, null,  agentsPerPageSettingersisterAndProvider, null);

			var result = target.GetAgentsPerPageSetting();

			result.Content.Agents.Should().Be.EqualTo(expectedAgents);
		}

		[Test]
		public void ShouldSwapShifts()
		{
			var loggedOnUser = new FakeLoggedOnUser();
			var swapShiftHandler = MockRepository.GenerateMock<ISwapMainShiftForTwoPersonsCommandHandler>();
			target = new TeamScheduleController(null, loggedOnUser, null, null,  null, swapShiftHandler);

			var command = new SwapMainShiftForTwoPersonsCommand
			{
				PersonIdFrom = Guid.NewGuid(),
				PersonIdTo = Guid.NewGuid(),
				ScheduleDate = new DateTime(2016, 01, 01)
			};

			target.SwapShifts(command);

			swapShiftHandler.AssertWasCalled(x => x.SwapShifts(Arg<SwapMainShiftForTwoPersonsCommand>.Matches(a => a == command)));
		}
	}
}
