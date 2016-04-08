using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.InfrastructureTest.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.PersistCallbacks
{
	[TestFixture]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	[ScheduleDictionaryPersistTest]
	public class TransactionHookInvokationTest
	{
		public FakeTransactionHook TransactionHook;
		public IScheduleDictionaryPersister Target;
		public IScheduleDictionaryPersistTestHelper Helper;

		[Test]
		public void ShouldInvoke()
		{
			var person = Helper.NewPerson();
			var schedules = Helper.MakeDictionary();
			schedules[person]
				.ScheduledDay("2015-10-19".Date())
				.CreateAndAddActivity(
					Helper.Activity(),
					"2015-10-19 08:00 - 2015-10-19 17:00".Period())
				.ModifyDictionary();
			TransactionHook.Clear();

			Target.Persist(schedules);

			TransactionHook.ModifiedRoots.OfType<IPersonAssignment>().Should().Not.Be.Empty();
		}

	}
}