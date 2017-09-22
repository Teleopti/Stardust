using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	public class UpdateConflictWithDeleteAccountTest : PersonAccountConflictTest
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
			inMemoryAndDatabasePersonAbsenceAccount.AccountCollection().Should().Be.Empty();
		}
	}
}