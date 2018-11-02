using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.InfrastructureTest.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks
{
	[TestFixture]
	[ScheduleDictionaryPersistTest]
	public class TransactionHookInvokationScheduleTest : IIsolateSystem
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

			Target.Persist(schedules);

			TransactionHook.ModifiedRoots.OfType<IPersonAssignment>().Should().Not.Be.Empty();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}