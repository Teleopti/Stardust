using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	public class ConflictEagerLoadingTest : PersonAccountConflictTest
	{
		protected override bool GivenOtherHasChanged(IPersonAbsenceAccount othersPersonAccount)
		{
			othersPersonAccount.Remove(othersPersonAccount.AccountCollection().Single());
			return true;
		}

		protected override void WhenImChanging(IPersonAbsenceAccount myPersonAbsenceAccount)
		{
			((Domain.Scheduling.PersonalAccount.Account)myPersonAbsenceAccount.AccountCollection().Single()).LatestCalculatedBalance = new TimeSpan(20, 0, 0);
		}

		protected override void Then(IPersonAbsenceAccount inMemoryAndDatabasePersonAbsenceAccount)
		{
			LazyLoadingManager.IsInitialized(inMemoryAndDatabasePersonAbsenceAccount.AccountCollection());
		}
	}
}