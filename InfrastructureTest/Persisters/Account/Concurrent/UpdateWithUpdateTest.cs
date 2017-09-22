using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account.Concurrent
{
	public class UpdateWithUpdateTest : PersonAccountConcurrentTest
	{
		protected override void WhenOtherIsChanging(IPersonAbsenceAccount othersPersonAccount)
		{
			//in database 10 hours is used
			((Domain.Scheduling.PersonalAccount.Account)othersPersonAccount.AccountCollection().Single()).LatestCalculatedBalance = new TimeSpan(10, 0, 0);
		}

		protected override void WhenImChanging(IPersonAbsenceAccount myPersonAbsenceAccount)
		{
			//in mem 20 hours is used
			((Domain.Scheduling.PersonalAccount.Account)myPersonAbsenceAccount.AccountCollection().Single()).LatestCalculatedBalance = new TimeSpan(20, 0, 0);
		}

		protected override void ThenPersonAccountWithSolvedConflicts(IPersonAbsenceAccount personAccountWithSolvedConflicts)
		{
			personAccountWithSolvedConflicts.AccountCollection().Single().LatestCalculatedBalance
				.Should().Be.EqualTo(TimeSpan.Zero);
		}
	}
}