using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules.Concurrent
{
	public abstract class ScheduleRangeConcurrentTest : ScheduleRangePersisterBaseTest
	{
		protected abstract void WhenOtherIsChanging(IScheduleRange othersScheduleRange);
		protected abstract void WhenImChanging(IScheduleRange myScheduleRange);
		protected abstract void ThenOneRangeHadConflicts(IScheduleRange unsavedScheduleRangeWithConflicts,
			IEnumerable<PersistConflict> conflicts,
			bool myScheduleRangeWasTheOneWithConflicts);

		[Test]
		public void DoTheTestConcurrent()
		{
			var otherRange = LoadScheduleDictionary()[Person];
			var myRange = LoadScheduleDictionary()[Person];
			WhenOtherIsChanging(otherRange);
			WhenImChanging(myRange);

			var myTask = Task<IEnumerable<PersistConflict>>.Factory.StartNew(() => Target.Persist(myRange));
			var otherTask = Task<IEnumerable<PersistConflict>>.Factory.StartNew(() => Target.Persist(otherRange));
			Task.WaitAll(myTask, otherTask);

			var myConflicts = myTask.Result.ToList();
			var otherConflicts = otherTask.Result.ToList();

			if (myConflicts.Count > 0)
			{
				if (otherConflicts.Count > 0)
				{
					//both conflicts
					Assert.Fail("Both I and other got conflicts. Something wrong!");
				}
				//myconflict only
				ThenOneRangeHadConflicts(myRange, myConflicts, true);
			}
			else
			{
				if (otherConflicts.Count > 0)
				{
					//other conflicts only
					ThenOneRangeHadConflicts(otherRange, otherConflicts, false);
				}
				else
				{
					//no conflicts
					Assert.Fail("No conflicts. Something wrong!");
				}
			}
			GeneralAsserts(myRange, myConflicts);
			GeneralAsserts(otherRange, otherConflicts);
		}

		public override void ReassociateDataForAllPeople()
		{
			throw new NotImplementedException();
		}

		public override void ReassociateDataFor(IPerson person)
		{
			var uow = UnitOfWorkFactory.Current.CurrentUnitOfWork();
			new PersonRepository(uow).FindPeople(new[]{person.Id.Value});
			new ActivityRepository(uow).Get(Activity.Id.Value);
			new ShiftCategoryRepository(uow).Get(ShiftCategory.Id.Value);
			new ScenarioRepository(uow).Get(Scenario.Id.Value);
		}
	}
}