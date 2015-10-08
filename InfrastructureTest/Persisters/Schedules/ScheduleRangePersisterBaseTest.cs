using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[TestFixture]
	public abstract class ScheduleRangePersisterBaseTest : DatabaseTestWithoutTransaction, IReassociateDataForSchedules
	{
		protected IPerson Person { get; private set; }
		protected IActivity Activity { get; private set; }
		protected IScenario Scenario { get; private set; }
		protected IShiftCategory ShiftCategory { get; private set; }
		protected IAbsence Absence { get; private set; }
		protected IMultiplicatorDefinitionSet DefinitionSet { get; private set; }
		protected IScheduleRangePersister Target { get; private set; }
		protected IDayOffTemplate DayOffTemplate { get; private set; }

		protected override void SetupForRepositoryTestWithoutTransaction()
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
			var scheduleRep = new ScheduleRepository(currUnitOfWork);
			return new ScheduleRangePersister(CurrentUnitOfWorkFactory.Make(),
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				ConflictCollector(),
				new ScheduleDifferenceSaver(scheduleRep),
				MockRepository.GenerateMock<IInitiatorIdentifier>());
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
				new PersonRepository(new ThisUnitOfWork(unitOfWork)).Add(Person);
				new ActivityRepository(unitOfWork).Add(Activity);
				new ShiftCategoryRepository(unitOfWork).Add(ShiftCategory);
				new ScenarioRepository(unitOfWork).Add(Scenario);
				new AbsenceRepository(unitOfWork).Add(Absence);
				new MultiplicatorDefinitionSetRepository(unitOfWork).Add(DefinitionSet);
				new DayOffTemplateRepository(unitOfWork).Add(DayOffTemplate);
				Given().ForEach(x => new ScheduleRepository(unitOfWork).Add(x));
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
				var rep = new ScheduleRepository(unitOfWork);
				var dictionary = rep.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(1800, 1, 1, 2040, 1, 1)),
																								 Scenario,
																								 new PersonProvider(new[] { Person }),
																								 new ScheduleDictionaryLoadOptions(true, true),
																								 new List<IPerson> { Person });
				return dictionary[Person];
			}
		}

		protected virtual bool ExpectOptimistLockException
		{
			get { return false; }
		}

		protected virtual IScheduleRangeConflictCollector ConflictCollector()
		{
			var currUnitOfWork = new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make());
			var scheduleRep = new ScheduleRepository(currUnitOfWork);
			return new ScheduleRangeConflictCollector(scheduleRep, new PersonAssignmentRepository(currUnitOfWork), this, new LazyLoadingManagerWrapper());
		}

		public abstract void ReassociateDataForAllPeople();
		public abstract void ReassociateDataFor(IPerson person);
	}
}