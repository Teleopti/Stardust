using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public abstract class ScheduleRangeConflictTest : ScheduleRangePersisterBaseTest
	{
		protected abstract void WhenOtherHasChanged(IScheduleRange othersScheduleRange);
		protected abstract void WhenImChanging(IScheduleRange myScheduleRange);
		protected abstract void Then(IEnumerable<PersistConflict> conflicts);
		protected abstract void Then(IScheduleRange myScheduleRange);

		[Test]
		public void DoTheTest()
		{
			var otherRange = LoadScheduleRange();
			WhenOtherHasChanged(otherRange);

			var myRange = LoadScheduleRange();
			Target.Persist(otherRange);

			WhenImChanging(myRange);

			if (ExpectOptimistLockException)
			{
				Assert.Throws<OptimisticLockException>(() => Target.Persist(myRange));
			}
			else
			{
				var conflicts = Target.Persist(myRange).PersistConflicts;
				Then(conflicts);
				Then(myRange);

				if (!conflicts.Any())
				{
					//if no conflicts, db version should be same as users schedulerange
					var canLoadAfterChangeVerifier = LoadScheduleRange();
					Then(canLoadAfterChangeVerifier);
				}
				GeneralAsserts(myRange, conflicts);
			}
		}

		public override void ReassociateDataForAllPeople()
		{
			throw new NotImplementedException();
		}

		public override void ReassociateDataFor(IPerson person)
		{
			var uow = UnitOfWorkFactory.Current.CurrentUnitOfWork();
			uow.Reassociate(person);
			uow.Reassociate(Activity);
			uow.Reassociate(ShiftCategory);
			uow.Reassociate(Scenario);
		}
	}
}