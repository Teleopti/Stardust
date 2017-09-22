using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	public class UpdateConflictWithUpdateTest : PersonAccountConflictTest
	{
		protected override bool GivenOtherHasChanged(IPersonAbsenceAccount othersPersonAccount)
		{
			//in database 10 hours is used
			((Domain.Scheduling.PersonalAccount.Account)othersPersonAccount.AccountCollection().Single()).LatestCalculatedBalance = new TimeSpan(10, 0, 0);
			return true;
		}

		protected override void WhenImChanging(IPersonAbsenceAccount myPersonAbsenceAccount)
		{
			//in mem 20 hours is used
			((Domain.Scheduling.PersonalAccount.Account)myPersonAbsenceAccount.AccountCollection().Single()).LatestCalculatedBalance = new TimeSpan(20, 0, 0);
		}

		protected override void Then(IPersonAbsenceAccount inMemoryAndDatabasePersonAbsenceAccount)
		{
			//conflict! recalculate - no absence present -> 0
			inMemoryAndDatabasePersonAbsenceAccount.AccountCollection().Single().LatestCalculatedBalance
				.Should().Be.EqualTo(TimeSpan.Zero);
		}
	}
}