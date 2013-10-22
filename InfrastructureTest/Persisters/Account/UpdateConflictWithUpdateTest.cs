using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	public class UpdateConflictWithUpdateTest : PersonAccountPersisterBaseTest
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

		protected override void Then(IPersonAbsenceAccount myPersonAbsenceAccount)
		{
			//conflict! recalculate - no absence present -> 0
			myPersonAbsenceAccount.AccountCollection().Single().LatestCalculatedBalance
				.Should().Be.EqualTo(TimeSpan.Zero);
		}
	}
}