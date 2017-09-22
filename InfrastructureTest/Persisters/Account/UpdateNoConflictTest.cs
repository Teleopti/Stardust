using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	public class UpdateNoConflictTest : PersonAccountConflictTest
	{
		protected override bool GivenOtherHasChanged(IPersonAbsenceAccount othersPersonAccount)
		{
			return false;
		}

		protected override void WhenImChanging(IPersonAbsenceAccount myPersonAbsenceAccount)
		{
			myPersonAbsenceAccount.AccountCollection().Single().BalanceIn = TimeSpan.FromHours(11);
		}

		protected override void Then(IPersonAbsenceAccount inMemoryAndDatabasePersonAbsenceAccount)
		{
			inMemoryAndDatabasePersonAbsenceAccount.AccountCollection().Single().BalanceIn.Should().Be.EqualTo(TimeSpan.FromHours(11));
		}
	}
}