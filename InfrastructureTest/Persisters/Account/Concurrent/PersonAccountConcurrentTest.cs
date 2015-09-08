using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account.Concurrent
{
	public abstract class PersonAccountConcurrentTest : PersonAccountPersisterBaseTest
	{
		protected abstract void WhenOtherIsChanging(IPersonAbsenceAccount othersPersonAccount);
		protected abstract void WhenImChanging(IPersonAbsenceAccount myPersonAbsenceAccount);
		protected abstract void ThenPersonAccountWithSolvedConflicts(IPersonAbsenceAccount personAccountWithSolvedConflicts);

		[Test]
		public async void DoTheTest()
		{
			var otherAccounts = FetchPersonAccount();
			var myAccounts = FetchPersonAccount();

			var myAccount = myAccounts.Single();
			var otherAccount = otherAccounts.Single();

			WhenOtherIsChanging(otherAccount);
			WhenImChanging(myAccount);

			var myTask = Task<bool>.Factory.StartNew(() => Target.Persist(myAccounts));
			var otherTask = Task<bool>.Factory.StartNew(() => Target.Persist(otherAccounts));
			await Task.WhenAll(myTask, otherTask);

			var myHadConflicts = myTask.Result;
			var otherHadConflicts = otherTask.Result;

			myAccounts.Should().Be.Empty();
			otherAccounts.Should().Be.Empty();

			if (myHadConflicts)
			{
				if (otherHadConflicts)
				{
					Assert.Fail("Both my and other had conflicts. Something is very wrong!");
				}
				ThenPersonAccountWithSolvedConflicts(myAccount);
			}
			else
			{
				ThenPersonAccountWithSolvedConflicts(otherHadConflicts ? otherAccount : null);
			}
		}
	}
}