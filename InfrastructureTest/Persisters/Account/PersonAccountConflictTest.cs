using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	public abstract class PersonAccountConflictTest : PersonAccountPersisterBaseTest
	{
		protected abstract bool GivenOtherHasChanged(IPersonAbsenceAccount othersPersonAccount);
		protected abstract void WhenImChanging(IPersonAbsenceAccount myPersonAbsenceAccount);
		protected abstract void Then(IPersonAbsenceAccount inMemoryAndDatabasePersonAbsenceAccount);

		[Test]
		public void DoTheTest()
		{
			var theirAccounts = FetchPersonAccount();
			var myAccounts = FetchPersonAccount();

			if (GivenOtherHasChanged(theirAccounts.Single()))
			{
				Target.Persist(theirAccounts);
				theirAccounts.Should().Be.Empty();
			}

			var myAccount = myAccounts.Single();
			WhenImChanging(myAccount);
			Target.Persist(myAccounts);
			myAccounts.Should().Be.Empty();

			Then(myAccount);
			Then(FetchPersonAccount().Single());
		}
	}
}