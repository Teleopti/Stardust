using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	[TestFixture]
	public abstract class ScheduleRangePersisterIntegrationTest : DatabaseTestWithoutTransaction
	{
		protected IPerson Person { get; private set; }
		protected IActivity Activity { get; private set; }
		protected IScenario Scenario { get; private set; }
		protected IShiftCategory ShiftCategory { get; private set; }
		private IScheduleRangePersister Target { get; set; }
		private IEnumerable<IPersistableScheduleData> _givenState;

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			setupEntities();
			setupDatabase();
			makeTarget();
		}

		private void makeTarget()
		{
			var scheduleRep = new ScheduleRepository(new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal())));
			Target = new ScheduleRangePersister(UnitOfWorkFactory.Current, 
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				new ScheduleRangeConflictCollector(new DifferenceEntityCollectionService<IPersistableScheduleData>(), scheduleRep), 
				new ScheduleRangeSaver(scheduleRep));
		}

		private void setupEntities()
		{
			Person = PersonFactory.CreatePerson("persist", "test");
			Activity = new Activity("persist test");
			ShiftCategory = new ShiftCategory("persist test");
			Scenario = new Scenario("scenario");
		}

		private void setupDatabase()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(unitOfWork).Add(Person);
				new ActivityRepository(unitOfWork).Add(Activity);
				new ShiftCategoryRepository(unitOfWork).Add(ShiftCategory);
				new ScenarioRepository(unitOfWork).Add(Scenario);
				_givenState = Given();
				_givenState.ForEach(x => new ScheduleRepository(unitOfWork).Add(x));
				unitOfWork.PersistAll();
			}
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new Repository(unitOfWork);
				repository.Remove(Person);
				repository.Remove(Activity);
				repository.Remove(ShiftCategory);
				repository.Remove(Scenario);
				foreach (var personAssignment in new PersonAssignmentRepository(unitOfWork).LoadAll())
				{
					repository.Remove(personAssignment);
				}
				unitOfWork.PersistAll();
			}
		}

		protected abstract IEnumerable<IPersistableScheduleData> Given();
		protected abstract IEnumerable<IScheduleDay> WhenI(IScheduleRange myScheduleRange);
		protected abstract IEnumerable<IScheduleDay> WhenOther(IScheduleRange othersScheduleRange);
		protected abstract void Then(IEnumerable<PersistConflict> conflicts, IScheduleRange scheduleRangeInMemory, IScheduleRange scheduleRangeInDatabase);


		[Test]
		public void DoTheTest()
		{
			var myDic = loadScheduleDictionary();
			var myRange = myDic[Person];
			myDic.Modify(ScheduleModifier.Scheduler, WhenI(myRange), NewBusinessRuleCollection.Minimum(), MockRepository.GenerateMock<IScheduleDayChangeCallback>(), MockRepository.GenerateMock<IScheduleTagSetter>());

			var otherDic = loadScheduleDictionary();
			var otherRange = otherDic[Person];
			otherDic.Modify(ScheduleModifier.Scheduler, WhenOther(otherRange), NewBusinessRuleCollection.Minimum(), MockRepository.GenerateMock<IScheduleDayChangeCallback>(), MockRepository.GenerateMock<IScheduleTagSetter>());
			Target.Persist(otherRange);

			var conflicts = Target.Persist(myRange);

			Then(conflicts, myRange, loadScheduleDictionary()[Person]);
			generalAsserts(myRange, conflicts);
		}

		private static void generalAsserts(IScheduleRange range, IEnumerable<PersistConflict> conflicts)
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

		private IScheduleDictionary loadScheduleDictionary()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new ScheduleRepository(unitOfWork);
				var dictionary = rep.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(1800, 1, 1, 2040, 1, 1)),
																								 Scenario,
																								 new PersonProvider(new[] { Person }),
																								 new ScheduleDictionaryLoadOptions(false, false),
																								 new List<IPerson> { Person });
				return dictionary;
			}
		}
	}
}