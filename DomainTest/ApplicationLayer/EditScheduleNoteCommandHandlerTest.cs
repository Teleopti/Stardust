using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	public class EditScheduleNoteCommandHandlerTest : ISetup
	{
		public EditScheduleNoteCommandHandler Target;
		public FakeWriteSideRepository<IPerson> PersonRepo;
		public FakeScheduleStorage ScheduleStorage;
		public FakeCurrentScenario CurrentScenario;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<EditScheduleNoteCommandHandler>().For<IHandleCommand<EditScheduleNoteCommand>>();
			system.UseTestDouble<FakeWriteSideRepository<IPerson>>().For<IProxyForId<IPerson>>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
		}

		[Test]
		public void ShouldAddNewInternalNote()
		{
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
				person, new ScheduleDictionaryLoadOptions(true, true), new DateOnlyPeriod(date, date), CurrentScenario.Current())[person].ScheduledDayCollection(new DateOnlyPeriod(date, date)).Single();
			var note = schedule.NoteCollection().FirstOrDefault();
			note.GetScheduleNote(new NoFormatting()).Should().Be("new note");

		}

		[Test]
		public void ShouldUpdateInternalNote()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);
			var scenario = CurrentScenario.Current();

			var note = new Note(person, date, scenario, "existing note").WithId();
			ScheduleStorage.Add(note);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
				InternalNote = "new note"
			};

			Target.Handle(command);
			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), new DateOnlyPeriod(date, date), CurrentScenario.Current())[person].ScheduledDayCollection(new DateOnlyPeriod(date, date)).Single();
			var updatedNote = schedule.NoteCollection().FirstOrDefault();
			updatedNote.GetScheduleNote(new NoFormatting()).Should().Be("new note");
		}

		[Test]
		public void ShouldNotUpdatePublicNoteWhenInputIsNull()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);
			var existingNote = new PublicNote(person, date, CurrentScenario.Current(), "existing note").WithId();
			ScheduleStorage.Add(existingNote);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
			};

			Target.Handle(command);
			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), new DateOnlyPeriod(date, date), CurrentScenario.Current())[person].ScheduledDayCollection(new DateOnlyPeriod(date, date)).Single();
			var note = schedule.PublicNoteCollection().FirstOrDefault();
			note.GetScheduleNote(new NoFormatting()).Should().Be("existing note");
		}
		[Test]
		public void ShouldAddNewPublicNote()
		{
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
				person, new ScheduleDictionaryLoadOptions(true, true), new DateOnlyPeriod(date, date), CurrentScenario.Current())[person].ScheduledDayCollection(new DateOnlyPeriod(date, date)).Single();
			var note = schedule.PublicNoteCollection().FirstOrDefault();
			note.GetScheduleNote(new NoFormatting()).Should().Be("new note");
		}

		[Test]
		public void ShouldClearPublicNote()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);
			var existingNote = new PublicNote(person, date, CurrentScenario.Current(), "existing note").WithId();
			ScheduleStorage.Add(existingNote);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
				PublicNote = ""
			};

			Target.Handle(command);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), new DateOnlyPeriod(date, date), CurrentScenario.Current())[person].ScheduledDayCollection(new DateOnlyPeriod(date, date)).Single();
			var note = schedule.PublicNoteCollection().SingleOrDefault();
			note.Should().Be.Null();
		}

		[Test]
		public void ShouldUpdatePublicNote()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepo.Add(person);
			var date = new DateOnly(2016, 9, 29);
			var existingNote = new PublicNote(person, date, CurrentScenario.Current(), "existing note");
			existingNote.WithId();
			ScheduleStorage.Add(existingNote);

			var command = new EditScheduleNoteCommand
			{
				Date = new DateOnly(2016, 9, 29),
				PersonId = person.Id.GetValueOrDefault(),
				PublicNote = "another one"
			};

			Target.Handle(command);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), new DateOnlyPeriod(date, date), CurrentScenario.Current())[person].ScheduledDayCollection(new DateOnlyPeriod(date, date)).Single();
			var note = schedule.PublicNoteCollection().SingleOrDefault();
			note.Should().Not.Be.Null();
			note.Id.Should().Be.EqualTo(existingNote.Id);
			note.GetScheduleNote(new NoFormatting()).Should().Be("another one");
		}
	}
}
