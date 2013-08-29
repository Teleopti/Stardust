using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class ScheduleMessageSenderTest
	{
		private IMessageSender target;
		private MockRepository mocks;
		private IServiceBusSender serviceBusSender;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			serviceBusSender = mocks.DynamicMock<IServiceBusSender>();
			target = new ScheduleMessageSender(serviceBusSender);
		}

		[Test]
		public void ShouldSendNotificationForPersistableScheduleData()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);

			using (mocks.Record())
			{
				Expect.Call(serviceBusSender.EnsureBus()).Return(true);
				Expect.Call(()=>serviceBusSender.Send(null)).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.Execute(null, new[] { rootChangeInfo });
			}
		}

		[Test]
		public void ShouldNotSendNotificationForNullScenario()
		{
			var person = PersonFactory.CreatePerson();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, null);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);

			using (mocks.Record())
			{
				Expect.Call(()=>serviceBusSender.Send(null)).IgnoreArguments().Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Execute(null, new[] { rootChangeInfo });
			}
		}
		
		[Test]
		public void ShouldNotSendNotificationForOtherType()
		{
			var person = PersonFactory.CreatePerson();
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(person, DomainUpdateType.Insert);

			using (mocks.Record())
			{
				Expect.Call(()=>serviceBusSender.Send(null)).IgnoreArguments().Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Execute(null, new[] { rootChangeInfo });
			}
		}

		[Test]
		public void ShouldNotSendNotificationForInternalNote()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var note = new Note(person,DateOnly.Today,scenario,"my note");
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(note, DomainUpdateType.Insert);

			using (mocks.Record())
			{
				Expect.Call(()=>serviceBusSender.Send(null)).IgnoreArguments().Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Execute(null, new[] { rootChangeInfo });
			}
		}

		[Test]
		public void ShouldNotSendNotificationForPublicNote()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var note = new PublicNote(person, DateOnly.Today, scenario, "my note");
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(note, DomainUpdateType.Insert);

			using (mocks.Record())
			{
				Expect.Call(()=>serviceBusSender.Send(null)).IgnoreArguments().Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Execute(null, new[] { rootChangeInfo });
			}
		}
	}
}