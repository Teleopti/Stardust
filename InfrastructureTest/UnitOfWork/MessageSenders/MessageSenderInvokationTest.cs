using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.InfrastructureTest.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.MessageSenders
{
	[TestFixture]
	[Toggle(Toggles.MessageBroker_SchedulingScreenMailbox_32733)]
	[ScheduleDictionaryPersistTest]
	public class MessageSenderInvokationTest
	{
		public FakeUnitOfWorkMessageSender MessageSender;
		public IScheduleDictionaryPersister Target;
		public IScheduleDictionaryPersistTestHelper Helper;

		[Test]
		public void ShouldExecuteMessageSenders()
		{
			var person = Helper.NewPerson();
			var schedules = Helper.MakeDictionary();
			schedules[person]
				.ScheduledDay("2015-10-19".Date())
				.CreateAndAddActivity(
					Helper.Activity(),
					"2015-10-19 08:00 - 2015-10-19 17:00".Period())
				.ModifyDictionary();
			MessageSender.Clear();

			Target.Persist(schedules);

			MessageSender.ModifiedRoots.OfType<IPersonAssignment>().Should().Not.Be.Empty();
		}

	}
}