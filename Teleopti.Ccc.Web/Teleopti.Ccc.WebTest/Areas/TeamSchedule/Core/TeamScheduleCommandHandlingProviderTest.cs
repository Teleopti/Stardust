using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.WebTest.Areas.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core
{
	[TestFixture, TeamScheduleTest]
	public class TeamScheduleCommandHandlingProviderTest : ISetup
	{
		public ITeamScheduleCommandHandlingProvider Target;
		public FakeCommandHandler CommandHandler;
		public FakePersonRepository PersonRepository;
		public Global.FakePermissionProvider PermissionProvider;
		public FakePersonAssignmentWriteSideRepository PersonAssignmentRepo;
		public FakeScenarioRepository CurrentScenario;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeShiftCategoryRepository ShiftCategoryRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
		}


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

			CommandHandler.ResetCalledCount();

			Target.MoveNonoverwritableLayers(input);

			CommandHandler.CalledCount.Should().Be.EqualTo(0);
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

			CommandHandler.ResetCalledCount();

			Target.MoveNonoverwritableLayers(input);

			CommandHandler.CalledCount.Should().Be.EqualTo(2);
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

			CommandHandler.ResetCalledCount();
			var results = Target.BackoutScheduleChange(input);
			CommandHandler.CalledCount.Should().Be.EqualTo(0);

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

			CommandHandler.ResetCalledCount();
			var results = Target.EditScheduleNote(input);
			CommandHandler.CalledCount.Should().Be.EqualTo(0);

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

			CommandHandler.ResetCalledCount();
			var results = Target.EditScheduleNote(input);
			CommandHandler.CalledCount.Should().Be.EqualTo(0);

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

			CommandHandler.ResetCalledCount();
			var results = Target.EditScheduleNote(input);

			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			(CommandHandler.CalledCommands.Single() as EditScheduleNoteCommand).PublicNote.Should()
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

			CommandHandler.ResetCalledCount();
			var results = Target.BackoutScheduleChange(input);
			CommandHandler.CalledCount.Should().Be.EqualTo(1);
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
			CommandHandler.ResetCalledCount();
			var results = Target.ChangeShiftCategory(input);
			CommandHandler.CalledCount.Should().Be.EqualTo(1);
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
			CommandHandler.ResetCalledCount();
			var results = Target.ChangeShiftCategory(input);
			CommandHandler.CalledCount.Should().Be.EqualTo(0);
			results.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnErrorWhenAddDayOffWithInvalidDate()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);

			var results = Target.AddDayOff(new AddDayOffFormData
			{
				StartDate = new DateOnly(2018, 1, 10),
				EndDate = new DateOnly(2018, 1, 9),
				PersonIds = new Guid[] { person.Id.Value },
				TemplateId = Guid.NewGuid()
			});

			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}
		[Test]
		public void ShouldReturnErrorWhenAddDayOffWithNoPeople()
		{
			var results = Target.AddDayOff(new AddDayOffFormData
			{
				StartDate = new DateOnly(2018, 1, 10),
				EndDate = new DateOnly(2018, 1, 10),
				TemplateId = Guid.NewGuid()
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldReturnErrorWhenAddDayOffWithNoTemplateId()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var results = Target.AddDayOff(new AddDayOffFormData
			{
				StartDate = new DateOnly(2018, 1, 10),
				EndDate = new DateOnly(2018, 1, 10),
				PersonIds = new Guid[] { person.Id.Value }
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldReturnErrorWhenAddDayOffWithNoExistTemplate()
		{
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var results = Target.AddDayOff(new AddDayOffFormData
			{
				StartDate = new DateOnly(2018, 1, 10),
				EndDate = new DateOnly(2018, 1, 10),
				PersonIds = new Guid[] { person.Id.Value },
				TemplateId = Guid.NewGuid()
			});

			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldReturnErrorWhenAddDayOffWithoutPermittedPerson()
		{
			PermissionProvider.Enable();
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);

			var template = DayOffFactory.CreateDayOff(new Description("template")).WithId(); ;
			DayOffTemplateRepository.Has(template);

			var results = Target.AddDayOff(new AddDayOffFormData
			{
				StartDate = new DateOnly(2018, 1, 10),
				EndDate = new DateOnly(2018, 1, 10),
				PersonIds = new Guid[] { person.Id.Value },
				TemplateId = template.Id.Value
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.YouDoNotHavePermissionsToViewTeamSchedules);
		}

		[Test]
		public void ShouldInvokeAddDayOffCommandHandler()
		{
			PermissionProvider.Enable();
			var dateonly = new DateOnly(2018, 1, 10);
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, person, dateonly);

			var template = DayOffFactory.CreateDayOff(new Description("template")).WithId();
			DayOffTemplateRepository.Has(template);

			CommandHandler.ResetCalledCount();
			var trackId = Guid.NewGuid();
			var results = Target.AddDayOff(new AddDayOffFormData
			{
				StartDate = dateonly,
				EndDate = dateonly,
				PersonIds = new Guid[] { person.Id.Value },
				TemplateId = template.Id.Value,
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = trackId }

			});
			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			CommandHandler.CalledCommands.Single().TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
			results.Count.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldReturnErrorWhenRemoveDayOffWithoutDate()
		{
			var results = Target.RemoveDayOff(new RemoveDayOffFormData
			{
				PersonIds = new Guid[] { Guid.NewGuid() }
			});

			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldReturnErrorWhenRemoveDayOffWithoutPerson() {
			var results = Target.RemoveDayOff(new RemoveDayOffFormData
			{
				Date = new DateOnly(2018,1,12)
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}
		[Test]
		public void ShouldReturnErrorWhenRemoveDayOffWithEmptyPersonIdList()
		{
			var results = Target.RemoveDayOff(new RemoveDayOffFormData
			{
				Date = new DateOnly(2018, 1, 12),
				PersonIds = new Guid[] { }
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}


		[Test]
		public void ShouldReturnErrorWhenRemoveDayOffWithoutPermissionOnTeamSchedule()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(person);
			PermissionProvider.Enable();
			var results = Target.RemoveDayOff(new RemoveDayOffFormData
			{
				Date = new DateOnly(2018, 1, 12),
				PersonIds = new Guid[] { Guid.NewGuid() }
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.YouDoNotHavePermissionsToViewTeamSchedules);
		}

		[Test]
		public void ShouldInvokeRemoveDayOffCommandWithValidInput()
		{
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var trackId = Guid.NewGuid();

			var results = Target.RemoveDayOff(new RemoveDayOffFormData
			{
				Date = DateOnly.Today,
				PersonIds = new[] { person.Id.GetValueOrDefault() },
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = trackId }
			});


			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			CommandHandler.CalledCommands.Single().TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
			results.Count.Should().Be.EqualTo(0);
		}


	}
}
