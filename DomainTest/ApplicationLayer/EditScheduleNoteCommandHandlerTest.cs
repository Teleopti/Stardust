using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture, DomainTest]
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
	}
}
