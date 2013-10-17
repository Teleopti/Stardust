using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			setupEntities();
			setupDatabase();
			makeTarget();
		}

		private void makeTarget()
		{
			Target = new ScheduleRangePersister(UnitOfWorkFactory.Current, 
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				new ScheduleRangeConflictCollector(), 
				new ScheduleRangeSaver(new ScheduleRepository(new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal())))));
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
				Given().ForEach(x => new ScheduleRepository(unitOfWork).Add(x));
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
				Given().ForEach(repository.Remove);
				unitOfWork.PersistAll();
			}
		}

		protected abstract IEnumerable<IPersistableScheduleData> Given();
		protected abstract IEnumerable<IPersistableScheduleData> When();
		protected abstract void Then(IEnumerable<PersistConflict> conflicts, IScheduleRange scheduleRange);

		[Test]
		public void DoTheTest()
		{
			var range = loadScheduleRange();
			range.AddRange(When());

			var result = Target.Persist(range);

			Then(result, range);
		}

		private ScheduleRange loadScheduleRange()
		{
			using (var unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new ScheduleRepository(unitOfWork);
				var dictionary = rep.FindSchedulesForPersons(new ScheduleDateTimePeriod(new DateTimePeriod(1800, 1, 1, 2040, 1, 1)),
																								 Scenario,
																								 new PersonProvider(new[] { Person }),
																								 new ScheduleDictionaryLoadOptions(false, false),
																								 new List<IPerson> { Person });
				return (ScheduleRange) dictionary[Person];
			}
		}
	}
}