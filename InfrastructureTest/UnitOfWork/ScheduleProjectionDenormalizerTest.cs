using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class ScheduleProjectionDenormalizerTest
	{
		private IMessageSender target;
		private MockRepository mocks;
		private ISendDenormalizeNotification sendDenormalizeNotification;
		private ISaveToDenormalizationQueue saveToDenormalizationQueue;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			sendDenormalizeNotification = mocks.DynamicMock<ISendDenormalizeNotification>();
			saveToDenormalizationQueue = mocks.DynamicMock<ISaveToDenormalizationQueue>();
			target = new ScheduleMessageSender(sendDenormalizeNotification,saveToDenormalizationQueue);
		}

		[Test]
		public void ShouldSendNotificationForPersistableScheduleData()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);

			var runSql = mocks.DynamicMock<IRunSql>();
			
			using (mocks.Record())
			{
				Expect.Call(sendDenormalizeNotification.Notify);
				
			}
			using (mocks.Playback())
			{
				target.Execute(runSql,new []{rootChangeInfo});
			}
		}

		[Test]
		public void ShouldNotSendNotificationForNullScenario()
		{
			var person = PersonFactory.CreatePerson();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, null);
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(personAssignment, DomainUpdateType.Insert);

			var runSql = mocks.DynamicMock<IRunSql>();

			using (mocks.Record())
			{
				Expect.Call(sendDenormalizeNotification.Notify).Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Execute(runSql, new[] { rootChangeInfo });
			}
		}
		
		[Test]
		public void ShouldNotSendNotificationForOtherType()
		{
			var person = PersonFactory.CreatePerson();
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(person, DomainUpdateType.Insert);

			var runSql = mocks.DynamicMock<IRunSql>();

			using (mocks.Record())
			{
				Expect.Call(sendDenormalizeNotification.Notify).Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Execute(runSql, new[] { rootChangeInfo });
			}
		}
	}
}