using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	public class UpdateNoConflictTest : PersonAccountPersisterIntegrationTest
	{
		protected override bool GivenOtherHasChanged(IPersonAbsenceAccount othersPersonAccount)
		{
			return false;
		}

		protected override void WhenImChanging(IPersonAbsenceAccount myPersonAbsenseAccount)
		{
			myPersonAbsenseAccount.AccountCollection().Single().BalanceIn = TimeSpan.FromHours(11);
		}

		protected override void Then(IPersonAbsenceAccount myPersonAbsenceAccount)
		{
			myPersonAbsenceAccount.AccountCollection().Single().BalanceIn.Should().Be.EqualTo(TimeSpan.FromHours(11));
		}
	}
}