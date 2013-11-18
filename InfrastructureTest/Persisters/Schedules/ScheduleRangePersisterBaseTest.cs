using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
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
		protected IScheduleRangePersister Target { get; set; }
		protected IScheduleTag ScheduleTag { get; private set; }
		protected IDayOffTemplate DayOffTemplate { get; private set; }
		private IEnumerable<INonversionedPersistableScheduleData> _givenState;

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			setupEntities();
			setupDatabase();
			makeTarget();
		}

		private void makeTarget()
		{
			var currUnitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var scheduleRep = new ScheduleRepository(currUnitOfWork);
			Target = new ScheduleRangePersister(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()),
				new DifferenceEntityCollectionService<INonversionedPersistableScheduleData>(),
				ConflictCollector(),
				new ScheduleDifferenceSaver(scheduleRep),
				MockRepository.GenerateMock<IMessageBrokerIdentifier>());
		}

		private void setupEntities()
		{
			Person = PersonFactory.CreatePerson("persist", "test");
			Activity = new Activity("persist test");
			ShiftCategory = new ShiftCategory("persist test");
			Scenario = new Scenario("scenario");
			Absence = new Absence { Description = new Description("perist", "test") };
			DefinitionSet = new MultiplicatorDefinitionSet("persist test", MultiplicatorType.Overtime);
			ScheduleTag = new ScheduleTag { Description = "persist test" };
			DayOffTemplate = new DayOffTemplate(new Description("persist test"));
		}

		private void setupDatabase()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(unitOfWork).Add(Person);
				new ActivityRepository(unitOfWork).Add(Activity);
				new ShiftCategoryRepository(unitOfWork).Add(ShiftCategory);
				new ScenarioRepository(unitOfWork).Add(Scenario);
				new AbsenceRepository(unitOfWork).Add(Absence);
				new MultiplicatorDefinitionSetRepository(unitOfWork).Add(DefinitionSet);
				new ScheduleTagRepository(unitOfWork).Add(ScheduleTag);
				new DayOffTemplateRepository(unitOfWork).Add(DayOffTemplate);
				var scheduleDatas = new List<INonversionedPersistableScheduleData>();
				Given(scheduleDatas);
				_givenState = scheduleDatas;
				_givenState.ForEach(x => new ScheduleRepository(unitOfWork).Add(x));
				unitOfWork.PersistAll();
			}
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new Repository(unitOfWork);
				var schedRep = new ScheduleRepository(unitOfWork);
				foreach (var persistableScheduleData in schedRep.LoadAll())
				{
					repository.Remove(persistableScheduleData);
				}
				unitOfWork.PersistAll();
				unitOfWork.Clear();
				repository.Remove(Person);
				repository.Remove(ScheduleTag);
				repository.Remove(DefinitionSet);
				repository.Remove(Activity);
				repository.Remove(ShiftCategory);
				repository.Remove(Scenario);
				repository.Remove(DayOffTemplate);
				repository.Remove(Absence);
				unitOfWork.PersistAll();
			}
		}

		protected abstract void Given(ICollection<INonversionedPersistableScheduleData> scheduleDataInDatabaseAtStart);
		protected abstract void WhenOtherHasChanged(IScheduleRange othersScheduleRange);
		protected abstract void WhenImChanging(IScheduleRange myScheduleRange);
		protected abstract void Then(IEnumerable<PersistConflict> conflicts);
		protected abstract void Then(IScheduleRange myScheduleRange);


		[Test]
		public void DoTheTest()
		{
			var otherDic = loadScheduleDictionary();
			var otherRange = otherDic[Person];
			WhenOtherHasChanged(otherRange);

			var myDic = loadScheduleDictionary();
			Target.Persist(otherRange);

			var myRange = myDic[Person];
			WhenImChanging(myRange);

			if (ExpectOptimistLockException)
			{
				Assert.Throws<OptimisticLockException>(() => Target.Persist(myRange));
			}
			else
			{
				var conflicts = Target.Persist(myRange);
				Then(conflicts);
				Then(myRange);

				var canLoadAfterChangeDicVerifier = loadScheduleDictionary()[Person];
				if (!conflicts.Any())
				{
					//if no conflicts, db version should be same as users schedulerange
					Then(canLoadAfterChangeDicVerifier);
				}
				generalAsserts(myRange, conflicts);
			}
		}

		protected void DoModify(IScheduleDay scheduleDay)
		{
			scheduleDay.Owner.Modify(ScheduleModifier.Scheduler, scheduleDay, NewBusinessRuleCollection.Minimum(),
															 MockRepository.GenerateMock<IScheduleDayChangeCallback>(),
															 MockRepository.GenerateMock<IScheduleTagSetter>());
		}

		private static void generalAsserts(IScheduleRange range, IEnumerable<PersistConflict> conflicts)
		{
			var rangeDiff = range.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<INonversionedPersistableScheduleData>());

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
				ReassociateDataFor(Person);
				var rep = new ScheduleRepository(unitOfWork);
				var dictionary = rep.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(1800, 1, 1, 2040, 1, 1)),
																								 Scenario,
																								 new PersonProvider(new[] { Person }),
																								 new ScheduleDictionaryLoadOptions(true, true),
																								 new List<IPerson> { Person });
				return dictionary;
			}
		}

		public void ReassociateDataForAllPeople()
		{
			throw new NotImplementedException();
		}

		public void ReassociateDataFor(IPerson person)
		{
			var uow = UnitOfWorkFactory.Current.CurrentUnitOfWork();
			uow.Reassociate(person);
			uow.Reassociate(Activity);
			uow.Reassociate(ShiftCategory);
			uow.Reassociate(Scenario);
		}

		protected virtual bool ExpectOptimistLockException
		{
			get { return false; }
		}

		protected virtual IScheduleRangeConflictCollector ConflictCollector()
		{
			var currUnitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var scheduleRep = new ScheduleRepository(currUnitOfWork);
			return new ScheduleRangeConflictCollector(scheduleRep, new PersonAssignmentRepository(currUnitOfWork), this, new LazyLoadingManagerWrapper());
		}
	}
}