using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core
{
	[TestFixture, TeamScheduleTest]
	public class TeamScheduleCommandHandlingProviderTest
	{
		public ITeamScheduleCommandHandlingProvider Target;
		public FakeActivityCommandHandler ActivityCommandHandler;
		public FakePersonRepository PersonRepository;
		public Global.FakePermissionProvider PermissionProvider;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeScenarioRepository CurrentScenario;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeShiftCategoryRepository ShiftCategoryRepository;


		[Test]
		public void ShouldNotFixOverwriteLayerWithoutPermission()
		{
			PermissionProvider.Enable();
			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			var person2 = PersonFactory.CreatePersonWithGuid("c", "d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016, 4, 16);

			var input = new MoveNonoverwritableLayersFormData
			{
				PersonIds = new[] { person1.Id.Value, person2.Id.Value },
				Date = date,
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.MoveNonoverwritableLayers(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldFixOverwriteLayerWithPermission()
		{
			PermissionProvider.Enable();
			var person1 = PersonFactory.CreatePersonWithGuid("a", "b");
			var person2 = PersonFactory.CreatePersonWithGuid("c", "d");
			PersonRepository.Has(person1);
			PersonRepository.Has(person2);

			var date = new DateOnly(2016, 4, 16);

			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity, person1, date);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MoveInvalidOverlappedActivity, person2, date);

			var input = new MoveNonoverwritableLayersFormData
			{
				PersonIds = new[] { person1.Id.Value, person2.Id.Value },
				Date = date,
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();

			Target.MoveNonoverwritableLayers(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
		}



		[Test]
		public void ShouldReturnNoWriteProtectedAgentsIfHasModifyWriteProtectedSchedulePermission()
		{
			var agenta = PersonFactory.CreatePersonWithGuid("a", "a");
			var agentb = PersonFactory.CreatePersonWithGuid("b", "b");


			PersonRepository.Has(agenta);
			PersonRepository.Has(agentb);

			agenta.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 1, 1);
			agentb.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 5, 1);

			var result = Target.CheckWriteProtectedAgents(new DateOnly(2016, 3, 1), new[] { agenta.Id.Value, agentb.Id.Value });
			result.ToList().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnWriteProtectedAgentsIfWithoutModifyWriteProtectedSchedulePermission()
		{
			PermissionProvider.Enable();
			var agenta = PersonFactory.CreatePersonWithGuid("a", "a");
			var agentb = PersonFactory.CreatePersonWithGuid("b", "b");

			PersonRepository.Has(agenta);
			PersonRepository.Has(agentb);

			agenta.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 1, 1);
			agentb.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 5, 1);

			var result = Target.CheckWriteProtectedAgents(new DateOnly(2016, 3, 1), new[] { agenta.Id.Value, agentb.Id.Value });
			result.ToList().Count.Should().Be.EqualTo(1);
		}

	

		[Test]
		public void ShouldNotBackoutScheduleChangeToWriteProtectedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 6, 1);

			var date = new DateOnly(2016, 4, 16);

			var input = new BackoutScheduleChangeFormData
			{
				PersonDates = new[]
				{
					new PersonDate
					{
						Date = date,
						PersonId = person.Id.Value ,
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.BackoutScheduleChange(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);

			results.Count.Should().Be.EqualTo(1);
			results.First().ErrorMessages.Count.Should().Be.EqualTo(1);
			results.First().ErrorMessages[0].Should().Be.EqualTo(Resources.WriteProtectSchedule);
		}


		[Test]
		public void ShouldNotChangeInternalNoteForWriteProtectedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 6, 1);

			var date = new DateOnly(2016, 4, 16);

			var input = new EditScheduleNoteFormData
			{
				SelectedDate = date,
				PersonId = person.Id.Value,
				InternalNote = "new note"
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.EditScheduleNote(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);

			results.Count.Should().Be.EqualTo(1);
			results.First().ErrorMessages.Count.Should().Be.EqualTo(1);
			results.First().ErrorMessages[0].Should().Be.EqualTo(Resources.WriteProtectSchedule);
		}

		[Test]
		public void ShouldNotChangePublicNoteForWriteProtectedSchedule()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 6, 1);

			var date = new DateOnly(2016, 4, 16);

			var input = new EditScheduleNoteFormData
			{
				SelectedDate = date,
				PersonId = person.Id.Value,
				PublicNote = "public note"
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.EditScheduleNote(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);

			results.Count.Should().Be.EqualTo(1);
			results.First().ErrorMessages.Count.Should().Be.EqualTo(1);
			results.First().ErrorMessages[0].Should().Be.EqualTo(Resources.WriteProtectSchedule);

		}

		[Test]
		public void ShouldInvokeEditNoteCommandHandlerWhenPublicNoteChanged()
		{
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);

			var input = new EditScheduleNoteFormData
			{
				SelectedDate = date,
				PersonId = person.Id.Value,
				PublicNote = "public note"
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.EditScheduleNote(input);

			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			(ActivityCommandHandler.CalledCommands.Single() as EditScheduleNoteCommand).PublicNote.Should()
				.Be.EqualTo("public note");
			results.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldInvokeBackoutScheduleChangeCommandHandler()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 1, 1);

			var date = new DateOnly(2016, 4, 16);

			var input = new BackoutScheduleChangeFormData
			{
				PersonDates = new[]
				{
					new PersonDate
					{
						Date = date,
						PersonId = person.Id.Value ,
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			ActivityCommandHandler.ResetCalledCount();
			var results = Target.BackoutScheduleChange(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			results.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnErrorWhenChangingShiftCategoryWithoutMainShift()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.EditShiftCategory, person, date);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			PersonAssignmentRepo.Add(personAss);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("sc").WithId();
			ShiftCategoryRepository.Add(shiftCategory);

			var input = new ChangeShiftCategoryFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonIds = new List<Guid> { person.Id.Value },
				Date = date,
				ShiftCategoryId = shiftCategory.Id.Value
			};
			ActivityCommandHandler.ResetCalledCount();
			var results = Target.ChangeShiftCategory(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(1);
			results.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotChangeShiftCategoryOnWriteProtectedSchedule()
		{

			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var date = new DateOnly(2016, 4, 16);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, person, date);
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2016, 6, 1);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);
			var personAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, new DateTimePeriod(2016, 4, 16, 8, 2016, 4, 16, 16));
			PersonAssignmentRepo.Add(personAss);

			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("sc").WithId();
			ShiftCategoryRepository.Add(shiftCategory);

			var input = new ChangeShiftCategoryFormData
			{
				TrackedCommandInfo = new TrackedCommandInfo(),
				PersonIds = new List<Guid> { person.Id.Value },
				Date = date,
				ShiftCategoryId = shiftCategory.Id.Value
			};
			ActivityCommandHandler.ResetCalledCount();
			var results = Target.ChangeShiftCategory(input);
			ActivityCommandHandler.CalledCount.Should().Be.EqualTo(0);
			results.Count.Should().Be.EqualTo(1);
		}
	}

	public class FakeActivityCommandHandler :
		IHandleCommand<AddActivityCommand>,
		IHandleCommand<AddOvertimeActivityCommand>,
		IHandleCommand<AddPersonalActivityCommand>,
		IHandleCommand<RemoveActivityCommand>,
		IHandleCommand<MoveShiftLayerCommand>,
		IHandleCommand<BackoutScheduleChangeCommand>,
		IHandleCommand<ChangeShiftCategoryCommand>,
		IHandleCommand<FixNotOverwriteLayerCommand>,
		IHandleCommand<EditScheduleNoteCommand>,
		IHandleCommand<MoveShiftCommand>
	{
		private int calledCount;
		private IList<ITrackableCommand> commands = new List<ITrackableCommand>();
		public void Handle(AddActivityCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public int CalledCount
		{
			get { return calledCount; }
		}

		public IList<ITrackableCommand> CalledCommands
		{
			get { return commands; }
		}
		public void ResetCalledCount()
		{
			calledCount = 0;
		}

		public void Handle(RemoveActivityCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(MoveShiftLayerCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(AddPersonalActivityCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(AddOvertimeActivityCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(BackoutScheduleChangeCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(ChangeShiftCategoryCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(MoveActivityCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(FixNotOverwriteLayerCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(EditScheduleNoteCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(MoveShiftCommand command)
		{
			calledCount++;
			commands.Add(command);
		}
	}
}
