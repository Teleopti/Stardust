using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[NoDefaultData]
	[DomainTest]
	public class EditScheduleNoteCommandHandlerTest : IIsolateSystem
	{
		public EditScheduleNoteCommandHandler Target;
		public FakeWriteSideRepository<IPerson> PersonRepo;
		public IScheduleStorage ScheduleStorage;
		public FakeScenarioRepository CurrentScenario;
		public FakeNoteRepository NoteRepository;
		public FakePublicNoteRepository PublicNoteRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<EditScheduleNoteCommandHandler>().For<IHandleCommand<EditScheduleNoteCommand>>();
			isolate.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
		}

		[Test]
		public void ShouldAddNewInternalNote()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
				InternalNote = "new note"
			};

			Target.Handle(command);
			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), date.ToDateOnlyPeriod(), scenario)[person].ScheduledDayCollection(date.ToDateOnlyPeriod()).Single();
			var note = schedule.NoteCollection().FirstOrDefault();
			note.GetScheduleNote(new NoFormatting()).Should().Be("new note");

		}

		[Test]
		public void ShouldUpdateInternalNote()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);
			var scenario = CurrentScenario.Has("Default");

			var note = new Note(person, date, scenario, "existing note").WithId();
			NoteRepository.Add(note);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
				InternalNote = "new note"
			};

			Target.Handle(command);
			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), date.ToDateOnlyPeriod(), scenario)[person].ScheduledDayCollection(date.ToDateOnlyPeriod()).Single();
			var updatedNote = schedule.NoteCollection().FirstOrDefault();
			updatedNote.GetScheduleNote(new NoFormatting()).Should().Be("new note");
		}

		[Test]
		public void ShouldNotUpdatePublicNoteWhenInputIsNull()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);
			var existingNote = new PublicNote(person, date, scenario, "existing note").WithId();
			PublicNoteRepository.Add(existingNote);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault()
			};

			Target.Handle(command);
			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), date.ToDateOnlyPeriod(), scenario)[person].ScheduledDayCollection(date.ToDateOnlyPeriod()).Single();
			var note = schedule.PublicNoteCollection().FirstOrDefault();
			note.GetScheduleNote(new NoFormatting()).Should().Be("existing note");
		}
		[Test]
		public void ShouldAddNewPublicNote()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
				PublicNote = "new note"
			};

			Target.Handle(command);
			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), date.ToDateOnlyPeriod(), scenario)[person].ScheduledDayCollection(date.ToDateOnlyPeriod()).Single();
			var note = schedule.PublicNoteCollection().FirstOrDefault();
			note.GetScheduleNote(new NoFormatting()).Should().Be("new note");
		}

		[Test]
		public void ShouldClearPublicNote()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);

			var date = new DateOnly(2016, 9, 29);
			var existingNote = new PublicNote(person, date, scenario, "existing note").WithId();
			PublicNoteRepository.Add(existingNote);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
				PublicNote = ""
			};

			Target.Handle(command);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), date.ToDateOnlyPeriod(), scenario)[person].ScheduledDayCollection(date.ToDateOnlyPeriod()).Single();
			var note = schedule.PublicNoteCollection().SingleOrDefault();
			note.Should().Be.Null();
		}

		[Test]
		public void ShouldUpdatePublicNote()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);
			var existingNote = new PublicNote(person, date, scenario, "existing note").WithId();
			PublicNoteRepository.Add(existingNote);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
				PublicNote = "another one"
			};

			Target.Handle(command);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), date.ToDateOnlyPeriod(), scenario)[person].ScheduledDayCollection(date.ToDateOnlyPeriod()).Single();
			var note = schedule.PublicNoteCollection().SingleOrDefault();
			note.Should().Not.Be.Null();
			note.Id.Should().Be.EqualTo(existingNote.Id);
			note.GetScheduleNote(new NoFormatting()).Should().Be("another one");
		}
	}
}
