﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	[TestFixture]
	internal class TeamScheduleControllerTest
	{
		[Test]
		public void ShouldGetFullDayAbsencePermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence))
				.Return(expectedResult);

			var target = new TeamScheduleController(null, null, principalAuthorization, null, null, null, null, null);
			var result = target.GetPermissions();

			result.Content.IsAddFullDayAbsenceAvailable.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetIntradayAbsencePermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence))
				.Return(expectedResult);

			var target = new TeamScheduleController(null, null, principalAuthorization, null, null, null, null, null);
			var result = target.GetPermissions();

			result.Content.IsAddIntradayAbsenceAvailable.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetModifyPersonAbsencePermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(expectedResult);

			var target = new TeamScheduleController(null, null, principalAuthorization, null, null, null, null, null);
			var result = target.GetPermissions();

			result.Content.HasModifyPersonAbsencePermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetSwapShiftsPermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.SwapShifts))
				.Return(expectedResult);

			var target = new TeamScheduleController(null, null, principalAuthorization, null, null, null, null, null);
			var result = target.GetPermissions();

			result.Content.IsSwapShiftsAvailable.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetAddingActivityPermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddActivity))
				.Return(expectedResult);

			var target = new TeamScheduleController(null, null, principalAuthorization, null, null, null, null, null);
			var result = target.GetPermissions();

			result.Content.HasAddingActivityPermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetModifyPersonAssignmentPermission()
		{
			const bool expectedResult = true;
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
				.Return(expectedResult);

			var target = new TeamScheduleController(null, null, principalAuthorization, null, null, null, null, null);
			var result = target.GetPermissions();

			result.Content.HasModifyAssignmentPermission.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldAssignOperatePersonForAddFullDayAbsence()
		{
			var expectedPerson = new Person();
			expectedPerson.SetId(Guid.NewGuid());
			var loggonUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggonUser.Stub(x => x.CurrentUser()).Return(expectedPerson);
			var pricipalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			pricipalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);

			var target = new TeamScheduleController(null, loggonUser, pricipalAuthorization, null, null, null, null, null);

			var form = new FullDayAbsenceForm {PersonIds = new List<Guid>(), TrackedCommandInfo = new TrackedCommandInfo()};
			target.AddFullDayAbsence(form);

			form.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(expectedPerson.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldAddFullDayAbsenceForMoreThanOneAgent()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var pricipalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			pricipalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);
			var target = new TeamScheduleController(null, null, pricipalAuthorization, absencePersister, null, null, null, null);

			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var form = new FullDayAbsenceForm
			{
				PersonIds = new List<Guid> {person1, person2}
			};
			target.AddFullDayAbsence(form);

			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[0])));
			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[1])));
		}

		[Test]
		public void ShouldAddFullDayAbsenceThroughInputForm()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var pricipalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			pricipalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);
			var target = new TeamScheduleController(null, null, pricipalAuthorization, absencePersister, null, null, null, null);

			var form = new FullDayAbsenceForm
			{
				PersonIds = new List<Guid> {Guid.NewGuid()},
				AbsenceId = Guid.NewGuid(),
				StartDate = DateTime.MinValue,
				EndDate = DateTime.MaxValue,
			};
			target.AddFullDayAbsence(form);

			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.AbsenceId == form.AbsenceId)));
			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.StartDate == form.StartDate)));
			absencePersister.AssertWasCalled(
				x => x.PersistFullDayAbsence(Arg<AddFullDayAbsenceCommand>.Matches(s => s.EndDate == form.EndDate)));
		}

		[Test]
		public void ShouldAssignOperatePersonForAddIntradayAbsence()
		{
			var expectedPerson = new Person();
			expectedPerson.SetId(Guid.NewGuid());
			var loggonUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggonUser.Stub(x => x.CurrentUser()).Return(expectedPerson);
			var pricipalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			pricipalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);

			var target = new TeamScheduleController(null, loggonUser, pricipalAuthorization, null, null, null, null, null);

			var form = new IntradayAbsenceForm
			{
				PersonIds = new List<Guid>(),
				TrackedCommandInfo = new TrackedCommandInfo(),
				StartTime = DateTime.MinValue,
				EndTime = DateTime.MaxValue
			};
			target.AddIntradayAbsence(form);

			form.TrackedCommandInfo.OperatedPersonId.Should().Be.EqualTo(expectedPerson.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldAddIntradayAbsenceForMoreThanOneAgent()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var pricipalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			pricipalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);
			var target = new TeamScheduleController(null, null, pricipalAuthorization, absencePersister, null, null, null, null);

			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var form = new IntradayAbsenceForm
			{
				StartTime = DateTime.MinValue,
				EndTime = DateTime.MaxValue,
				PersonIds = new List<Guid> {person1, person2}
			};
			target.AddIntradayAbsence(form);

			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[0])));
			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.PersonId == form.PersonIds.ToList()[1])));
		}

		[Test]
		public void ShouldAddIntradayAbsenceThroughInputForm()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var pricipalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			pricipalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);
			var target = new TeamScheduleController(null, null, pricipalAuthorization, absencePersister, null, null, null, null);

			var form = new IntradayAbsenceForm
			{
				PersonIds = new List<Guid> {Guid.NewGuid()},
				AbsenceId = Guid.NewGuid(),
				StartTime = DateTime.MinValue,
				EndTime = DateTime.MaxValue,
			};
			target.AddIntradayAbsence(form);

			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.AbsenceId == form.AbsenceId)));
			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.StartTime == form.StartTime)));
			absencePersister.AssertWasCalled(
				x => x.PersistIntradayAbsence(Arg<AddIntradayAbsenceCommand>.Matches(s => s.EndTime == form.EndTime)));
		}

		[Test]
		public void ShouldReturnBadRequestWhenEndTimeEarlierThanStartTime()
		{
			var absencePersister = MockRepository.GenerateMock<IAbsencePersister>();
			var target = new TeamScheduleController(null, null, null, absencePersister, null, null, null, null);

			var form = new IntradayAbsenceForm
			{
				StartTime = DateTime.MaxValue,
				EndTime = DateTime.MinValue,
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
			var target = new TeamScheduleController(null, null, null, null, agentsPerPageSettingersisterAndProvider, null, null,
				null);

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
			var target = new TeamScheduleController(null, loggonUser, null, null, agentsPerPageSettingersisterAndProvider, null,
				null, null);

			var result = target.GetAgentsPerPageSetting();

			result.Content.Agents.Should().Be.EqualTo(expectedAgents);
		}

		[Test]
		public void ShouldSwapShifts()
		{
			var loggedOnUser = new FakeLoggedOnUser();
			var swapShiftHandler = MockRepository.GenerateMock<ISwapMainShiftForTwoPersonsCommandHandler>();
			var target = new TeamScheduleController(null, loggedOnUser, null, null, null, swapShiftHandler, null, null);

			var command = new SwapMainShiftForTwoPersonsCommand
			{
				PersonIdFrom = Guid.NewGuid(),
				PersonIdTo = Guid.NewGuid(),
				ScheduleDate = new DateTime(2016, 01, 01)
			};

			target.SwapShifts(command);

			swapShiftHandler.AssertWasCalled(x => x.SwapShifts(Arg<SwapMainShiftForTwoPersonsCommand>.Matches(a => a == command)));
		}

		[Test]
		public void ShouldRemovePersonAbsences()
		{
			var scheduleDate = new DateTime(DateTime.Now.Date.Ticks, DateTimeKind.Unspecified);
			var personId = Guid.NewGuid();
			var personAbsenceId = Guid.NewGuid();
			var personAbsenceIdStandalone = Guid.NewGuid();
			var removePersonAbsenceCommandHandler = MockRepository.GenerateMock<IHandleCommand<RemovePersonAbsenceCommand>>();

			var target = setupForRemoveAbsence(scheduleDate, personId, personAbsenceId, removePersonAbsenceCommandHandler, null);

			var form = new RemovePersonAbsenceForm
			{
				ScheduleDate = scheduleDate,
				PersonIds = new List<Guid> {personId},
				RemoveEntireCrossDayAbsence = true,
				PersonAbsenceIds = new List<Guid> {personAbsenceIdStandalone}
			};
			target.RemoveAbsence(form);

			removePersonAbsenceCommandHandler.AssertWasCalled(
				x => x.Handle(Arg<RemovePersonAbsenceCommand>.Matches(
					s => s.PersonAbsenceIds.Contains(personAbsenceIdStandalone))));
			removePersonAbsenceCommandHandler.AssertWasCalled(
				x => x.Handle(Arg<RemovePersonAbsenceCommand>.Matches(
					s => s.PersonAbsenceIds.Contains(personAbsenceId))));
		}

		[Test]
		public void ShouldRemovePartOfPersonAbsences()
		{
			var scheduleDate = new DateTime(DateTime.Now.Date.Ticks, DateTimeKind.Unspecified);
			var personId = Guid.NewGuid();
			var personAbsenceId = Guid.NewGuid();
			var personAbsenceIdStandalone = Guid.NewGuid();
			var removePartPersonAbsenceCommandHandler =
				MockRepository.GenerateMock<IHandleCommand<RemovePartPersonAbsenceCommand>>();
			var target = setupForRemoveAbsence(scheduleDate, personId, personAbsenceId, null,
				removePartPersonAbsenceCommandHandler);

			var form = new RemovePersonAbsenceForm
			{
				ScheduleDate = scheduleDate,
				PersonIds = new List<Guid> {personId},
				RemoveEntireCrossDayAbsence = false,
				PersonAbsenceIds = new List<Guid> { personAbsenceIdStandalone }
			};
			target.RemoveAbsence(form);

			removePartPersonAbsenceCommandHandler.AssertWasCalled(
				x => x.Handle(Arg<RemovePartPersonAbsenceCommand>.Matches(
					s => s.PersonAbsenceIds.Contains(personAbsenceIdStandalone)
						 && s.PeriodToRemove.StartDateTime == scheduleDate.Date &&
						 s.PeriodToRemove.EndDateTime == scheduleDate.Date.AddDays(1))));
			removePartPersonAbsenceCommandHandler.AssertWasCalled(
				x => x.Handle(Arg<RemovePartPersonAbsenceCommand>.Matches(
					s => s.PersonAbsenceIds.Contains(personAbsenceId)
						 && s.PeriodToRemove.StartDateTime == scheduleDate.Date &&
						 s.PeriodToRemove.EndDateTime == scheduleDate.Date.AddDays(1))));
		}

		private static TeamScheduleController setupForRemoveAbsence(DateTime scheduleDate, Guid personId, Guid personAbsenceId,
			IHandleCommand<RemovePersonAbsenceCommand> removePersonAbsenceCommandHandler,
			IHandleCommand<RemovePartPersonAbsenceCommand> removePartPersonAbsenceCommandHandler)
		{
			var pricipalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			pricipalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				.Return(true);

			var teamScheduleViewModelFactory = MockRepository.GenerateMock<ITeamScheduleViewModelFactory>();
			var schedule = new GroupScheduleViewModel
			{
				Schedules =
					new List<GroupScheduleShiftViewModel>
					{
						new GroupScheduleShiftViewModel
						{
							Projection = new List<GroupScheduleProjectionViewModel>
							{
								new GroupScheduleProjectionViewModel
								{
									ParentPersonAbsence = personAbsenceId
								}
							}
						}
					}
			};
			teamScheduleViewModelFactory.Stub(
				x => x.CreateViewModelForPeople(new[] {personId}, new DateOnly(scheduleDate)))
				.Return(schedule);


			var expectedPerson = PersonFactory.CreatePerson();
			expectedPerson.SetId(Guid.NewGuid());
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(expectedPerson);

			return new TeamScheduleController(teamScheduleViewModelFactory, loggedOnUser, pricipalAuthorization, null, null,
				null, removePersonAbsenceCommandHandler, removePartPersonAbsenceCommandHandler);
		}
	}
}
