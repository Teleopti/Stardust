using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core
{
	[TestFixture, TeamScheduleTest]
	public class TeamScheduleCommandHandlingProviderTest : IIsolateSystem
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
		public FakeActivityRepository ActivityRepository;
		public IScheduleStorage ScheduleStorage;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
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
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.MyTeamSchedules, person, new DateOnly(2018, 1, 10));

			var template = DayOffFactory.CreateDayOff(new Description("template")).WithId(); ;
			DayOffTemplateRepository.Has(template);

			var results = Target.AddDayOff(new AddDayOffFormData
			{
				StartDate = new DateOnly(2018, 1, 10),
				EndDate = new DateOnly(2018, 1, 10),
				PersonIds = new Guid[] { person.Id.Value },
				TemplateId = template.Id.Value
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionAddDayOff);
		}

		[Test]
		public void ShouldReturnErrorWhenAddingDayOffToUnpublishedScheduleAndNoPermissionToViewUnpublishedSchedule()
		{
			PermissionProvider.Enable();
			var dateonly = new DateOnly(2018, 1, 10);
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			PermissionProvider.PublishToDate(dateonly.AddDays(-1));
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddDayOff, person, dateonly);

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
			CommandHandler.CalledCount.Should().Be.EqualTo(0);
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionToEditUnpublishedSchedule);
		}

		[Test]
		public void ShouldInvokeAddDayOffCommandHandler()
		{
			PermissionProvider.Enable();
			var dateonly = new DateOnly(2018, 1, 10);
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			PermissionProvider.PermitPerson(DefinedRaptorApplicationFunctionPaths.AddDayOff, person, dateonly);

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
			CommandHandler.GetSingleCommand<ITrackableCommand>().TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
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
		public void ShouldReturnErrorWhenRemoveDayOffWithoutPerson()
		{
			var results = Target.RemoveDayOff(new RemoveDayOffFormData
			{
				Date = new DateOnly(2018, 1, 12)
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
		public void ShouldReturnErrorWhenRemoveDayOffWithoutPermission()
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(person);
			PermissionProvider.Enable();
			var results = Target.RemoveDayOff(new RemoveDayOffFormData
			{
				Date = new DateOnly(2018, 1, 12),
				PersonIds = new[] { person.Id.GetValueOrDefault() }
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionRemoveDayOff);
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
			CommandHandler.GetSingleCommand<ITrackableCommand>().TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
			results.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldAddCommandExecutedResultAfterInvokingRemoveDayOffCommandWithValidInput()
		{
			var person = PersonFactory.CreatePersonWithGuid("a", "b");
			PersonRepository.Has(person);
			var trackId = Guid.NewGuid();

			CommandHandler.HasError = true;
			var results = Target.RemoveDayOff(new RemoveDayOffFormData
			{
				Date = DateOnly.Today,
				PersonIds = new[] { person.Id.GetValueOrDefault() },
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = trackId }
			});

			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			CommandHandler.GetSingleCommand<ITrackableCommand>().TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
			results.Single().ErrorMessages.Single().Should().Be.EqualTo("Execute command failed.");
		}

		[Test]
		public void ShouldReturnErrorWhenRemoveShiftWithoutDate()
		{
			var results = Target.RemoveShift(new RemoveShiftFormData
			{
				PersonIds = new Guid[] { Guid.NewGuid() }
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldReturnErrorWhenRemoveShiftWithoutPerson()
		{
			var results = Target.RemoveShift(new RemoveShiftFormData
			{
				Date = new DateOnly(2018, 2, 2)
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldReturnErrorWhenRemoveShiftWithEmptyPersonList()
		{
			var results = Target.RemoveShift(new RemoveShiftFormData
			{
				Date = new DateOnly(2018, 2, 2),
				PersonIds = new Guid[] { }
			});
			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.InvalidInput);
		}

		[Test]
		public void ShouldReturnErrorWhenRemovingShiftWithoutPermission()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);
			PermissionProvider.Enable();

			var results = Target.RemoveShift(new RemoveShiftFormData
			{
				Date = new DateOnly(2018, 2, 2),
				PersonIds = new[] { person.Id.GetValueOrDefault() },
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid() }
			});

			results.Single().ErrorMessages.Single().Should().Be.EqualTo(Resources.NoPermissionRemovingShift);
		}

		[Test]
		public void ShouldInvokeRemoveShiftCommandWithValidInput()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);
			var trackId = Guid.NewGuid();

			var results = Target.RemoveShift(new RemoveShiftFormData
			{
				Date = new DateOnly(2018, 2, 2),
				PersonIds = new[] { person.Id.GetValueOrDefault() },
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = trackId }
			});
			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			CommandHandler.GetSingleCommand<ITrackableCommand>().TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
			results.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldInvokeMultipleChangeScheduleCommandWithChangeActivityTypeCommand()
		{
			var date = new DateOnly(2018, 6, 22);
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(phoneActivity);
			var invoiceActivity = ActivityFactory.CreateActivity("invoice", Color.Yellow);
			ActivityRepository.Has(invoiceActivity);

			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, date);
			PersonAssignmentRepo.Add(personAss);

			personAss.AddActivity(phoneActivity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 9));
			personAss.AddActivity(invoiceActivity, new DateTimePeriod(2018, 6, 22, 9, 2018, 6, 22, 10));

			personAss.ShiftLayers.ForEach(sl => sl.SetId(Guid.NewGuid()));
			var layerIds = personAss.ShiftLayers.Select(sl => sl.Id.Value).ToArray();

			var result = Target.ChangeActivityType(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {  new EditingLayerModel
					{
						ActivityId = invoiceActivity.Id.GetValueOrDefault(),
						ShiftLayerIds = layerIds
					} }
			});

			CommandHandler.CalledCount.Should().Be.EqualTo(1);

			var dateonly = new DateOnly(new DateTime(2018, 6, 22));
			var multipleCommand = CommandHandler.CalledCommands.Single() as MultipleChangeScheduleCommand;
			multipleCommand.Person.Should().Be(person);
			multipleCommand.Date.Should().Be(dateonly);
			multipleCommand.ScheduleDictionary[person].ScheduledDay(dateonly).PersonAssignment().ShiftLayers.Count().Should().Be.EqualTo(2);

			multipleCommand.Commands.Count().Should().Be.EqualTo(2);

			var firstCommand = multipleCommand.Commands.First() as ChangeActivityTypeCommand;
			firstCommand.ShiftLayer.Id.Should().Be.EqualTo(layerIds[0]);
			firstCommand.Activity.Id.Should().Be.EqualTo(invoiceActivity.Id);

			var secondCommand = multipleCommand.Commands.Second() as ChangeActivityTypeCommand;
			secondCommand.ShiftLayer.Id.Should().Be.EqualTo(layerIds[1]);
			secondCommand.Activity.Id.Should().Be.EqualTo(invoiceActivity.Id);
		}

		[Test]
		public void ShouldInvokeMultipleChangeScheduleCommandWithAddActivityCommandWhenChangeActivityTypeLayersContainsNewActivityLayer()
		{
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());

			var date = new DateOnly(2018, 6, 22);
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(phoneActivity);
			var invoiceActivity = ActivityFactory.CreateActivity("invoice", Color.Yellow);
			ActivityRepository.Has(invoiceActivity);

			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, date);
			PersonAssignmentRepo.Add(personAss);

			personAss.AddActivity(phoneActivity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 16));
			personAss.AddActivity(invoiceActivity, new DateTimePeriod(2018, 6, 22, 9, 2018, 6, 22, 10));
			personAss.ShiftLayers.ForEach(sl => sl.SetId(Guid.NewGuid()));

			var layerIds = personAss.ShiftLayers.Select(sl => sl.Id.Value).ToArray();

			var startTime = new DateTime(2018, 6, 22, 8, 0, 0, 0);
			var endTime = new DateTime(2018, 6, 22, 9, 0, 0, 0);
			Target.ChangeActivityType(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {  new EditingLayerModel
					{
						ActivityId = invoiceActivity.Id.GetValueOrDefault(),
						ShiftLayerIds =new [ ]{ layerIds[0]},
						StartTime =startTime,
						EndTime =endTime,
						IsNew = true
					} }
			});

			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			var multipleCommand = CommandHandler.CalledCommands.Single() as MultipleChangeScheduleCommand;


			var cmd = multipleCommand.Commands.Single() as AddActivityCommandSimply;

			multipleCommand.Date.Should().Be.EqualTo(date);
			multipleCommand.Person.Should().Be.EqualTo(person);
			cmd.Activity.Should().Be.EqualTo(invoiceActivity);
			cmd.Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 22, 0, 0, 0, 0));
			cmd.Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 22, 1, 0, 0, 0));
		}

		[Test]
		public void ShouldInvokeMultipleChangeScheduleCommandWithAddPersonalActivityCommandWhenChangeActivityTypeLayersContainsNewPersonalActivityLayer()
		{
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());

			var date = new DateOnly(2018, 6, 22);
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(phoneActivity);
			var invoiceActivity = ActivityFactory.CreateActivity("invoice", Color.Yellow);
			ActivityRepository.Has(invoiceActivity);

			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, date);
			PersonAssignmentRepo.Add(personAss);

			personAss.AddActivity(phoneActivity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 16));
			personAss.AddPersonalActivity(invoiceActivity, new DateTimePeriod(2018, 6, 22, 9, 2018, 6, 22, 12));
			personAss.AddPersonalActivity(phoneActivity, new DateTimePeriod(2018, 6, 22, 10, 2018, 6, 22, 11));
			personAss.ShiftLayers.ForEach(sl => sl.SetId(Guid.NewGuid()));

			var layerIds = personAss.ShiftLayers.Select(sl => sl.Id.Value).ToArray();

			var startTime = new DateTime(2018, 6, 22, 11, 0, 0, 0);
			var endTime = new DateTime(2018, 6, 22, 12, 0, 0, 0);
			Target.ChangeActivityType(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {  new EditingLayerModel
					{
						ActivityId = invoiceActivity.Id.GetValueOrDefault(),
						ShiftLayerIds =new [ ]{ layerIds[1]},
						StartTime = startTime,
						EndTime = endTime,
						IsNew = true
					} }
			});

			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			var multipleCommand = CommandHandler.CalledCommands.Single() as MultipleChangeScheduleCommand;
			var addPersonalActivityCommand = multipleCommand.Commands.Single() as AddPersonalActivityCommandSimply;
			multipleCommand.Date.Should().Be.EqualTo(date);
			multipleCommand.Person.Should().Be.EqualTo(person);
			addPersonalActivityCommand.Activity.Should().Be.EqualTo(invoiceActivity);
			addPersonalActivityCommand.Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 22, 3, 0, 0, 0));
			addPersonalActivityCommand.Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 22, 4, 0, 0, 0));
		}

		[Test]
		public void ShouldInvokeMultipleChangeScheduleCommandWithAddOvertimeActivityCommandWhenChangeActivityTypeLayersContainsNewOvertimeActivityLayer()
		{
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());

			var date = new DateOnly(2018, 6, 22);
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(phoneActivity);
			var invoiceActivity = ActivityFactory.CreateActivity("invoice", Color.Yellow);
			ActivityRepository.Has(invoiceActivity);

			var definitionSet = new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime);

			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, date);
			PersonAssignmentRepo.Add(personAss);

			personAss.AddActivity(phoneActivity, new DateTimePeriod(2018, 6, 22, 8, 2018, 6, 22, 16));
			personAss.AddOvertimeActivity(invoiceActivity, new DateTimePeriod(2018, 6, 22, 9, 2018, 6, 22, 12), definitionSet);
			personAss.AddOvertimeActivity(phoneActivity, new DateTimePeriod(2018, 6, 22, 10, 2018, 6, 22, 11), definitionSet);
			personAss.ShiftLayers.ForEach(sl => sl.SetId(Guid.NewGuid()));

			var layerIds = personAss.ShiftLayers.Select(sl => sl.Id.Value).ToArray();

			var startTime = new DateTime(2018, 6, 22, 11, 0, 0, 0);
			var endTime = new DateTime(2018, 6, 22, 12, 0, 0, 0);
			Target.ChangeActivityType(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				Layers = new[] {  new EditingLayerModel
					{
						ActivityId = invoiceActivity.Id.GetValueOrDefault(),
						ShiftLayerIds =new [ ]{ layerIds[1]},
						StartTime = startTime,
						EndTime = endTime,
						IsNew = true
					} }
			});

			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			var multipleCommand = CommandHandler.CalledCommands.Single() as MultipleChangeScheduleCommand;
			var addOvertimeActivityCommand = multipleCommand.Commands.Single() as AddOvertimeActivityCommandSimply;
			multipleCommand.Date.Should().Be.EqualTo(date);
			multipleCommand.Person.Should().Be.EqualTo(person);
			addOvertimeActivityCommand.Activity.Should().Be.EqualTo(invoiceActivity);
			addOvertimeActivityCommand.Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 22, 3, 0, 0, 0));
			addOvertimeActivityCommand.Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 6, 22, 4, 0, 0, 0));
			addOvertimeActivityCommand.MultiplicatorDefinitionSet.Should().Be.EqualTo(definitionSet);
		}

		[Test]
		public void ShouldInvokeMultipleChangeScheduleCommandWithTrackCommandInfo()
		{
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo());

			var date = new DateOnly(2018, 6, 22);
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var phoneActivity = ActivityFactory.CreateActivity("phone", Color.Yellow);
			ActivityRepository.Has(phoneActivity);
			var invoiceActivity = ActivityFactory.CreateActivity("invoice", Color.Yellow);
			ActivityRepository.Has(invoiceActivity);

			var personAss = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, date);
			PersonAssignmentRepo.Add(personAss);
			personAss.AddActivity(phoneActivity, new DateTimePeriod(new DateTime(2018, 6, 22, 11, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 6, 22, 12, 0, 0, 0, DateTimeKind.Utc)));

			personAss.ShiftLayers.ForEach(l => l.SetId(Guid.NewGuid()));
			var layerIds = personAss.ShiftLayers.Select(sl => sl.Id.Value).ToArray();

			var trackCommandInfo = new TrackedCommandInfo { TrackId = Guid.NewGuid(), OperatedPersonId = Guid.NewGuid() };
			Target.ChangeActivityType(new ChangeActivityTypeFormData
			{
				Date = new DateTime(2018, 6, 22),
				PersonId = person.Id.Value,
				TrackedCommandInfo = trackCommandInfo,
				Layers = new[] {  new EditingLayerModel
					{
						ActivityId = invoiceActivity.Id.GetValueOrDefault(),
						ShiftLayerIds =new [ ]{ layerIds[0]}
					} }
			});

			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			var multipleCommand = CommandHandler.CalledCommands.Single() as MultipleChangeScheduleCommand;
			multipleCommand.TrackedCommandInfo.Should().Be.EqualTo(trackCommandInfo);
		}

		[Test]
		public void ShouldInvokeRemoveAbsenceCommandWithValidInput()
		{

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);
			var trackId = Guid.NewGuid();
			var personAbsenceId = Guid.NewGuid();

			var results = Target.RemoveAbsence(new RemovePersonAbsenceForm
			{
				SelectedPersonAbsences = new[] {
					new SelectedPersonAbsence{
						AbsenceDates = new []{ new AbsenceDate { Date = new DateOnly(2018,7,23), PersonAbsenceId = personAbsenceId } },
						PersonId =person.Id.Value
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = trackId }
			});

			CommandHandler.CalledCount.Should().Be.EqualTo(1);
			var command = CommandHandler.CalledCommands.Single() as RemoveSelectedPersonAbsenceCommand;
			command.ScheduleRange.Period.StartDateTime.Should().Be.EqualTo(new DateTime(2018, 7, 22));
			command.ScheduleRange.Period.EndDateTime.Should().Be.EqualTo(new DateTime(2018, 7, 24));
			command.TrackedCommandInfo.TrackId.Should().Be.EqualTo(trackId);
			command.PersonAbsenceId.Should().Be.EqualTo(personAbsenceId);
			command.Person.Should().Be.EqualTo(person);
		}

		[Test]
		public void ShouldReturnErrorBasedOnCurrentCultureWhenRemoveAbsenceWithoutPermission()
		{
			PermissionProvider.Enable();

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.Has(scenario);

			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Has(person);
			var trackId = Guid.NewGuid();
			var personAbsenceId = Guid.NewGuid();

			var removeAbsenceForm = new RemovePersonAbsenceForm
			{
				SelectedPersonAbsences = new[] {
					new SelectedPersonAbsence{
						AbsenceDates = new []{ new AbsenceDate { Date = new DateOnly(2018,7,23), PersonAbsenceId = personAbsenceId } },
						PersonId =person.Id.Value
					}
				},
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = trackId }
			};

			var results = Target.RemoveAbsence(removeAbsenceForm);
			Thread.CurrentThread.CurrentUICulture = CultureInfoFactory.CreateEnglishCulture();
			results.Single().ErrorMessages.Single().Should().Be.EqualTo("No permission to remove absence from agents.");

			Thread.CurrentThread.CurrentUICulture = CultureInfoFactory.CreateChineseCulture();
			results = Target.RemoveAbsence(removeAbsenceForm);
			results.Single().ErrorMessages.Single().Should().Be.EqualTo("没有权限为座席代表移除缺勤。");

		}


	}
}
