using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[TestFixture]
	[DatabaseTest]
	public abstract class ScheduleRangePersisterBaseTest : IReassociateDataForSchedules
	{
		protected IPerson Person { get; private set; }
		protected IActivity Activity { get; private set; }
		protected IScenario Scenario { get; private set; }
		protected IShiftCategory ShiftCategory { get; private set; }
		protected IAbsence Absence { get; private set; }
		protected IMultiplicatorDefinitionSet DefinitionSet { get; private set; }
		protected IScheduleRangePersister Target { get; private set; }
		protected IDayOffTemplate DayOffTemplate { get; private set; }

		[SetUp]
		protected void Setup()
		{
			setupEntities();
			setupDatabase();
			makeTarget();
		}

		protected abstract IEnumerable<IPersistableScheduleData> Given();

		private void makeTarget()
		{
			Target = CreateTarget();
		}

		protected virtual IScheduleRangePersister CreateTarget()
		{
			var currUnitOfWork = new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make());
			var personAssignmentRepository = new PersonAssignmentRepository(currUnitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(currUnitOfWork);
			var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currUnitOfWork);
			var noteRepository = new NoteRepository(currUnitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(currUnitOfWork);
			var publicNoteRepository = new PublicNoteRepository(currUnitOfWork);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currUnitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currUnitOfWork);
			var scheduleRep = new ScheduleStorage(currUnitOfWork, personAssignmentRepository,
				personAbsenceRepository, new MeetingRepository(currUnitOfWork),
				agentDayScheduleTagRepository, noteRepository,
				publicNoteRepository, preferenceDayRepository,
				studentAvailabilityDayRepository,
				new PersonAvailabilityRepository(currUnitOfWork), new PersonRotationRepository(currUnitOfWork),
				overtimeAvailabilityRepository,
				new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
				new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
					() => personAbsenceRepository,
					() => preferenceDayRepository, () => noteRepository,
					() => publicNoteRepository,
					() => studentAvailabilityDayRepository,
					() => agentDayScheduleTagRepository,
					() => overtimeAvailabilityRepository), CurrentAuthorization.Make());
			return new ScheduleRangePersister(CurrentUnitOfWorkFactory.Make(),
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				ConflictCollector(),
				new ScheduleDifferenceSaver(new EmptyScheduleDayDifferenceSaver(), new PersistScheduleChanges(scheduleRep, CurrentUnitOfWork.Make())),
				MockRepository.GenerateMock<IInitiatorIdentifier>(),
				new KeepScheduleEvents());
		}

		private void setupEntities()
		{
			Person = PersonFactory.CreatePerson("persist", "test");
			Activity = new Activity("persist test");
			ShiftCategory = new ShiftCategory("persist test");
			Scenario = new Scenario("scenario");
			Absence = new Absence { Description = new Description("perist", "test") };
			DefinitionSet = new MultiplicatorDefinitionSet("persist test", MultiplicatorType.Overtime);
			DayOffTemplate = new DayOffTemplate(new Description("persist test"));
		}

		private void setupDatabase()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
				new PersonRepository(currentUnitOfWork, null, null).Add(Person);
				new ActivityRepository(unitOfWork).Add(Activity);
				new ShiftCategoryRepository(unitOfWork).Add(ShiftCategory);
				new ScenarioRepository(unitOfWork).Add(Scenario);
				new AbsenceRepository(unitOfWork).Add(Absence);
				new MultiplicatorDefinitionSetRepository(unitOfWork).Add(DefinitionSet);
				new DayOffTemplateRepository(unitOfWork).Add(DayOffTemplate);
				var personAssignmentRepository = new PersonAssignmentRepository(currentUnitOfWork);
				var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
				var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currentUnitOfWork);
				var noteRepository = new NoteRepository(currentUnitOfWork);
				var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
				var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
				var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
				var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
				var scheduleStorage = new ScheduleStorage(currentUnitOfWork,
					personAssignmentRepository, personAbsenceRepository,
					new MeetingRepository(currentUnitOfWork), agentDayScheduleTagRepository,
					noteRepository, publicNoteRepository,
					preferenceDayRepository,
					studentAvailabilityDayRepository,
					new PersonAvailabilityRepository(currentUnitOfWork),
					new PersonRotationRepository(currentUnitOfWork),
					overtimeAvailabilityRepository,
					new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
					new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
						() => personAbsenceRepository,
						() => preferenceDayRepository, () => noteRepository,
						() => publicNoteRepository,
						() => studentAvailabilityDayRepository,
						() => agentDayScheduleTagRepository,
						() => overtimeAvailabilityRepository), CurrentAuthorization.Make());
				Given().ForEach(x =>
				{
					scheduleStorage.Add(x);
				});
				unitOfWork.PersistAll();
			}
		}

		protected void DoModify(IScheduleDay scheduleDay)
		{
			scheduleDay.Owner.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(),
															 MockRepository.GenerateMock<IScheduleDayChangeCallback>(),
															 MockRepository.GenerateMock<IScheduleTagSetter>());
		}

		protected static void GeneralAsserts(IScheduleRange range, IEnumerable<PersistConflict> conflicts)
		{
			var rangeDiff = range.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>());

			if (conflicts.Any())
			{
				rangeDiff.Should("After save, there is a no diff in Snapshot and current data in ScheduleRange even though conflicts were found. Indicates that something is very wrong!").Not.Be.Empty();
			}
			else
			{
				rangeDiff.Should("After save, there is a diff in Snapshot and current data in ScheduleRange even though no conflicts were found. Indicates that something is very wrong!").Be.Empty();
			}
		}

		protected IScheduleRange LoadScheduleRange()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ReassociateDataFor(Person);
				var currentUnitOfWork = new ThisUnitOfWork(unitOfWork);
				var personAssignmentRepository = new PersonAssignmentRepository(currentUnitOfWork);
				var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
				var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currentUnitOfWork);
				var noteRepository = new NoteRepository(currentUnitOfWork);
				var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
				var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
				var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
				var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
				var rep = new ScheduleStorage(currentUnitOfWork, personAssignmentRepository,
					personAbsenceRepository, new MeetingRepository(currentUnitOfWork),
					agentDayScheduleTagRepository, noteRepository,
					publicNoteRepository, preferenceDayRepository,
					studentAvailabilityDayRepository,
					new PersonAvailabilityRepository(currentUnitOfWork),
					new PersonRotationRepository(currentUnitOfWork),
					overtimeAvailabilityRepository,
					new PersistableScheduleDataPermissionChecker(new FullPermission()),
					new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
						() => personAbsenceRepository,
						() => preferenceDayRepository, () => noteRepository,
						() => publicNoteRepository,
						() => studentAvailabilityDayRepository,
						() => agentDayScheduleTagRepository,
						() => overtimeAvailabilityRepository), new FullPermission());
				var dictionary = rep.FindSchedulesForPersons(Scenario,
																								 new[] { Person },
																								 new ScheduleDictionaryLoadOptions(true, true),
																								 new DateTimePeriod(1800, 1, 1, 2040, 1, 1), new List<IPerson> { Person }, false);
				return dictionary[Person];
			}
		}

		protected virtual bool ExpectOptimistLockException => false;

		protected virtual IScheduleRangeConflictCollector ConflictCollector()
		{
			var currUnitOfWork = new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make());
			var personAssignmentRepository = new PersonAssignmentRepository(currUnitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(currUnitOfWork);
			var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currUnitOfWork);
			var noteRepository = new NoteRepository(currUnitOfWork);
			var publicNoteRepository = new PublicNoteRepository(currUnitOfWork);
			var preferenceDayRepository = new PreferenceDayRepository(currUnitOfWork);
			var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currUnitOfWork);
			var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currUnitOfWork);
			var scheduleRep = new ScheduleStorage(currUnitOfWork, personAssignmentRepository,
				personAbsenceRepository, new MeetingRepository(currUnitOfWork),
				agentDayScheduleTagRepository, noteRepository,
				publicNoteRepository, preferenceDayRepository,
				studentAvailabilityDayRepository, new PersonAvailabilityRepository(currUnitOfWork),
				new PersonRotationRepository(currUnitOfWork), overtimeAvailabilityRepository,
				new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
				new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
					() => personAbsenceRepository,
					() => preferenceDayRepository, () => noteRepository,
					() => publicNoteRepository,
					() => studentAvailabilityDayRepository,
					() => agentDayScheduleTagRepository,
					() => overtimeAvailabilityRepository), CurrentAuthorization.Make());
			return new ScheduleRangeConflictCollector(scheduleRep, personAssignmentRepository, this, new LazyLoadingManagerWrapper(), new DatabaseVersion(currUnitOfWork));
		}

		public abstract void ReassociateDataForAllPeople();
		public abstract void ReassociateDataFor(IPerson person);
	}
}